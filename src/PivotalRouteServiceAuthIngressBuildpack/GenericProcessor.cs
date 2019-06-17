namespace Pivotal.RouteService.Auth.Ingress.Buildpack
{
    public class GenericProcessor : IProcessor
    {
        private readonly IDetector detector;
        private readonly IConfigFileAppender fileAppender;
        private readonly IAssemblyMover assemblyMover;

        public GenericProcessor(IDetector detector, IConfigFileAppender fileAppender, IAssemblyMover assemblyMover)
        {
            this.detector = detector;
            this.fileAppender = fileAppender;
            this.assemblyMover = assemblyMover;
        }

        public void Execute()
        {
            if(detector.Find())
            {
                using (fileAppender)
                    fileAppender.Execute();

                assemblyMover.Move();
            }
        }
    }
}
