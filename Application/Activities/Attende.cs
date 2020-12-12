using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Domain;

namespace Application.Activities
{
    public class Attend
    {
        public class Command : IRequest
        {
        public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }
            //Mediator unit is an empty unit/object
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {

                var activities = await _context.Activities.FindAsync(request.Id);

                // TODO add error handler
                if(activities == null)
                    throw new Exception("Not found");

                var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetCurrentUsername());

                var attendance = await _context.UserActivities.SingleOrDefaultAsync(x => x.ActivityId == activities.Id && x.AppUserId == user.Id);

                // TODO add error handler
                if(attendance != null)
                    throw new Exception("not attending, bad request");

                attendance = new UserActivity
                {
                    Activity = activities,
                    AppUser = user,
                    IsHost = false,
                    DateJoined = DateTime.Now
                };

                // handler logic
                var success = await _context.SaveChangesAsync() > 0;

                if(success) return Unit.Value;

                throw new Exception("Prolem saving Changes");
            }
        }

    }
}