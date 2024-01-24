using Application.Core;
using Application.Profiles;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities;

public class List
{
    public class Query : IRequest<Result<List<ActivityDto>>> {}

    public class Handler : IRequestHandler<Query, Result<List<ActivityDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }
        
        public async Task<Result<List<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var activities = await _context.Activities
                .Include(a => a.Attendees)
                .ThenInclude(u => u.AppUser)
                .Select(x=> new ActivityDto()
                {
                    Category = x.Category,
                    City = x.City,
                    Date = x.Date,
                    Description = x.Description,
                    HostUsername = x.Attendees.FirstOrDefault(y=>y.IsHost).AppUser.UserName,
                    Id = x.Id,
                    Title = x.Title,
                    Venue = x.Venue,
                    Attendees = x.Attendees.Select(a => new Profile
                        {
                            Username = a.AppUser.UserName,
                            DisplayNAme = a.AppUser.DisplayName,
                            Bio = a.AppUser.Bio,
                        }).ToList()
                }).ToListAsync(cancellationToken);
            
            return Result<List<ActivityDto>>.Success(activities);
        }
    }
}