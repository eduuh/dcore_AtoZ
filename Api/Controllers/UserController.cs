using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Application.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class UserController : BaseController
    {
        [HttpPost("login")]
        public async Task<ActionResult<User>> login(Login.Query query){
            return await Mediator.Send(query);
        }
        [HttpPost("register")]
        public async Task<ActionResult<User>> login(Register.Command command){
            return await Mediator.Send(command);
        }
        [HttpGet]
        public async Task<ActionResult<User>> CurrentUser(){
            return await Mediator.Send(new CurrentUser.Query());
        }


        // you would need to include a query string to your url to specify the version
        //?api-version-2.0
        [HttpGet]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<User>> CurrentUser2(){
            return await Mediator.Send(new CurrentUser.Query());
        }
    }
}
