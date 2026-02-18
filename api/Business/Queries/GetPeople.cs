using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StargateContext _context;
        public GetPeopleHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            var result = new GetPeopleResult();

            var people = await _context.People
                .AsNoTracking()
                .Select(person => new PersonAstronaut
                {
                    PersonId = person.Id,
                    Name = person.Name,
                    CurrentRank = person.AstronautDetail != null ? person.AstronautDetail.CurrentRank : string.Empty,
                    CurrentDutyTitle = person.AstronautDetail != null ? person.AstronautDetail.CurrentDutyTitle : string.Empty,
                    CareerStartDate = person.AstronautDetail != null ? person.AstronautDetail.CareerStartDate : null,
                    CareerEndDate = person.AstronautDetail != null ? person.AstronautDetail.CareerEndDate : null
                })
                .ToListAsync(cancellationToken);

            result.People = people;

            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };

    }
}
