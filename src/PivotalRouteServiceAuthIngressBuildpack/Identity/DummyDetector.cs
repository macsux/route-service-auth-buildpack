using System;
using System.Xml;

namespace Pivotal.RouteService.Auth.Ingress.Buildpack.Identity
{
    public class DummyDetector : IDetector
    {
        public DummyDetector()
        {
        }

        public bool Find()
        {
            return true;
        }
    }
}
