using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Application.Comments;
using System.Linq;
using System.Security.Claims;

namespace Api
{
    public class ChatHub : Hub
    {
        private readonly IMediator mediator;

        public ChatHub(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task SendComment(Create.Command command) {
            var username = Context.User?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            command.Username = username;

            var comment = await mediator.Send(command);

            await Clients.All.SendAsync("ReceiveComment", comment);
        }
    }
}
