using System.Net;
using System.Net.Cache;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using MediatR;
using Persistence;

namespace Application.Activities
{
    public class Delete
    {
                public class Command : IRequest
                {
                    public Guid id { get; set; }
                }
        
                public class Handler : IRequestHandler<Command>
                {
                    private readonly DataContext _context;
                    public Handler(DataContext context)
                    {
                        this._context = context;
        
                    }
                    //Mediator unit is an empty unit/object
                    public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
                    {
                // handler logic
                var activity = await _context.Activities.FindAsync(request.id);
                
                if(activity ==null) throw new RestException(HttpStatusCode.NotFound, 
                 "Not found"
                );

                _context.Remove(activity);

                var success = await _context.SaveChangesAsync() > 0;
        
                       if(success) return Unit.Value;
        
                        throw new Exception("Prolem saving Changes");
                    }
                }
        
    }
}