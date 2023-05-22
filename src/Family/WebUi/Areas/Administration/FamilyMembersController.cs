using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebUi.Areas.Administration.FamilyMembers;
using WebUi.Areas.Administration.FamilyMembers.Detail;

namespace WebUi.Areas.Administration
{
    [Area(AreaNames.Administration)]
    [Route(Routes.AreaRouteTemplate)]
    public class FamilyMembersController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public FamilyMembersController(
            IMediator mediator,
            IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        private Task<TResponse> ExecuteAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(request, cancellationToken);
        }

        private async Task<IActionResult> HandleGetAsync<TResponse>(IRequest<TResponse> request, Func<TResponse, IActionResult> func, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteAsync(request, cancellationToken);
            return func(response);
        }
        
        public Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            return HandleGetAsync(
                new FamilyMembers.Index.Get(),
                View,
                cancellationToken);
        }

        [HttpGet("{id:int}")]
        public Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
        {
            return HandleGetAsync(
                new FamilyMembers.Detail.Get(id),
                View,
                cancellationToken);
        }

        private async Task<IActionResult> HandlePostAsync<TResponse>(
            IRequest<TResponse> request,
            Func<ValidationState, IActionResult> func,
            CancellationToken cancellationToken = default)
        where TResponse : new()
        {
            try
            {
                await ExecuteAsync(request, cancellationToken);
                return func(ValidationState.Success);
            }
            catch (ValidationException validationException)
            {
                foreach (var validationFailure in validationException.Errors)
                {
                    ModelState.AddModelError(validationFailure.PropertyName, validationFailure.ErrorMessage);
                }
                
                return func(ValidationState.Failure);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] FamilyMembers.Detail.ViewModel viewModel, CancellationToken cancellationToken = default)
        {
            var x = await HandlePostAsync(
                new FamilyMembers.Detail.Update(viewModel),
                state => state switch
                {
                    ValidationState.Success => View("Index"),
                    ValidationState.Failure => View("Detail", _mapper.Map<ViewModel>(viewModel)),
                    _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
                },
                cancellationToken);

            return x;
        }

        [HttpPost("{id:int}")]
        public IActionResult Delete(int id)
        {
            return View(ViewNames.Index);
        }
    }

    public enum ValidationState
    {
        Success,
        Failure
    }
}
