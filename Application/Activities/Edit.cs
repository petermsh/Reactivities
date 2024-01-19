using Application.Core;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Activities;

public class Edit
{
    public class Command : IRequest<Result<Unit>>
    {
        public Activity Activity { get; set; }
    }
    
    public class CommandValidator : AbstractValidator<Create.Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
        }
    }
    
    
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }
        
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var activity = await _context.Activities.FindAsync(request.Activity.Id);

            if (activity is null) return null;
            
            activity.Title = request.Activity.Title ?? activity.Title;
            activity.Category = request.Activity.Category ?? activity.Category;
            activity.City = request.Activity.City ?? activity.City;
            activity.Description = request.Activity.Description ?? activity.Description;
            activity.Venue = request.Activity.Venue ?? activity.Venue;
            
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            return !result ? Result<Unit>.Failure("Failed to update activity") : Result<Unit>.Success(Unit.Value);
        }
    }
}