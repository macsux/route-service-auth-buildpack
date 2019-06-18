using System;
using System.Xml;

namespace Pivotal.RouteService.Auth.Ingress.Buildpack.Identity
{
    public class WebConfigFileAppender : IConfigFileAppender
    {
        private bool disposedValue = false;
        private readonly string webConfigPath;
        XmlDocument doc = new XmlDocument();

        public WebConfigFileAppender(string webConfigPath)
        {
            this.webConfigPath = webConfigPath;
        }

        public void Execute()
        {
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
                routeServiceModuleNode.SetAttribute("name", nameof(Pivotal.RouteServiceIdentityModule.RouteServiceIdentityModule));
                routeServiceModuleNode.SetAttribute("type", typeof(Pivotal.RouteServiceIdentityModule.RouteServiceIdentityModule).AssemblyQualifiedName);
                modules.AppendChild(routeServiceModuleNode);
            }
        }

        private void SaveChanges()
        {
            doc.Save(webConfigPath);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SaveChanges();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
