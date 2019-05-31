This project offers a supply buildpack which helps applying IWA security (kerberos) to applications running in PCF, together with the below components.

1. Route Service (https://github.com/macsux/route-service-auth)
2. Nuget Package 'PivotalServices.WcfClient.Kerberos.Interceptor' (https://github.com/alfusinigoj/wcf-client-interceptor-egress-kerberos-auth), (egress) in case of Wcf Client connect to a secured service Wcf Service. Note: This package is not required for any ingress scenario.
