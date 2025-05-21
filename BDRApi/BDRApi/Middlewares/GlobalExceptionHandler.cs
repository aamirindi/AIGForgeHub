using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BDRApi.Middlewares
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            this._next = next;
            this._env = env;
            this._logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // continue to the next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // log the msg
                _logger.LogError(ex, ex.Message);

                // set the error statuscode to 500(internal server error) 
                context.Response.StatusCode = 500;

                // log the error in the json format
                context.Response.ContentType = "application/json";

                var response = new
                {
                    StatusCode = 500,
                    Message = _env.IsDevelopment() ? ex.Message : "An error has occured",
                    StackTrace = _env.IsDevelopment() ? ex.StackTrace : null
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}