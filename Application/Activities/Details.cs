using Application.Core;
using Application.Profiles;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities;

public class Details
{
    public class Query : IRequest<Result<ActivityDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ActivityDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }
        
        public async Task<Result<ActivityDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var activity = await _context.Activities
                .Include(a => a.Attendees)
                .ThenInclude(u => u.AppUser)
                .Select(x => new ActivityDto()
                {
                    Category = x.Category,
                    City = x.City,
                    Date = x.Date,
                    Description = x.Description,
                    HostUsername = x.Attendees.FirstOrDefault(y => y.IsHost).AppUser.UserName,
                    Id = x.Id,
                    Title = x.Title,
                    Venue = x.Venue,
                    Attendees = x.Attendees.Select(a => new Profile
                    {
                        Username = a.AppUser.UserName,
                        DisplayNAme = a.AppUser.DisplayName,
                        Bio = a.AppUser.Bio,
                    }).ToList()
                })
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
            return Result<ActivityDto>.Success(activity);
        }
    }
}