using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Api.Database;
using Api.Domain.Core;
using Api.Infrastructure;
using Google.Apis.Auth;
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
    public async Task<IActionResult> Register(RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var identityUser = new IdentityUser
        {
            UserName = request.Username,
            Email = request.Email
        };
        var result = await userManager.CreateAsync(identityUser, request.Password);

        if (result.Succeeded)
        {
            var userId = await usersContext.Users.AsNoTracking()
                .Where(user => user.UserName == request.Username)
                .Select(user => user.Id)
                .SingleAsync(cancellationToken);

            await mediator.Send(new CreateFamilyMemberCommand(new CreateFamilyMemberCommandModel(
                request.FirstName,
                request.LastName,
                request.Birthdate,
                userId)), cancellationToken);

            request.Password = string.Empty;
            return CreatedAtAction(nameof(Register), new { request.Email }, request);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticationResponse>> Authentication([FromBody] AuthenticationRequest request, CancellationToken cancellationToken = default)
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

        var userId = await usersContext.Users.AsNoTracking()
            .Where(user => user.NormalizedEmail == managedUser.NormalizedEmail)
            .Select(user => user.Id)
            .SingleAsync(cancellationToken);

        var familyMemberId = await applicationDbContext.FamilyMembers.AsNoTracking()
            .Where(fm => fm.AspNetUserId == userId)
            .Select(fm => fm.Id)
            .SingleOrDefaultAsync(cancellationToken);

        await userManager.AddClaimAsync(managedUser, new Claim(ApplicationClaimNames.CurrentFamilyMemberId, familyMemberId.ToString(), nameof(Guid)));

        await userManager.ResetAccessFailedCountAsync(managedUser);
        var accessToken = tokenService.CreateToken(managedUser, familyMemberId);
        await usersContext.SaveChangesAsync(cancellationToken);

        return Ok(new AuthenticationResponse
        {
            Username = managedUser.UserName!,
            Email = managedUser.Email!,
            Token = accessToken
        });
    }

    public async Task<IActionResult> LinkGoogleAccount(string idToken, CancellationToken cancellationToken = default)
    {+
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        if (payload == null)
        {
            return BadRequest("Invalid Google token.");
        }
        
        // TODO
        
        var userId = payload.Subject;
        var email = payload.Email;
        
        // TODO

        return Ok("Account linked");
    }
}

public class AuthenticationRequest
{
    public string Login { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class AuthenticationResponse
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Token { get; set; } = default!;
}

public class RegistrationRequest
{
    [Required]
    public string FirstName { get; set; } = default!;
    
    [Required]
    public string LastName { get; set; } = default!;
    
    [Required]
    public DateTime Birthdate { get; set; }

    [Required]
    public string Email { get; set; } = default!;

    [Required]
    public string Username { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}