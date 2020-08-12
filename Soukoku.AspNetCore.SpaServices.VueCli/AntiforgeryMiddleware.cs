using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
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

            // writes the hidden token field if on a page with same origin as script
            var bodyScript = $@"(function appendToken(){{
if(window.location.origin==='{GetOrigin(context.Request)}'){{
if(document.body){{
var input = document.createElement('input')
input.setAttribute('type', 'hidden')
input.setAttribute('name', '{tokenSet.FormFieldName}')
input.setAttribute('value', '{tokenSet.RequestToken}')
document.body.appendChild(input)
}}else{{window.requestAnimationFrame(appendToken)}}
}}}})()";

            await context.Response.WriteAsync(bodyScript, Encoding.UTF8);
        }

        static string GetOrigin(HttpRequest request)
        {
            return new Uri(request.GetDisplayUrl()).GetLeftPart(UriPartial.Authority);
        }
    }

    /// <summary>
    /// Extension methods for providing a virtual js with antiforgery token.
    /// This is for use in SPA content.
    /// </summary>
    public static class AntiforgeryMiddlewareExtensions
    {
        /// <summary>
        /// Enables the antiforgery script that will inject
        /// the hidden token field in html body. This is highly experimental.
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
