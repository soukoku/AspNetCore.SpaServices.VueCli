using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    class AntiforgeryScriptMiddleware
    {
        private readonly IAntiforgery _antiforgery;
        private readonly IOptions<AntiforgeryOptions> _antiforgeryOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly AntiforgeryScriptOptions _scriptOptions;

        public AntiforgeryScriptMiddleware(RequestDelegate _,
            AntiforgeryScriptOptions scriptOptions,
            IAntiforgery antiforgery,
            IOptions<AntiforgeryOptions> options,
            IServiceProvider serviceProvider)
        {
            _scriptOptions = scriptOptions;
            _antiforgery = antiforgery;
            _antiforgeryOptions = options;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_scriptOptions.AllowWhen != null &&
                !_scriptOptions.AllowWhen(context, _serviceProvider))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
            if (context.Request.Method != HttpMethods.Get)
            {
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                return;
            }

            var tokenSet = _antiforgery.GetTokens(context);

            if (!string.IsNullOrEmpty(tokenSet.CookieToken))
            {
                var cookieOp = _antiforgeryOptions.Value.Cookie.Build(context);
                context.Response.Cookies.Append(_antiforgeryOptions.Value.Cookie.Name, tokenSet.CookieToken, cookieOp);
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
}
