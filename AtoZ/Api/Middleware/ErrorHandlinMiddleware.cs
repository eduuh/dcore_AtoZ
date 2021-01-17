using System.Net;
using System;
using System.Threading.Tasks;
using Application.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Api.Middleware
{
    public class ErrorHandlinMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlinMiddleware> _logger;

        public ErrorHandlinMiddleware(RequestDelegate next, ILogger<ErrorHandlinMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task Invoke(HttpContext context){
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {

                await HandlerExceptionAsync(context, ex, _logger);
            }
        }

        private async Task HandlerExceptionAsync(HttpContext context, Exception ex, ILogger<ErrorHandlinMiddleware> logger)
        {
            object errors = null;
            
            switch(ex){
                case RestException re:
                    logger.LogError(ex, "REST ERROR");
                    errors = re.Errors;
                    context.Response.StatusCode = (int)re.Code;
                    break;
                case Exception e:
                    logger.LogError(e, "SERVER ERROR");
                    errors = string.IsNullOrWhiteSpace(e.Message) ? "Error" : e.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }
            context.Response.ContentType = "application/json";
            if(errors !=null){
                var result = JsonSerializer.Serialize(new
                {
                    errors
                });

              await context.Response.WriteAsync(result);
            }

        }
    }
}