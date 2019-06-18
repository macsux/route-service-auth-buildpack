using System;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Security.Principal;
using System.Web;

namespace Pivotal.RouteServiceAuthorizationPolicy
{
    public class RouteServiceAuthorizationPolicy : IAuthorizationPolicy
    {
        readonly string id;

        public RouteServiceAuthorizationPolicy()
        {
            id = Guid.NewGuid().ToString();
        }

        public ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        public string Id
        {
            get { return id; }
        }

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            try
            {
                evaluationContext.Properties["Principal"] = HttpContext.Current.User;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
