using Application.Core;
using Application.Interfaces;
using Application.Profiles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Followers;

public class List
{
    public class Query : IRequest<Result<List<Profile>>>
    {
        public string Predicate { get; set; }
        public string Username { get; set; }
    }
    
    public class Handler : IRequestHandler<Query, Result<List<Profile>>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }
        
        public async Task<Result<List<Profile>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var profiles = new List<Profile>();

            switch (request.Predicate)
            {
                case "followers":
                    profiles = await _context.UserFollowings.Where(x => x.Target.UserName == request.Username)
                        .Select(u => u.Observer)
                        .Select(x => new Profile
                        {
                            Username = x.UserName,
                            DisplayName = x.DisplayName,
                            Bio = x.Bio,
                            Image = x.Photos.FirstOrDefault(p => p.IsMain).Url,
                            Photos = x.Photos,
                            Following = x.Followers.Any(u=>u.Observer.UserName == _userAccessor.GetUsername()),
                            FollowersCount = x.Followers.Count,
                            FollowingCount = x.Followings.Count
                        }).ToListAsync(cancellationToken);
                    break;
                case "followings":
                    profiles = await _context.UserFollowings.Where(x => x.Observer.UserName == request.Username)
                        .Select(u => u.Target)
                        .Select(x => new Profile
                        {
                            Username = x.UserName,
                            DisplayName = x.DisplayName,
                            Bio = x.Bio,
                            Image = x.Photos.FirstOrDefault(p => p.IsMain).Url,
                            Photos = x.Photos,
                            Following = x.Followers.Any(u=>u.Observer.UserName == _userAccessor.GetUsername()),
                            FollowersCount = x.Followers.Count,
                            FollowingCount = x.Followings.Count
                        }).ToListAsync(cancellationToken);
                    break;
            }

            return Result<List<Profile>>.Success(profiles);
        }
    }
}