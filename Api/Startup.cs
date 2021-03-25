using System.Reflection.PortableExecutable;
using System.Text;
using Api.Middleware;
using Application.Activities;
using Application.Interfaces;
using Domain;
using FluentValidation.AspNetCore;
using Infrastructure.security;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace Api
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });

            // We will have a lot of handlers but we need to tell mediator once
            services.AddMediatR(typeof(List.Handler).Assembly);
            services.AddControllers(opt =>
           {
               var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
               opt.Filters.Add(new AuthorizeFilter(policy));

           }).AddFluentValidation(
              cfg =>
              {
                  cfg.RegisterValidatorsFromAssemblyContaining<Create>();
              }
            );

            // configure Identity
            var builder = services.AddIdentityCore<AppUser>();
            var identitybuilder = new IdentityBuilder(builder.UserType, builder.Services);
            identitybuilder.AddEntityFrameworkStores<DataContext>();
            identitybuilder.AddSignInManager<SignInManager<AppUser>>();

            services.AddScoped<IJwtGenerator, JwtGenerator>();
            services.AddScoped<IUserAccessor, UserAccessor>();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokenkey"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateAudience = false,
                    ValidateIssuer = false
                };
            });

            services.AddApiVersioning(config =>
            {
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.DefaultApiVersion = ApiVersion.Default;
                config.ReportApiVersions = true;
                config.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      //added custom middleware.
      app.UseMiddleware<ErrorHandlinMiddleware>();
      if (env.IsDevelopment())
      {
         // app.UseDeveloperExceptionPage();
      }

      app.UseHttpsRedirection();

      app.UseRouting();
      // use routing

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });

    }
    
  }
}
