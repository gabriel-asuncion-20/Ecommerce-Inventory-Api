using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EcommerceInventoryApi.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió una excepción no controlada: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var statusCode = HttpStatusCode.InternalServerError;
            var title = "Ocurrió un error interno en el servidor.";
            var detail = exception.Message;

            switch (exception)
            {
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    title = "Recurso no encontrado.";
                    break;
                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    title = "Operación no válida.";
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    title = "No autorizado.";
                    break;
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    title = "Argumento no válido.";
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new
            {
                type = $"https://httpstatuses.com/{(int)statusCode}",
                title = title,
                status = (int)statusCode,
                detail = detail,
                instance = context.Request.Path.Value,
                timestamp = DateTime.UtcNow
            };

            var jsonResult = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(jsonResult);
        }
    }
}
