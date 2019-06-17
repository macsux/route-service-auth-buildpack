using Pivotal.RouteService.Auth.Ingress.Buildpack;
using Pivotal.RouteService.Auth.Ingress.Buildpack.Wcf;
using System;
using System.IO;
using XmlDiffLib;
using Xunit;

namespace UnitTests.Wcf
{
    public class WebConfigFileAppenderTests
    {
        string originalWebConfigPathTemplate = Path.Combine(Environment.CurrentDirectory, "Wcf", "GivenWcfConfigurationFile{0}.config");
        string expectedWebConfigPath = Path.Combine(Environment.CurrentDirectory, "Wcf", "ExpectedWcfConfigurationFile.config");

        [Fact]
        public void Test_AppliesNecessaryConfigurationToWcfServiceSection()
        {
            var originalWebConfigPath = string.Format(originalWebConfigPathTemplate, "Test");

            File.Copy(string.Format(originalWebConfigPathTemplate, string.Empty), originalWebConfigPath, true);

            using (var appender = new WebConfigFileAppender(originalWebConfigPath))
                appender.Execute();

            var expectedWebConfig = File.ReadAllText(expectedWebConfigPath);
            var appendedWebConfig = File.ReadAllText(originalWebConfigPath);

            var diff = new XmlDiff(expectedWebConfig, appendedWebConfig);

            diff.CompareDocuments(new XmlDiffOptions() { IgnoreAttributeOrder = true, IgnoreCase = true, TrimWhitespace = true });

            Assert.Empty(diff.DiffNodeList);
        }

        [Fact]
        public void Test_WillNotSaveTheFileIfNotExecutedAsDisposable()
        {
            var originalWebConfigPath = string.Format(originalWebConfigPathTemplate, "Test");

            File.Copy(string.Format(originalWebConfigPathTemplate, string.Empty), originalWebConfigPath, true);

            var appender = new WebConfigFileAppender(originalWebConfigPath);
            appender.Execute();

            var expectedWebConfig = File.ReadAllText(expectedWebConfigPath);
            var appendedWebConfig = File.ReadAllText(originalWebConfigPath);

            var diff = new XmlDiff(expectedWebConfig, appendedWebConfig);

            diff.CompareDocuments(new XmlDiffOptions() { IgnoreAttributeOrder = true, IgnoreCase = true, TrimWhitespace = true });

            Assert.NotEmpty(diff.DiffNodeList);
        }
    }
}
