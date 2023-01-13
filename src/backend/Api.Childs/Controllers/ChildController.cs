using Api.Childs.Features.Child.Commands.Create;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Childs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public ChildController(
            IMapper mapper,
            IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateChild([FromBody] CreateChildRequest request, CancellationToken cancellationToken = default)
        {
            var command = _mapper.Map<CreateChildCommand>(request);
            var id = await _mediator.Send(command, cancellationToken);
            if (id is not null)
            {
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }

            return BadRequest(ModelState);
        }

        [HttpGet("{id:guid}")]
        public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
