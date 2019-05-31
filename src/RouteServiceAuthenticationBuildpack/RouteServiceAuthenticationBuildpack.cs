using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Pivotal.RouteServiceAuthorizationPolicy;
using Pivotal.RouteServiceIdentityModule;
using Pivotal.RouteServiceIwaWcfInterceptor;

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
            Console.WriteLine("================================================================================");
            Console.WriteLine("============= Route Service Iwa Auth Buildpack execution started ===============");
            Console.WriteLine("================================================================================");

            var webConfigPath = Path.Combine(buildPath, "web.config");

            if (!File.Exists(webConfigPath))
            {
                Console.WriteLine("-----> Web.config file not found, so skipping ececution...");
                return;
            }

            AddIdentityModule(buildPath, webConfigPath);

            AddAuthorizationPolicyForWcfServices(webConfigPath, buildPath);

            AddClientInterceptorForWcfClients(webConfigPath, buildPath);

            Console.WriteLine("================================================================================");
            Console.WriteLine("============= Route Service Iwa Auth Buildpack execution completed =============");
            Console.WriteLine("================================================================================");
        }

        private static void AddIdentityModule(string buildPath, string webConfigPath)
        {
            var doc = new XmlDocument();
            doc.Load(webConfigPath);

            if (doc.SelectSingleNode("configuration/system.webServer/modules/add[@name=\"RouteServiceIdentityModule\"]") == null)
            {
                Console.WriteLine("-----> Applying configuration changes to add RouteServiceIdentityModule in the request pipeline...");

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

            Console.WriteLine("-----> Injecting RouteServiceIdentityModule assembly into the application target directory...");

            var assemblyDll = typeof(RouteServiceIdentityModule).Assembly.Location;
            var targetFileName = Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll));

            if (!File.Exists(targetFileName))
                File.Copy(assemblyDll, Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll)));
        }

        private void AddAuthorizationPolicyForWcfServices(string webConfigPath, string buildPath)
        {
            var doc = new XmlDocument();
            doc.Load(webConfigPath);

            Console.WriteLine("-----> Checking for WCF Service application...");

            var services = doc.SelectSingleNode("configuration/system.serviceModel/services");

            if (services != null)
            {
                Console.WriteLine("-----> Detected WCF Service application");
                Console.WriteLine("-----> Applying configuration changes to add RouteServiceAuthorizationPolicy into the pipeline...");

                var serviceModel = doc.SelectSingleNode("configuration/system.serviceModel");

                var behaviours = GetOrCreateBehavioursSection(doc, serviceModel);

                var serviceBehaviours = GetOrCreateServiceBehaviours(doc, behaviours);

                var individualBehaviours = serviceBehaviours.SelectNodes("behavior");

                var pivotaWcfServiceIwaAuthBehaviourExists = PivotalServiceBehaviourExistsAlready(individualBehaviours);

                if (!pivotaWcfServiceIwaAuthBehaviourExists)
                    CreatePivotalServiceBehaviour(doc, serviceBehaviours);

                if (individualBehaviours.Count == 0)
                    SetPivotaWcfServiceIwaAuthBehaviourConfigurationToAllServices(doc, services);
                else
                    AddAuthorizationElementToPreExistingBehaviours(doc, individualBehaviours);

                doc.Save(webConfigPath);

                CopyAuthorizationPolicyAssemblyToAppBinPath(buildPath);

                ValidateIfAllServicesAreSetWithBehaviourConfiguration(services);
            }
        }

        private void AddClientInterceptorForWcfClients(string webConfigPath, string buildPath)
        {
            var doc = new XmlDocument();
            doc.Load(webConfigPath);

            Console.WriteLine("-----> Checking for WCF Client application...");

            var client = doc.SelectSingleNode("configuration/system.serviceModel/client");

            var svcEndpointLevelBehaviours = new List<string>();
            var clientEndpointLevelBehaviours = new List<string>();

            if (client != null)
            {
                Console.WriteLine("-----> Detected WCF Client in this application");

                ValidateIfPivotalWcfClientInterceptorPackageIsInstalled(buildPath);

                Console.WriteLine("-----> Applying configuration changes to add RouteServiceIwaWcfInterceptor, from nuget package PivotalServices.WcfClient.Kerberos.Interceptor into the egress pipeline...");

                AddAllExistingSvcEndpointBehaviours(doc, svcEndpointLevelBehaviours);

                var endpoints = client.SelectNodes("endpoint");

                AddAllExistingClientEndpointBehaviours(clientEndpointLevelBehaviours, endpoints);

                var serviceModel = doc.SelectSingleNode("configuration/system.serviceModel");

                var behaviours = GetOrCreateBehavioursSection(doc, serviceModel);

                var endpointBehaviours = GetOrCreateEndPointBehaviours(doc, behaviours);

                var individualBehaviours = endpointBehaviours.SelectNodes("behavior");

                bool pivotalWcfClientIwaInterceptorBehaviourExists = false;

                for (int i = 0; i < individualBehaviours.Count; i++)
                {
                    if (individualBehaviours.Item(i).Attributes["name"].Value == "pivotalWcfClientIwaInterceptorBehaviour")
                        pivotalWcfClientIwaInterceptorBehaviourExists = true;
                }

                if (!pivotalWcfClientIwaInterceptorBehaviourExists)
                {
                    var pivotalWcfClientIwaInterceptorBehaviour = doc.CreateElement("behavior");
                    pivotalWcfClientIwaInterceptorBehaviour.SetAttribute("name", "pivotalWcfClientIwaInterceptorBehaviour");
                    pivotalWcfClientIwaInterceptorBehaviour.AppendChild(AddInterceptorExtension(doc));
                    endpointBehaviours.AppendChild(pivotalWcfClientIwaInterceptorBehaviour);
                }

                if (individualBehaviours.Count == 0)
                {
                    var endPoints = client.SelectNodes("endpoint");

                    for (int i = 0; i < endPoints.Count; i++)
                    {
                        var behaviourConfigurationAttribute = doc.CreateAttribute("behaviorConfiguration");
                        behaviourConfigurationAttribute.Value = "pivotalWcfClientIwaInterceptorBehaviour";
                        endPoints.Item(i).Attributes.Append(behaviourConfigurationAttribute);
                    }
                }
                else
                {
                    for (int i = 0; i < individualBehaviours.Count; i++)
                    {
                        var behaviourName = individualBehaviours.Item(i).Attributes["name"].Value;

                        if (svcEndpointLevelBehaviours.Contains(behaviourName) && clientEndpointLevelBehaviours.Contains(behaviourName))
                        {
                            Console.Error.WriteLine($"EndPointBehaviour '{behaviourName}' is shared by client and service. Please split them and continue!");
                            Environment.Exit(-1);
                        }
                        else if (clientEndpointLevelBehaviours.Contains(behaviourName))
                        {
                            if (individualBehaviours.Item(i).SelectSingleNode("pivotalWcfClientIwaInterceptorExtensions") == null)
                            {
                                individualBehaviours.Item(i).AppendChild(AddInterceptorExtension(doc));
                            }
                        }
                    }
                }

                for (int i = 0; i < endpoints.Count; i++)
                {
                    var elbc = endpoints.Item(0).Attributes["behaviorConfiguration"]?.Value;

                    if (!string.IsNullOrWhiteSpace(elbc))
                    {
                        var behaviourConfigurationAttribute = doc.CreateAttribute("behaviorConfiguration");
                        behaviourConfigurationAttribute.Value = "pivotalWcfClientIwaInterceptorBehaviour";
                        endpoints.Item(i).Attributes.Append(behaviourConfigurationAttribute);
                    }
                }

                var extensions = (XmlElement)serviceModel.SelectSingleNode("extensions");

                if (extensions == null)
                {
                    extensions = doc.CreateElement("extensions");
                    serviceModel.AppendChild(extensions);
                }

                var behaviorExtensions = (XmlElement)extensions.SelectSingleNode("behaviorExtensions");

                if (behaviorExtensions == null)
                {
                    behaviorExtensions = doc.CreateElement("behaviorExtensions");
                    extensions.AppendChild(behaviorExtensions);
                }

                var pivotalIwaInterceptorExtensions = behaviorExtensions.SelectNodes("add");

                bool isExtensionExist = false;

                for (int i = 0; i < pivotalIwaInterceptorExtensions.Count; i++)
                {
                    if (pivotalIwaInterceptorExtensions.Item(i).Attributes["name"].Value == "pivotalWcfClientIwaInterceptorExtensions")
                        isExtensionExist = true;
                }

                if (!isExtensionExist)
                {
                    var interceptorExtensionNode = doc.CreateElement("add");
                    interceptorExtensionNode.SetAttribute("name", "pivotalWcfClientIwaInterceptorExtensions");
                    interceptorExtensionNode.SetAttribute("type", typeof(IwaInterceptorBehaviourExtensionElement).AssemblyQualifiedName);
                    behaviorExtensions.AppendChild(interceptorExtensionNode);
                }

                doc.Save(webConfigPath);

                ValidateIfAllEndPointsAreSetWithBehaviourConfiguration(client);

                InjectKerberosAssembliesAndRedistributables(buildPath);
            }
        }

        private static XmlElement GetOrCreateEndPointBehaviours(XmlDocument doc, XmlElement behaviours)
        {
            var endpointBehaviours = (XmlElement)behaviours.SelectSingleNode("endpointBehaviors");

            if (endpointBehaviours == null)
            {
                endpointBehaviours = doc.CreateElement("endpointBehaviors");
                behaviours.AppendChild(endpointBehaviours);
            }

            return endpointBehaviours;
        }

        private static void AddAllExistingClientEndpointBehaviours(List<string> clientEndpointLevelBehaviours, XmlNodeList endpoints)
        {
            for (int j = 0; j < endpoints.Count; j++)
            {
                var elbc = endpoints.Item(0).Attributes["behaviorConfiguration"]?.Value;
                if (!string.IsNullOrWhiteSpace(elbc))
                    clientEndpointLevelBehaviours.Add(elbc);
            }
        }

        private static void AddAllExistingSvcEndpointBehaviours(XmlDocument doc, List<string> svcEndpointLevelBehaviours)
        {
            var individualServices = doc.SelectNodes("configuration/system.serviceModel/services/service");
            for (int i = 0; i < individualServices.Count; i++)
            {
                var svcendpoints = individualServices.Item(i).SelectNodes("endpoint");

                for (int j = 0; j < svcendpoints.Count; j++)
                {
                    var elbc = svcendpoints.Item(0).Attributes["behaviorConfiguration"]?.Value;
                    if (!string.IsNullOrWhiteSpace(elbc))
                        svcEndpointLevelBehaviours.Add(elbc);
                }
            }
        }

        private static void ValidateIfPivotalWcfClientInterceptorPackageIsInstalled(string buildPath)
        {
            var dir = new DirectoryInfo(buildPath);
            if (dir.EnumerateFiles("RouteServiceIwaWcfInterceptor.dll", SearchOption.AllDirectories).ToList().Count == 0
                || dir.EnumerateFiles("Microsoft.AspNetCore.Authentication.GssKerberos.dll", SearchOption.AllDirectories).ToList().Count == 0)
            {
                Console.Error.WriteLine("-----> **ERROR** Could not find assembly 'RouteServiceIwaWcfInterceptor' or one/more of its dependencies, make sure to install the latest package 'PivotalServices.WcfClient.Kerberos.Interceptor' from nuget or https://www.myget.org/F/ajaganathan/api/v3/index.json");
                Environment.Exit(-1);
            }
        }

        private static void CopyAuthorizationPolicyAssemblyToAppBinPath(string buildPath)
        {
            var assemblyDll = typeof(RouteServiceAuthorizationPolicy).Assembly.Location;
            var targetFileName = Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll));

            Console.WriteLine("-----> Injecting RouteServiceAuthorizationPolicy assembly into the application target directory...");

            if (!File.Exists(targetFileName))
                File.Copy(assemblyDll, Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll)));
        }

        private void AddAuthorizationElementToPreExistingBehaviours(XmlDocument doc, XmlNodeList individualBehaviours)
        {
            for (int i = 0; i < individualBehaviours.Count; i++)
            {
                AddAuthorizationElementToBehaviour(doc, individualBehaviours, i);
            }
        }

        private static void SetPivotaWcfServiceIwaAuthBehaviourConfigurationToAllServices(XmlDocument doc, XmlNode services)
        {
            var individualServices = services.SelectNodes("service");

            for (int i = 0; i < individualServices.Count; i++)
            {
                SetPivotaWcfServiceIwaAuthBehaviourConfigurationToService(doc, individualServices, i);
            }
        }

        private void AddAuthorizationElementToBehaviour(XmlDocument doc, XmlNodeList individualBehaviours, int i)
        {
            individualBehaviours.Item(i).AppendChild(CreateServiceAuthorizationElement(doc, (XmlElement)individualBehaviours.Item(i)));
        }

        private static void SetPivotaWcfServiceIwaAuthBehaviourConfigurationToService(XmlDocument doc, XmlNodeList individualServices, int i)
        {
            var behaviourConfigurationAttribute = doc.CreateAttribute("behaviorConfiguration");
            behaviourConfigurationAttribute.Value = "PivotaWcfServiceIwaAuthBehaviour";
            individualServices.Item(i).Attributes.Append(behaviourConfigurationAttribute);
        }

        private void CreatePivotalServiceBehaviour(XmlDocument doc, XmlElement serviceBehaviours)
        {
            var individualBehaviour = doc.CreateElement("behavior");
            individualBehaviour.SetAttribute("name", "PivotaWcfServiceIwaAuthBehaviour");
            individualBehaviour.AppendChild(CreateServiceAuthorizationElement(doc, individualBehaviour));
            serviceBehaviours.AppendChild(individualBehaviour);
        }

        private static bool PivotalServiceBehaviourExistsAlready(XmlNodeList individualBehaviours)
        {
            bool pivotaWcfServiceIwaAuthBehaviourExists = false;

            for (int i = 0; i < individualBehaviours.Count; i++)
            {
                if (individualBehaviours.Item(i).Attributes["name"].Value == "PivotaWcfServiceIwaAuthBehaviour")
                    pivotaWcfServiceIwaAuthBehaviourExists = true;
            }

            return pivotaWcfServiceIwaAuthBehaviourExists;
        }

        private static XmlElement GetOrCreateServiceBehaviours(XmlDocument doc, XmlElement behaviours)
        {
            var serviceBehaviours = (XmlElement)behaviours.SelectSingleNode("serviceBehaviors");

            if (serviceBehaviours == null)
            {
                serviceBehaviours = doc.CreateElement("serviceBehaviors");
                behaviours.AppendChild(serviceBehaviours);
            }

            return serviceBehaviours;
        }

        private static XmlElement GetOrCreateBehavioursSection(XmlDocument doc, XmlNode serviceModel)
        {
            var behaviours = (XmlElement)serviceModel.SelectSingleNode("behaviors");

            if (behaviours == null)
            {
                behaviours = doc.CreateElement("behaviors");
                serviceModel.AppendChild(behaviours);
            }

            return behaviours;
        }

        private static void InjectKerberosAssembliesAndRedistributables(string buildPath)
        {
            string[] files = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(typeof(RouteServiceAuthenticationBuildpack).Assembly.Location), "requiredAssemblies"));

            Console.WriteLine("-----> Injecting MIT Kerberos assembllies and c++ redistributables into the application target directory...");

            foreach (string file in files)
                File.Copy(file, Path.Combine(buildPath, "bin", Path.GetFileName(file)), true);
        }

        private XmlNode AddInterceptorExtension(XmlDocument xmlDoc)
        {
            var extension = xmlDoc.CreateElement("pivotalWcfClientIwaInterceptorExtensions");
            return extension;
        }

        private XmlElement CreateServiceAuthorizationElement(XmlDocument xmlDoc, XmlElement individualBehaviour)
        {
            var serviceAuthorization = individualBehaviour.SelectSingleNode("serviceAuthorization");

            if (serviceAuthorization == null)
            {
                serviceAuthorization = xmlDoc.CreateElement("serviceAuthorization");

                var principalPermissionModeAttribute = xmlDoc.CreateAttribute("principalPermissionMode");
                principalPermissionModeAttribute.Value = "Custom";
                serviceAuthorization.Attributes.Append(principalPermissionModeAttribute);
            }

            var authPolicies = serviceAuthorization.SelectSingleNode("authorizationPolicies");

            if (authPolicies == null)
            {
                authPolicies = xmlDoc.CreateElement("authorizationPolicies");
                serviceAuthorization.AppendChild(authPolicies);
            }

            var policyNodes = authPolicies.SelectNodes("add");
            bool policyNodeExist = false;

            for (int i = 0; i < policyNodes.Count; i++)
            {
                if (policyNodes.Item(i).Attributes["policyType"].Value.Contains("RouteServiceAuthorizationPolicy"))
                    policyNodeExist = true;
            }

            if (!policyNodeExist)
            {
                var policyNode = xmlDoc.CreateElement("add");
                policyNode.SetAttribute("policyType", typeof(RouteServiceAuthorizationPolicy).AssemblyQualifiedName);
                authPolicies.AppendChild(policyNode);
            }

            return (XmlElement)serviceAuthorization;
        }

        private static void ValidateIfAllEndPointsAreSetWithBehaviourConfiguration(XmlNode client)
        {
            var endPoints = client.SelectNodes("endpoint");

            bool behaviourExist = true;

            for (int i = 0; i < endPoints.Count; i++)
            {
                if (endPoints.Item(i).Attributes["behaviorConfiguration"] == null)
                    behaviourExist = false;
            }

            if (!behaviourExist)
                Console.Error.WriteLine(@"-----> **WARNING** One or more of the client\endpoint does not have a behaviorConfiguration set!");
        }

        private static void ValidateIfAllServicesAreSetWithBehaviourConfiguration(XmlNode servicesRoot)
        {
            var services = servicesRoot.SelectNodes("service");

            bool behaviourExist = true;

            for (int i = 0; i < services.Count; i++)
            {
                if (services.Item(i).Attributes["behaviorConfiguration"] == null)
                    behaviourExist = false;
            }

            if (!behaviourExist)
                Console.Error.WriteLine(@"-----> **WARNING** One or more of the services\service does not have a behaviorConfiguration set!");
        }
    }
}
