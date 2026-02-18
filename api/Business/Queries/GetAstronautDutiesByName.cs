using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            var result = new GetAstronautDutiesByNameResult();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                result.Success = false;
                result.ResponseCode = (int)HttpStatusCode.BadRequest;
                result.Message = "Name is required.";
                return result;
            }

            var person = await _context.People
                .AsNoTracking()
                .Where(p => p.Name == request.Name)
                .Select(p => new PersonAstronaut
                {
                    PersonId = p.Id,
                    Name = p.Name,
                    CurrentRank = p.AstronautDetail != null ? p.AstronautDetail.CurrentRank : string.Empty,
                    CurrentDutyTitle = p.AstronautDetail != null ? p.AstronautDetail.CurrentDutyTitle : string.Empty,
                    CareerStartDate = p.AstronautDetail != null ? p.AstronautDetail.CareerStartDate : null,
                    CareerEndDate = p.AstronautDetail != null ? p.AstronautDetail.CareerEndDate : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (person is null)
            {
                result.Success = false;
                result.ResponseCode = (int)HttpStatusCode.NotFound;
                result.Message = "Person not found.";
                return result;
            }

            result.Person = person;

            var duties = await _context.AstronautDuties
                .AsNoTracking()
                .Where(duty => duty.PersonId == person.PersonId)
                .OrderByDescending(duty => duty.DutyStartDate)
                .ToListAsync(cancellationToken);

            result.AstronautDuties = duties;

            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
