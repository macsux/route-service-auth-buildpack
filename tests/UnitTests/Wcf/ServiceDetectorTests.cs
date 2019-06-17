using Pivotal.RouteService.Auth.Ingress.Buildpack.Wcf;
using System;
using System.IO;
using Xunit;

namespace UnitTests.Wcf
{
    public class ServiceDetectorTests
    {
        string testFilePath = Path.Combine(Environment.CurrentDirectory, "config", "test_web.config");

        string serviceExistsConfigText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration><system.serviceModel><services><service name=\"WithSvcBehaviourAndEndPointBehaviour\" ><endpoint address=\"\" binding=\"basicHttpBinding\" contract=\"WcfService.IService\" bindingConfiguration=\"noSecurity\" /></service></services> </system.serviceModel></configuration>";
        string serviceNotExistsConfigText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration>  <system.serviceModel></system.serviceModel></configuration>";

        public ServiceDetectorTests()
        {
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "config"));
        }

        [Fact]
        public void Test_IfDetectorReturnsTrueIfWcfServiceExists()
        {
            File.WriteAllText(testFilePath, serviceExistsConfigText);
            var detector = new ServiceDetector(testFilePath);
            Assert.True(detector.Find());
        }

        [Fact]
        public void Test_IfDetectorReturnsFalseIfWcfServiceSectionDoesNotExists()
        {
            File.WriteAllText(testFilePath, serviceNotExistsConfigText);
            var detector = new ServiceDetector(testFilePath);
            Assert.False(detector.Find());
        }
    }
}
