using Microsoft.AspNetCore.Builder;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for providing a virtual js with antiforgery token.
    /// This is for use in SPA content.
    /// </summary>
    public static class AntiforgeryScriptMiddlewareExtensions
    {
        /// <summary>
        /// Enables the antiforgery script that will inject
        /// the hidden token field in html body. This is highly experimental.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">Optional routine for setting up script options.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAntiforgeryScript(
            this IApplicationBuilder builder,
            Action<AntiforgeryScriptOptions> configureOptions = null)
        {
            var options = new AntiforgeryScriptOptions();
            configureOptions?.Invoke(options);

            return builder.Map(options.RequestPath, app => app.UseMiddleware<AntiforgeryScriptMiddleware>(options));
        }
    }
}
