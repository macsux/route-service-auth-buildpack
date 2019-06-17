using Pivotal.RouteService.Auth.Ingress.Buildpack.Identity;
using Pivotal.RouteService.Auth.Ingress.Buildpack.Wcf;
using System;
using System.IO;
using Xunit;

namespace UnitTests.Identity
{
    public class DummyDetectorTests
    {
        [Fact]
        public void Test_IfDetectorAlwaysReturnsTrue()
        {
            var detector = new DummyDetector();
            Assert.True(detector.Find());
        }
    }
}
