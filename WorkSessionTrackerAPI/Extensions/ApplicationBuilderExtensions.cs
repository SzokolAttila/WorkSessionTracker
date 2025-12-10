using Microsoft.AspNetCore.Builder;
using WorkSessionTrackerAPI.Middleware;

namespace WorkSessionTrackerAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
