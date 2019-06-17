### IMPORTANT NOTE: From version 2.0.0, this buildpack supports only ingress scenario. Egress scenarios are supported by another supply buildpack https://github.com/alfusinigoj/route-service-auth-egress-buildpack

#### This project offers a supply buildpack which helps applying IWA security (kerberos) for app-app/svc-svc/webapp secure communication, in PCF. 

In detail, if application running in PCF is protected by `route service` (https://github.com/macsux/route-service-auth), this buildpack helps in injecting the logged-in user principal into the current context/thread, please listen to Andrew Stackhov's video here "youtube video link here"'

#### Specific functionalities executed by this buildpack

  1. Injects a HttpModule which sets the user identity into the current context/thread by configuring web.config accordingly and copying the assembly containing the module
  2. If it detects a Wcf Service, configured the web.config for adding a service behaviour which uses a custom authorization policy to copy the user principal from current http context to wcf thread. It also copies the assembly containing the custom authorization policy

#### To manually build and release this buildpack using command `nuke <target>` from powershell window.
*Targets*
  1. `Clean`    --> cleans bin/obj/artifacts folder
  2. `Restore`  --> Restores all nuget dependencies (depends on `Clean`)
  3. `Compile`  --> Compiles the buildpack (depends on `Restore`)
  4. `Test`     --> Runs all the tests for the buildpack (depends on `Compile`)
  5. `Publish`  --> Publishes the buildpack artifact under `artifacts` directory (depends on `Test`)
  6. `Release`  --> Create a new buildpack release under the github repo (depends on `Publish`). You will be prompted for `Github ApiToken` which has release rights to the github repo, alternatively you can pass as commandline param `--GitHubToken`
