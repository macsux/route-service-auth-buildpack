using System;
using System.IO;

namespace Pivotal.RouteService.Auth.Ingress.Buildpack
{
    public class RequiredAssemblyMover : IAssemblyMover
    {
        private readonly Type containingType;
        private readonly string appBinPath;

        public RequiredAssemblyMover(Type containingType, string appBinPath)
        {
            this.containingType = containingType;
            this.appBinPath = appBinPath;
        }

        public void Move()
        {
            var assemblyDll = containingType.Assembly.Location;
            var targetFileName = Path.Combine(appBinPath, Path.GetFileName(assemblyDll));

            Console.WriteLine($"-----> Injecting {containingType.FullName} assembly into the {appBinPath} directory...");

            if (!File.Exists(targetFileName))
                File.Copy(assemblyDll, targetFileName);
        }
    }
}
