using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

namespace StargateAPI.Controllers
{
    [Authorize]
    public class PersonController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public PersonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetPeople()
        {
            var result = await _mediator.Send(new GetPeople()
            {

            });

            return this.GetResponse(result);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            var result = await _mediator.Send(new GetPersonByName()
            {
                Name = name
            });

            return this.GetResponse(result);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            var result = await _mediator.Send(new CreatePerson()
            {
                Name = name
            });

            return this.GetResponse(result);
        }
    }
}