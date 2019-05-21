using System;
using System.Security.Claims;
using System.Web;

namespace Pivotal.RouteServiceIdentityModule
{
    public class RouteServiceIdentityModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += ContextOnAuthenticateRequest;
            
        }

        private void ContextOnAuthenticateRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication) sender).Context;
            var identityHeader = context.Request.Headers.Get("X-Cf-Identity");
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