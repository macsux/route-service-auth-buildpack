namespace Pivotal.RouteService.Auth.Ingress.Buildpack.Identity
{
    public class IdentityProcessorBuilder
    {
        private readonly string webConfigPath;
        private readonly string appBinPath;

        public IdentityProcessorBuilder(string webConfigPath, string appBinPath)
        {
            this.webConfigPath = webConfigPath;
            this.appBinPath = appBinPath;
        }

        public IProcessor Build()
        {
            return new GenericProcessor(new DummyDetector(), 
                                        new WebConfigFileAppender(webConfigPath), 
                                        new RequiredAssemblyMover(typeof(Pivotal.RouteServiceIdentityModule.RouteServiceIdentityModule), appBinPath));
        }
    }
}
