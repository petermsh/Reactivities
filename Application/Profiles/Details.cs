using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles;

public class Details
{
    public class Query : IRequest<Result<Profile>>
    {
        public string Username { get; set; }
    }
    
    public class Handler : IRequestHandler<Query, Result<Profile>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }
        
        public async Task<Result<Profile>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(p => p.Photos)
                .Select(x => new Profile
                {
                    Username = x.UserName,
                    DisplayName = x.DisplayName,
                    Bio = x.Bio,
                    Image = x.Photos.FirstOrDefault(p=>p.IsMain).Url,
                    Photos = x.Photos,
                    Following = x.Followers.Any(u=>u.Observer.UserName == _userAccessor.GetUsername()),
                    FollowersCount = x.Followers.Count,
                    FollowingCount = x.Followings.Count
                }).FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

            return Result<Profile>.Success(user);
        }
    }
}