using System.Net;
using System.Text.Json;

namespace ChineseAuction.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string message;

            switch (exception)
            {
                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = "אינך מחובר או שה-Token אינו תקף";
                    break;

                case KeyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = "המשאב המבוקש לא נמצא";
                    break;

                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "נתונים שגויים בבקשה";
                    break;

                case InvalidOperationException:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "הפעולה אינה אפשרית במצב הנוכחי";
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "אירעה שגיאת שרת. נסה שוב מאוחר יותר";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                status = statusCode,
                error = message,
                // מומלץ להחזיר רק ב-Development
                details = exception.Message
            };

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            );
        }
    }
}
