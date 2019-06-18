using System;
using System.Security.Claims;
using System.Web;
using System.Linq;

namespace Pivotal.RouteServiceIdentityModule
{
    public class RouteServiceIdentityModule : IHttpModule
    {
        const string CF_IDENTITY_HEADER = "X-Cf-Identity";
        const string CF_IMPERSONATED_IDENTITY_HEADER = "X-Cf-Impersonated-Identity";


        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += ContextOnAuthenticateRequest;
        }

        private void ContextOnAuthenticateRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication) sender).Context;

            var isImpersonatedUserHeaderExists = context.Request.Headers.AllKeys.Contains(CF_IMPERSONATED_IDENTITY_HEADER);

            var identityHeader = isImpersonatedUserHeaderExists
                                    ? context.Request.Headers.Get(CF_IMPERSONATED_IDENTITY_HEADER) 
                                        : context.Request.Headers.Get(CF_IDENTITY_HEADER);

            if (!String.IsNullOrWhiteSpace(identityHeader))
            {
                var nameClaim = new Claim(ClaimTypes.Name, identityHeader);
                var identity = new ClaimsIdentity(new[] { nameClaim }, isImpersonatedUserHeaderExists ? "RouteService-Impersonated" : "RouteService");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        public void Dispose()
        {
        }
    }
}