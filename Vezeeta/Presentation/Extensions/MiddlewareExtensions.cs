using Presentation.Middlewares;

namespace Presentation.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            return app;
        }
    }
}
