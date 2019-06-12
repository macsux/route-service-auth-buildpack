#### This project offers a supply buildpack which helps applying IWA security (kerberos) to applications running in PCF, together with the below components.

1. Route Service (https://github.com/macsux/route-service-auth)
2. Nuget Package 'PivotalServices.WcfClient.Kerberos.Interceptor' (https://github.com/alfusinigoj/wcf-client-interceptor-egress-kerberos-auth), (egress) in case of Wcf Client connect to a secured service Wcf Service. **Note: This package is not required for any ingress scenario.**

##### Controlling Environment Variables:
1. `INCLUDE_EGRESS_MODULE_FOR_WCF_CLIENT` (Default value: `false`) -- Set it to `true` if the wcf client needs to egress processing, to call a secured (protected by route service) wcf service 
2. `INCLUDE_INRESS_MODULE_FOR_WCF_SERVICE` (Default value: 'true') -- Set it to `false` if the wcf service is not secured by a route service