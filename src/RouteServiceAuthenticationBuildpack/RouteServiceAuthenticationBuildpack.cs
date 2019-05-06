using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Pivotal.RouteServiceIdentityModule;

namespace RouteServiceAuthenticationBuildpack
{
    public class RouteServiceAuthenticationBuildpack : SupplyBuildpack
    {
        
        protected override bool Detect(string buildPath)
        {
            return File.Exists(Path.Combine(buildPath,"web.config"));
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            var webConfigPath = Path.Combine(buildPath, "web.config");
            if (!File.Exists(webConfigPath))
                return;
            var doc = new XmlDocument();
            doc.Load(webConfigPath);
            
            if (doc.SelectSingleNode("configuration/system.webServer/modules/add[@name=\"RouteServiceIdentityModule\"]") == null)
            {
                var modules = (XmlElement) doc.SelectSingleNode("configuration/system.webServer/modules");
                if (modules == null)
                {
                    var webServerNode = (XmlElement) doc.SelectSingleNode("configuration/system.webServer");
                    if (webServerNode == null)
                    {
                        webServerNode = doc.CreateElement("system.webServer");
                        var configNode = (XmlElement) doc.SelectSingleNode("configuration");
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
            if(!File.Exists(targetFileName))
                File.Copy(assemblyDll, Path.Combine(buildPath, "bin", Path.GetFileName(assemblyDll)));
        }
    }
}
