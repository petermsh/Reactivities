using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments;

public class Create
{
    public class Command : IRequest<Result<CommentDto>>
    {
        public string Body { get; set; }
        public Guid ActivityId { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Body).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, Result<CommentDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }
        
        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var activity = await _context.Activities
                .FindAsync(request.ActivityId);

            if (activity is null) return null;

            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);

            var comment = new Comment
            {
                Author = user,
                Activity = activity,
                Body = request.Body
            };

            activity.Comments.Add(comment);

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (success)
                return Result<CommentDto>.Success(new CommentDto()
                {
                    Id=comment.Id,
                    Body = comment.Body,
                    DisplayName = comment.Author.DisplayName,
                    Username = comment.Author.UserName,
                    Image = comment.Author.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                    CreatedAt = comment.CreatedAt
                });

            return Result<CommentDto>.Failure("Failed to add comment");
        }
    }
}