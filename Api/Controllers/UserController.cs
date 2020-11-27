using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Application.User;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class UserController : BaseController
    {
        [HttpPost("login")]
        public async Task<ActionResult<User>> login(Login.Query query){
            return await Mediator.Send(query);
        }
    }
}