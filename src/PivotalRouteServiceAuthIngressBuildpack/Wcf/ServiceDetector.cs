using System;
using System.Xml;

namespace Pivotal.RouteService.Auth.Ingress.Buildpack.Wcf
{
    public class ServiceDetector : IDetector
    {
        XmlDocument doc = new XmlDocument();

        public ServiceDetector(string webConfigPath)
        {
            doc.Load(webConfigPath);
        }

        public bool Find()
        {
            Console.WriteLine("-----> Checking for WCF Service application...");

            var services = doc.SelectSingleNode("configuration/system.serviceModel/services");

            if (services == null)
            {
                Console.WriteLine($"-----> **INFO** WCF Service not found, skipping configuring authorization policy for WCF Services");
                return false;
            }

            Console.WriteLine("-----> Detected WCF Service application");
            return true;
        }
    }
}
