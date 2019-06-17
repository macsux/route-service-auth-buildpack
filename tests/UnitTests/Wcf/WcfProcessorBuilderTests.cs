using Pivotal.RouteService.Auth.Ingress.Buildpack;
using Pivotal.RouteService.Auth.Ingress.Buildpack.Wcf;
using System;
using System.IO;
using Xunit;

namespace UnitTests.Wcf
{
    public class WcfProcessorBuilderTests
    {
        string testFilePath = Path.Combine(Environment.CurrentDirectory, "config", "test.config");

        public WcfProcessorBuilderTests()
        {
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "config"));
        }

        [Fact]
        public void Test_IfProcessorBuilderBuildsWithAllRequiredExecutors()
        {
            File.WriteAllText(testFilePath, "<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration></configuration>");

            var builder = new WcfProcessorBuilder(testFilePath, string.Empty);
            var wcfProcessor = builder.Build();

            var detector = TestHelper.GetNonPublicInstanceFieldValue(wcfProcessor, "detector");
            var appender = TestHelper.GetNonPublicInstanceFieldValue(wcfProcessor, "fileAppender");
            var mover = TestHelper.GetNonPublicInstanceFieldValue(wcfProcessor, "assemblyMover");

            Assert.True(detector is ServiceDetector);
            Assert.True(appender is WebConfigFileAppender);
            Assert.True(mover is RequiredAssemblyMover);
        }
    }
}
