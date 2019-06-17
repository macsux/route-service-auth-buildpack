using System;

namespace Pivotal.RouteService.Auth.Ingress.Buildpack
{
    public class Program
    {
        static int Main(string[] args)
        {
            return new IngressBuildpack().Run(args);
        }
    }
}