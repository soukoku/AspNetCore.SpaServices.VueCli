using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    class AntiforgeryMiddleware
    {
        private readonly IAntiforgery antiforgery;
        private readonly IOptions<AntiforgeryOptions> options;

        public AntiforgeryMiddleware(RequestDelegate next,
            IAntiforgery antiforgery, IOptions<AntiforgeryOptions> options)
        {
            this.antiforgery = antiforgery;
            this.options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method != HttpMethods.Get)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var tokenSet = antiforgery.GetTokens(context);

            if (!string.IsNullOrEmpty(tokenSet.CookieToken))
            {
                var cookieOp = options.Value.Cookie.Build(context);
                context.Response.Cookies.Append(options.Value.Cookie.Name, tokenSet.CookieToken, cookieOp);
            }
            context.Response.ContentType = "application/javascript; charset=utf-8";
            context.Response.Headers["Cache-control"] = "no-store";
            context.Response.Headers["Pragma"] = "no-cache";

            var script = $"window['{tokenSet.FormFieldName}']=(function(){{token='{tokenSet.RequestToken}';return {{get:function(){{return token}}}}}})();";
            await context.Response.WriteAsync(script);
        }
    }

    /// <summary>
    /// Extension methods for providing a virtual js with antiforgery token.
    /// This is for use in SPA content.
    /// </summary>
    public static class AntiforgeryMiddlewareExtensions
    {
        /// <summary>
        /// Enables the antiforgery script. This is highly experimental.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAntiforgeryScript(
            this IApplicationBuilder builder,
            string path = "/js/antiforgery")
        {
            return builder.Map(path, app => app.UseMiddleware<AntiforgeryMiddleware>());
        }
    }
}
