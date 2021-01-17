using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.User
{
    public class Login
    {
        public class Query : IRequest<User>
        {
            public string Email { get; set; }
            public string Password { get; set; }

        }
        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.Email).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Query, User>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly SignInManager<AppUser> _signinmanager;
            private readonly IJwtGenerator _jwtgenerator;
            public Handler(UserManager<AppUser> userManager, SignInManager<AppUser> signinmanager, IJwtGenerator jwtgenerator)
            {
                _jwtgenerator = jwtgenerator;
                _signinmanager = signinmanager;
                _userManager = userManager;

            }

            public async Task<User> Handle(Query request, CancellationToken cancellationToken)
            {
                AppUser user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null) throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not Authorized " });

                var result = await _signinmanager.CheckPasswordSignInAsync(user, request.Password, false);


                if (result.Succeeded)
                {
                    return new User
                    {
                        DispalyName = user.DisplayName,
                        Token = _jwtgenerator.createToken(user),
                        Username = user.UserName,
                        Image = null
                    };

                }
                throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not Authorized"});


            }
        }

    }
}