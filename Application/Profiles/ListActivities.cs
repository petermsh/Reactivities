using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles;

public class ListActivities
{
    public class Query : IRequest<Result<List<UserActivityDto>>>
    {
        public string Username { get; set; }
        public string Predicate { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<UserActivityDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<UserActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.ActivityAttendees
                .Where(u => u.AppUser.UserName == request.Username)
                .OrderBy(a => a.Activity.Date)
                .Select(x=> new UserActivityDto
                {
                    Id = x.Activity.Id,
                    Category = x.Activity.Category,
                    Date = x.Activity.Date,
                    HostUsername = x.Activity.Attendees.FirstOrDefault(y=>y.IsHost).AppUser.UserName,
                    Title = x.Activity.Title
                }).AsQueryable();
            
            query = request.Predicate switch
            {
                "past" => query.Where(a => a.Date <= DateTime.Now),
                "hosting" => query.Where(a => a.HostUsername == request.Username),
                _ => query.Where(a => a.Date >= DateTime.Now)
            };
            
            var activities = await query.ToListAsync();
            return Result<List<UserActivityDto>>.Success(activities);
        }
    }
}