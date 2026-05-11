using Application.Presentation.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Application.Presentation.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSharedMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            return app;
        }
    }
}
