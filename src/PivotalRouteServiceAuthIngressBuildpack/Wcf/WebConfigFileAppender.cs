using System;
using System.Xml;

namespace Pivotal.RouteService.Auth.Ingress.Buildpack.Wcf
{
    public class WebConfigFileAppender : IConfigFileAppender
    {
        private bool disposedValue = false;
        private readonly string webConfigPath;
        XmlDocument doc = new XmlDocument();

        public WebConfigFileAppender(string webConfigPath)
        {
            this.webConfigPath = webConfigPath;
            doc.Load(webConfigPath);
        }

        public void Execute()
        {
            Console.WriteLine("-----> Applying configuration changes to add RouteServiceAuthorizationPolicy into the pipeline...");

            var services = doc.SelectSingleNode("configuration/system.serviceModel/services");

            var serviceModel = doc.SelectSingleNode("configuration/system.serviceModel");

            var behaviours = GetOrCreateBehavioursSection(doc, serviceModel);

            var serviceBehaviours = GetOrCreateServiceBehaviours(doc, behaviours);

            var individualBehaviours = serviceBehaviours.SelectNodes("behavior");

            var pivotaWcfServiceIwaAuthBehaviourExists = PivotalServiceBehaviourExistsAlready(individualBehaviours);

            if (!pivotaWcfServiceIwaAuthBehaviourExists)
                CreatePivotalServiceBehaviour(doc, serviceBehaviours);

            SetPivotaWcfServiceIwaAuthBehaviourConfigurationToAllServices(doc, services);

            AddAuthorizationElementToPreExistingBehaviours(doc, individualBehaviours);

            ValidateIfAllServicesAreSetWithBehaviourConfiguration(services);
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
            individualBehaviours.Item(i).AppendChild(CreateDefaultServiceBehaviourElementServiceDebug(doc, (XmlElement)individualBehaviours.Item(i)));
            individualBehaviours.Item(i).AppendChild(CreateDefaultServiceBehaviourElementServiceMetadata(doc, (XmlElement)individualBehaviours.Item(i)));
            individualBehaviours.Item(i).AppendChild(CreateDefaultServiceBehaviourElementUseRequestHeadersForMetadataAddress(doc, (XmlElement)individualBehaviours.Item(i)));
        }

        private static void SetPivotaWcfServiceIwaAuthBehaviourConfigurationToService(XmlDocument doc, XmlNodeList individualServices, int i)
        {
            if (individualServices.Item(i).Attributes["behaviorConfiguration"] == null)
            {
                var behaviourConfigurationAttribute = doc.CreateAttribute("behaviorConfiguration");
                behaviourConfigurationAttribute.Value = "PivotaWcfServiceIwaAuthBehaviour";
                individualServices.Item(i).Attributes.Append(behaviourConfigurationAttribute);
            }
        }

        private void CreatePivotalServiceBehaviour(XmlDocument doc, XmlElement serviceBehaviours)
        {
            var individualBehaviour = doc.CreateElement("behavior");
            individualBehaviour.SetAttribute("name", "PivotaWcfServiceIwaAuthBehaviour");
            individualBehaviour.AppendChild(CreateServiceAuthorizationElement(doc, individualBehaviour));
            individualBehaviour.AppendChild(CreateDefaultServiceBehaviourElementServiceDebug(doc, individualBehaviour));
            individualBehaviour.AppendChild(CreateDefaultServiceBehaviourElementServiceMetadata(doc, individualBehaviour));
            individualBehaviour.AppendChild(CreateDefaultServiceBehaviourElementUseRequestHeadersForMetadataAddress(doc, individualBehaviour));
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

        private XmlElement CreateDefaultServiceBehaviourElementServiceMetadata(XmlDocument xmlDoc, XmlElement individualBehaviour)
        {
            var serviceMetadata = individualBehaviour.SelectSingleNode("serviceMetadata");

            if (serviceMetadata == null)
            {
                serviceMetadata = xmlDoc.CreateElement("serviceMetadata");

                var attribute = xmlDoc.CreateAttribute("httpGetEnabled");
                attribute.Value = "true";
                serviceMetadata.Attributes.Append(attribute);
            }

            return (XmlElement)serviceMetadata;
        }

        private XmlElement CreateDefaultServiceBehaviourElementServiceDebug(XmlDocument xmlDoc, XmlElement individualBehaviour)
        {
            var serviceDebug = individualBehaviour.SelectSingleNode("serviceDebug");

            if (serviceDebug == null)
            {
                serviceDebug = xmlDoc.CreateElement("serviceDebug");

                var attribute = xmlDoc.CreateAttribute("includeExceptionDetailInFaults");
                attribute.Value = "false";
                serviceDebug.Attributes.Append(attribute);
            }

            return (XmlElement)serviceDebug;
        }

        private XmlElement CreateDefaultServiceBehaviourElementUseRequestHeadersForMetadataAddress(XmlDocument xmlDoc, XmlElement individualBehaviour)
        {
            var useRequestHeadersForMetadataAddress = individualBehaviour.SelectSingleNode("useRequestHeadersForMetadataAddress");

            if (useRequestHeadersForMetadataAddress == null)
                useRequestHeadersForMetadataAddress = xmlDoc.CreateElement("useRequestHeadersForMetadataAddress");

            return (XmlElement)useRequestHeadersForMetadataAddress;
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
                policyNode.SetAttribute("policyType", typeof(Pivotal.RouteServiceAuthorizationPolicy.RouteServiceAuthorizationPolicy).AssemblyQualifiedName);
                authPolicies.AppendChild(policyNode);
            }

            return (XmlElement)serviceAuthorization;
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
