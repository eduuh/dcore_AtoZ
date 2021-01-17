using System.Net;
using System;
using System.Threading.Tasks;
using Application.Errors;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Serilog;

namespace Api.Middleware
{
    public class ErrorHandlinMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlinMiddleware(RequestDelegate next) 
        {
            _next = next;
        }
        
        public async Task Invoke(HttpContext context){
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {

             await HandlerExceptionAsync(context, ex);
            }
        }

        private async Task HandlerExceptionAsync(HttpContext context, Exception ex)
        {
            object errors = null;
            
            switch(ex){
                case RestException re:
                    Log.Error("REST ERROR");
                    errors = re.Errors;
                    context.Response.StatusCode = (int)re.Code;
                    break;
                case Exception e:
                    Log.Error("SERVER ERROR");
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