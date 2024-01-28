using Application.Core;
using Application.Interfaces;
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
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
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
                    Attendees = x.Attendees.Select(a => new AttendeeDto
                    {
                        Username = a.AppUser.UserName,
                        DisplayName = a.AppUser.DisplayName,
                        Bio = a.AppUser.Bio,
                        Image = a.AppUser.Photos.FirstOrDefault(p=>p.IsMain).Url,
                        Following = a.AppUser.Followers.Any(u=>u.Observer.UserName == _userAccessor.GetUsername()),
                        FollowersCount = a.AppUser.Followers.Count,
                        FollowingCount = a.AppUser.Followings.Count
                    }).ToList()
                })
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
            return Result<ActivityDto>.Success(activity);
        }
    }
}