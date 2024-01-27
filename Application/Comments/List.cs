using System.Reflection.Metadata;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments;

public class List
{
    public class Query : IRequest<Result<List<CommentDto>>>
    {
        public Guid ActivityId { get; set; }
    }
    
    public class Handler : IRequestHandler<Query, Result<List<CommentDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }
        
        public async Task<Result<List<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var comments = await _context.Comments
                .Where(x => x.Activity.Id == request.ActivityId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CommentDto
                {
                    Id = x.Id,
                    Body = x.Body,
                    DisplayName = x.Author.DisplayName,
                    Username = x.Author.UserName,
                    Image = x.Author.Photos.FirstOrDefault(p => p.IsMain).Url,
                    CreatedAt = x.CreatedAt,
                }).ToListAsync(cancellationToken);

            return Result<List<CommentDto>>.Success(comments);
        }
    }
}