namespace Pivotal.RouteService.Auth.Ingress.Buildpack.Wcf
{
    public class WcfProcessorBuilder
    {
        private readonly string webConfigPath;
        private readonly string appBinPath;

        public WcfProcessorBuilder(string webConfigPath, string appBinPath)
        {
            this.webConfigPath = webConfigPath;
            this.appBinPath = appBinPath;
        }

        public IProcessor Build()
        {
            return new GenericProcessor(new ServiceDetector(webConfigPath), 
                                        new WebConfigFileAppender(webConfigPath), 
                                        new RequiredAssemblyMover(typeof(Pivotal.RouteServiceAuthorizationPolicy.RouteServiceAuthorizationPolicy), appBinPath));
        }
    }
}
