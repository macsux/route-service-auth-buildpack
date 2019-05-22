using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Pivotal.RouteServiceAuthorizationPolicy;
using Pivotal.RouteServiceIdentityModule;

namespace RouteServiceAuthenticationBuildpack
{
    public class RouteServiceAuthenticationBuildpack : SupplyBuildpack
    {

        protected override bool Detect(string buildPath)
        {
            return File.Exists(Path.Combine(buildPath, "web.config"));
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            var webConfigPath = Path.Combine(buildPath, "web.config");

            if (!File.Exists(webConfigPath))
                return;

            AddIdentityModule(buildPath, webConfigPath);

            AddAuthorizationPolicyForWcfServices(webConfigPath, buildPath);
        }

        private static void AddIdentityModule(string buildPath, string webConfigPath)
        {
            var doc = new XmlDocument();
            doc.Load(webConfigPath);

            if (doc.SelectSingleNode("configuration/system.webServer/modules/add[@name=\"RouteServiceIdentityModule\"]") == null)
            {
                var modules = (XmlElement)doc.SelectSingleNode("configuration/system.webServer/modules");
                if (modules == null)
                {
                    var webServerNode = (XmlElement)doc.SelectSingleNode("configuration/system.webServer");
                    if (webServerNode == null)
                    {
                        webServerNode = doc.CreateElement("system.webServer");
                        var configNode = (XmlElement)doc.SelectSingleNode("configuration");
                        configNode.AppendChild(webServerNode);
                    }

                    modules = doc.CreateElement("modules");
                    webServerNode.AppendChild(modules);
                }


                modules.SetAttribute("runAllManagedModulesForAllRequests", "true");
                var routeServiceModuleNode = doc.CreateElement("add");
                routeServiceModuleNode.SetAttribute("name", nameof(RouteServiceIdentityModule));
                routeServiceModuleNode.SetAttribute("type", typeof(RouteServiceIdentityModule).AssemblyQualifiedName);
                modules.AppendChild(routeServiceModuleNode);
                doc.Save(webConfigPath);
            }

            var assemblyDll = typeof(RouteServiceIdentityModule).Assembly.Location;
            var targetFileName = Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll));

            if (!File.Exists(targetFileName))
                File.Copy(assemblyDll, Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll)));
        }

        private void AddAuthorizationPolicyForWcfServices(string webConfigPath, string buildPath)
        {
            var doc = new XmlDocument();
            doc.Load(webConfigPath);

            if (doc.SelectSingleNode("configuration/system.serviceModel/services") != null)
            {
                Console.WriteLine("Detected WCF Service application..");

                var serviceModel = doc.SelectSingleNode("configuration/system.serviceModel");

                var behaviours = (XmlElement)doc.SelectSingleNode("configuration/system.serviceModel/behaviors");

                if (behaviours == null)
                {
                    behaviours = doc.CreateElement("behaviors");
                    serviceModel.AppendChild(behaviours);
                }

                var serviceBehaviours = (XmlElement)doc.SelectSingleNode("configuration/system.serviceModel/behaviors/serviceBehaviors");

                if (serviceBehaviours == null)
                {
                    serviceBehaviours = doc.CreateElement("serviceBehaviors");
                    behaviours.AppendChild(serviceBehaviours);
                }

                var individualBehaviours = doc.SelectNodes("//configuration/system.serviceModel/behaviors/serviceBehaviors/behavior");

                if (individualBehaviours.Count == 0)
                {
                    var individualBehaviour = doc.CreateElement("behavior");
                    individualBehaviour.SetAttribute("name", "customAuthBehaviour");
                    serviceBehaviours.AppendChild(individualBehaviour);

                    individualBehaviour.AppendChild(CreateServiceAuthorizationElement(doc));

                    var services = doc.SelectNodes("configuration/system.serviceModel/services/service");

                    for (int i = 0; i < services.Count; i++)
                    {
                        var behaviourConfigurationAttribute = doc.CreateAttribute("behaviorConfiguration");
                        behaviourConfigurationAttribute.Value = "customAuthBehaviour";
                        services.Item(i).Attributes.Append(behaviourConfigurationAttribute);
                    }
                }
                else
                {
                    for (int i = 0; i < individualBehaviours.Count; i++)
                    {
                        individualBehaviours.Item(i).AppendChild(CreateServiceAuthorizationElement(doc));
                    }
                }

                doc.Save(webConfigPath);

                var assemblyDll = typeof(RouteServiceAuthorizationPolicy).Assembly.Location;
                var targetFileName = Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll));

                if (!File.Exists(targetFileName))
                    File.Copy(assemblyDll, Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll)));
            }
        }

        private XmlElement CreateServiceAuthorizationElement(XmlDocument xmlDoc)
        {
            var serviceAuthorization = xmlDoc.CreateElement("serviceAuthorization");

            var principalPermissionModeAttribute = xmlDoc.CreateAttribute("principalPermissionMode");
            principalPermissionModeAttribute.Value = "Custom";
            serviceAuthorization.Attributes.Append(principalPermissionModeAttribute);

            var authPolicies = xmlDoc.CreateElement("authorizationPolicies");
            serviceAuthorization.AppendChild(authPolicies);

            var policyNode = xmlDoc.CreateElement("add");
            policyNode.SetAttribute("policyType", typeof(RouteServiceAuthorizationPolicy).AssemblyQualifiedName);
            authPolicies.AppendChild(policyNode);

            return serviceAuthorization;
        }
    }
}
