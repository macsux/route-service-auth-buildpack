using Pivotal.RouteService.Auth.Ingress.Buildpack.Identity;
using System;
using System.IO;
using XmlDiffLib;
using Xunit;

namespace UnitTests.Identity
{
    public class WebConfigFileAppenderTests
    {
        string originalWebConfigPathTemplate = Path.Combine(Environment.CurrentDirectory, "Identity" , "GivenIdentityConfigurationFile{0}.config");
        string originalWebConfigPathNonEmptyTemplate = Path.Combine(Environment.CurrentDirectory, "Identity" , "GivenIdentityConfigurationFileNonEmpty{0}.config");
        string expectedWebConfigPath = Path.Combine(Environment.CurrentDirectory, "Identity", "ExpectedIdentityConfigurationFile.config");
        string expectedWebConfigPathModuleNonEmpty = Path.Combine(Environment.CurrentDirectory, "Identity", "ExpectedIdentityConfigurationFileNonEmpty.config");

        [Fact]
        public void Test_AppliesNecessaryConfigurationToModulesIfModuleIsEmpty()
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
        public void Test_AppliesNecessaryConfigurationToModulesIfModuleIsNotEmpty()
        {
            var originalWebConfigPath = string.Format(originalWebConfigPathNonEmptyTemplate, "Test");

            File.Copy(string.Format(originalWebConfigPathNonEmptyTemplate, string.Empty), originalWebConfigPath, true);

            using (var appender = new WebConfigFileAppender(originalWebConfigPath))
                appender.Execute();

            var expectedWebConfig = File.ReadAllText(expectedWebConfigPathModuleNonEmpty);
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
