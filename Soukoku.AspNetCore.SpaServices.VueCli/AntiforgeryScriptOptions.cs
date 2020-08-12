using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Options for the antiforgery script middleware.
    /// </summary>
    public class AntiforgeryScriptOptions
    {
        /// <summary>
        /// Path where the script is served on.
        /// Defaults to "/js/antiforgery".
        /// </summary>
        public string RequestPath { get; set; } = "/js/antiforgery";

        /// <summary>
        /// Optional delegate to determine if script should be allowed per request.
        /// </summary>
        public Func<HttpContext, IServiceProvider, bool> AllowWhen { get; set; }


        /// <summary>
        /// A delegate for use with <see cref="AllowWhen"/> that checks
        /// if request is authenticated.
        /// </summary>
        /// <returns></returns>
        public static readonly Func<HttpContext, IServiceProvider, bool> RequireAuthentication
            = (ctx, _) => ctx.User.Identity.IsAuthenticated;

    }
}
