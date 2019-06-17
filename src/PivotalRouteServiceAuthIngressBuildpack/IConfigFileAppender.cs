using System;

namespace Pivotal.RouteService.Auth.Ingress.Buildpack
{
    public interface IConfigFileAppender : IDisposable
    {
        void Execute();
    }
}