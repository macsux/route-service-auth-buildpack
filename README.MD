## Supply buildpack to establish Thread identity

This buildpack will inject HTTP module into apps that are started by HWC buildpack to establish a thread ClaimsPrincipal using X-CF-Identity header. This is to be used in conjunction with [Keberos Route Service](<https://github.com/macsux/route-service-auth>) allowing zero-code-change Kerberos authentication in legacy .NET apps.

#### WARNING

Using this buildpack without the above route service will expose your application to security risk as PCF natively does not strip X-CF-Identity header which would allow to be spoofed from external requests