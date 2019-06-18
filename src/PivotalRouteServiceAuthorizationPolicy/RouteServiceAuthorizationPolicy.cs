using System;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
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
                Console.WriteLine($"Current HttpContext User: {HttpContext.Current.User}");
                evaluationContext.Properties["Principal"] = HttpContext.Current.User;
                Console.WriteLine($"Current Thread Principal User: {evaluationContext.Properties["Principal"]}");
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
