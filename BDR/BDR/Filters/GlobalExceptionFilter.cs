using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;

namespace BillApi.Services
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, context.Exception.Message);

            if (context.Exception is SecurityTokenException)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Invalid token. Please log in again.",
                    error = _env.IsDevelopment() ? context.Exception.Message : null
                });
            }
            else if (context.Exception is UnauthorizedAccessException)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Access denied. Token may be missing or invalid.",
                    error = _env.IsDevelopment() ? context.Exception.Message : null
                });
            }
            else
            {
                context.Result = new ObjectResult(new
                {
                    message = "An unexpected error occurred.",
                    error = _env.IsDevelopment() ? context.Exception.Message : null
                })
                {
                    StatusCode = 500
                };
            }

            context.ExceptionHandled = true;
        }
    }
}
