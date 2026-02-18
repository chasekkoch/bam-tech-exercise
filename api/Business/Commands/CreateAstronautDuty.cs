using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    internal static class CreateAstronautDutyHelper
    {
        internal static DateTime NormalizeToUtcDate(DateTime value)
        {
            var utc = value.Kind switch
            {
                DateTimeKind.Local => value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => value
            };

            return DateTime.SpecifyKind(utc.Date, DateTimeKind.Utc);
        }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name)
                || string.IsNullOrWhiteSpace(request.Rank)
                || string.IsNullOrWhiteSpace(request.DutyTitle))
            {
                throw new BadHttpRequestException("Name, rank, and duty title are required.");
            }

            if (request.DutyStartDate == default)
            {
                throw new BadHttpRequestException("Duty start date is required.");
            }

            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is null) throw new BadHttpRequestException("Bad Request");

            var dutyStartDate = CreateAstronautDutyHelper.NormalizeToUtcDate(request.DutyStartDate);

            var verifyNoPreviousDuty = _context.AstronautDuties
                .FirstOrDefault(z => z.PersonId == person.Id
                    && z.DutyTitle == request.DutyTitle
                    && z.DutyStartDate == dutyStartDate);

            if (verifyNoPreviousDuty is not null) throw new BadHttpRequestException("Bad Request");

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var result = new CreateAstronautDutyResult();
            var isRetired = string.Equals(request.DutyTitle, "RETIRED", StringComparison.OrdinalIgnoreCase);
            var dutyStartDate = CreateAstronautDutyHelper.NormalizeToUtcDate(request.DutyStartDate);

            var person = await _context.People
                .FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);

            if (person is null)
            {
                result.Success = false;
                result.ResponseCode = (int)HttpStatusCode.NotFound;
                result.Message = "Person not found.";
                return result;
            }

            var astronautDetail = await _context.AstronautDetails
                .FirstOrDefaultAsync(detail => detail.PersonId == person.Id, cancellationToken);

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail
                {
                    PersonId = person.Id,
                    CurrentDutyTitle = request.DutyTitle,
                    CurrentRank = request.Rank,
                    CareerStartDate = dutyStartDate,
                    CareerEndDate = isRetired ? dutyStartDate.AddDays(-1) : null
                };

                await _context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);

            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerEndDate = isRetired ? dutyStartDate.AddDays(-1) : null;
                _context.AstronautDetails.Update(astronautDetail);
            }

            var currentDuty = await _context.AstronautDuties
                .Where(duty => duty.PersonId == person.Id && duty.DutyEndDate == null)
                .OrderByDescending(duty => duty.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentDuty != null)
            {
                currentDuty.DutyEndDate = dutyStartDate.AddDays(-1);
                _context.AstronautDuties.Update(currentDuty);
            }

            var newAstronautDuty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = dutyStartDate,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            result.Id = newAstronautDuty.Id;
            return result;
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
