using System;
using System.Security.Claims;
using System.Web;

namespace Pivotal.RouteServiceIdentityModule
{
    public class RouteServiceIdentityModule : IHttpModule
    {
        const string CF_IDENTITY_HEADER = "X-Cf-Identity";
        const string CF_IMPERSONATED_IDENTITY_HEADER = "X-Cf-ImpersonatedIdentity";


        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += ContextOnAuthenticateRequest;
        }

        private void ContextOnAuthenticateRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication) sender).Context;

            var identityHeader = context.Request.Headers.Get(CF_IMPERSONATED_IDENTITY_HEADER) 
                                    ?? context.Request.Headers.Get(CF_IDENTITY_HEADER);

            if (identityHeader != null)
            {
                var nameClaim = new Claim(ClaimTypes.Name, identityHeader);
                var identity = new ClaimsIdentity(new[] { nameClaim }, "RouteService");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        public void Dispose()
        {
        }
    }
}