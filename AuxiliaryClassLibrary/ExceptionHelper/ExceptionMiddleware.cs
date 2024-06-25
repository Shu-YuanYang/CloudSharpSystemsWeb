using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Net.Mime;
using System.Text.Json;
using System.Security.Authentication;

namespace AuxiliaryClassLibrary.ExceptionHelper
{
    // Reference https://levelup.gitconnected.com/two-different-approaches-for-global-exception-handling-in-asp-net-core-web-api-f815c27b1e2d
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly IHostEnvironment _env;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }
        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Shu-Yuan Yang 20240520 added to identify authentication/authorization errors.
            switch (exception) {
                case InvalidCredentialException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }
            
            var response = new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = _env.IsDevelopment() ?
                    (exception.Message + "\n" + exception.StackTrace?.ToString())
                    :
                    (exception.Message)
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}
