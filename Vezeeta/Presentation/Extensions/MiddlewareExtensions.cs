using Presentation.Middlewares;

namespace Presentation.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            return app;
        }
    }
}
