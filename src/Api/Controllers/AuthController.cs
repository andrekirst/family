using System.Security.Claims;
using Api.Database;
using Api.Domain.Core.Authentication;
using Api.Domain.Core.Authentication.Google;
using Api.Extensions;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationDbContext applicationDbContext,
    UsersContext usersContext,
    ITokenService tokenService,
    IMediator mediator)
    : ControllerBase
{
    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var identityUser = new IdentityUser
        {
            UserName = request.Username,
            Email = request.EMail
        };
        var result = await userManager.CreateAsync(identityUser, request.Password);

        if (result.Succeeded)
        {
            var userId = await usersContext.Users
                .AsNoTracking()
                .Where(user => user.UserName == request.Username)
                .Select(user => user.Id)
                .SingleAsync(cancellationToken);

            await mediator.Send(request.ToCreateFamilyMemberCommand(userId), cancellationToken);

            request.InvalidatePassword();
            return Created();
        }

        ModelState.AddIdentityModelErrors(result.Errors);

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var managedUser = await userManager.FindByEmailAsync(request.Login) ??
                          await userManager.FindByNameAsync(request.Login);

        if (managedUser == null)
        {
            return BadRequest("Bad credentials");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(managedUser, request.Password);
        if (!isPasswordValid)
        {
            await userManager.AccessFailedAsync(managedUser);
            return BadRequest("Bad credentials");
        }

        var userInDb = await usersContext.Users.AnyAsync(user => user.NormalizedEmail == managedUser.NormalizedEmail, cancellationToken);
        if (!userInDb)
        {
            return Unauthorized();
        }

        var familyMemberId = await applicationDbContext.FamilyMembers
            .AsNoTracking()
            .Where(fm => fm.AspNetUserId == managedUser.Id)
            .Select(fm => fm.Id)
            .SingleOrDefaultAsync(cancellationToken);

        await userManager.AddClaimAsync(managedUser, new Claim(ApplicationClaimNames.CurrentFamilyMemberId, familyMemberId.ToString(), nameof(Guid)));

        await userManager.ResetAccessFailedCountAsync(managedUser);
        var accessToken = tokenService.CreateToken(managedUser, familyMemberId);
        await usersContext.SaveChangesAsync(cancellationToken);

        return Ok(new LoginResponse
        {
            Id = managedUser.Id,
            Username = managedUser.UserName!,
            Email = managedUser.Email!,
            Token = accessToken
        });
    }

    [HttpPost, Route("google-login"), AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        var command = new GoogleLoginCommand(request);
        var result = await mediator.Send(command, cancellationToken);

        return result
            ? Ok()
            : BadRequest();
    }
}