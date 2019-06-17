using Pivotal.RouteService.Auth.Ingress.Buildpack;
using Pivotal.RouteService.Auth.Ingress.Buildpack.Identity;
using System;
using System.IO;
using Xunit;

namespace UnitTests.Identity
{
    public class IdentityProcessorBuilderTests
    {
        string testFilePath = Path.Combine(Environment.CurrentDirectory, "config", "test.config");

        public IdentityProcessorBuilderTests()
        {
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "config"));
        }

        [Fact]
        public void Test_IfProcessorBuilderBuildsWithAllRequiredExecutors()
        {
            File.WriteAllText(testFilePath, "<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration></configuration>");

            var builder = new IdentityProcessorBuilder(testFilePath, string.Empty);
            var wcfProcessor = builder.Build();

            var detector = TestHelper.GetNonPublicInstanceFieldValue(wcfProcessor, "detector");
            var appender = TestHelper.GetNonPublicInstanceFieldValue(wcfProcessor, "fileAppender");
            var mover = TestHelper.GetNonPublicInstanceFieldValue(wcfProcessor, "assemblyMover");

            Assert.True(detector is DummyDetector);
            Assert.True(appender is WebConfigFileAppender);
            Assert.True(mover is RequiredAssemblyMover);
        }
    }
}
