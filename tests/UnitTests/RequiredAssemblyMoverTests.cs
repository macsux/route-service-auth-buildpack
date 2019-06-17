using Pivotal.RouteService.Auth.Ingress.Buildpack;
using Pivotal.RouteService.Auth.Ingress.Buildpack.Wcf;
using System;
using System.IO;
using Xunit;

namespace UnitTests
{
    public class RequiredAssemblyMoverTests
    {
        string testTargetPath = Path.Combine(Environment.CurrentDirectory, "targetBin");

        public RequiredAssemblyMoverTests()
        {
            Directory.CreateDirectory(testTargetPath);
        }

        [Fact]
        public void Test_IfAllRequiredAssembliesAreMovedToTheTargetBinFolder()
        {
            var mover = new RequiredAssemblyMover(typeof(RequiredAssemblyMoverTests), testTargetPath);
            mover.Move();

            Assert.True(File.Exists(Path.Combine(testTargetPath, "UnitTests.dll")));
        }
    }
}
