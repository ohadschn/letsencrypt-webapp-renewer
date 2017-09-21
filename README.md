[![Build status](https://ci.appveyor.com/api/projects/status/3fwuiks1yq4oro4v/branch/master?svg=true)](https://ci.appveyor.com/project/ohadschn/letsencrypt-webapp-renewer/branch/master)

# letsencrypt-webapp-renewer
A WebJob-ready console application for renewing Azure Web App TLS/SSL certificates (based on [letsencrypt-siteextension](https://github.com/sjkp/letsencrypt-siteextension))
## Motivation
HTTPS is the pervasive standard for all websites, regardless of size or field. 
The Mozilla foundation has gone so far as to [announce their intent to completely phase out HTTP](https://blog.mozilla.org/security/2015/04/30/deprecating-non-secure-http/). 
Unfortunately, the procurement, maintenance, and renewal of SSL/TLS certificates has been an expensive and manual process for many.

Enter [Let's Encrypt](https://letsencrypt.org/) - a free, automated, and open Certificate Authority. Shortly after its release, Simon J.K. Pedersen created the excellent [letsencrypt-siteextension](https://github.com/sjkp/letsencrypt-siteextension) Azure Web App extension for easy integration with Azure Web Apps. However, at the time of writing it suffers from several issues:

- The extension must be installed on the same web app as your site.
  - This means you must install the extension on each and every Web App you own.
  - Worse, if you happen to publish your Web App with the "Delete Existing files", it will silently delete the WebJob created by the extension, effectively nullifying it.
- There are no e-mail notifications (you could set some basic ones with Zapier but they won't contain details on the actual renewals that took place).
- It relies on an Azure Storage account which has to be [configured in a certain way](https://github.com/sjkp/letsencrypt-siteextension/issues/148), which is an unneeded possible point of failure.
- The extension can only be run in the context of a web app. You might want to run it as a command-line tool (e.g. from your CI system).

## Solution
`letsencrypt-webapp-renewer` is a WebJob-ready command-line executable that builds upon [letsencrypt.azure.core](https://www.nuget.org/packages/letsencrypt.azure.core/) (the core component behind letsencrypt-siteextension) to provide the following features:
- Install on any Web App (doesn't have to be the same web app for which you want to manage SSL certs).
  - Multiple Web App management is supported.
  - Publishing with "Delete Existing files" has no effect when the WebJob is deployed to a different (preferably dedicated) Web App.
- E-mail notifications are built in (via SendGrid).
- No external dependencies other than Let's Encrypt.
- Can be executed as a plain command-line tool from any environment.

## Preparation
Create an AAD service principal with the proper permissions, as explained [here](https://github.com/sjkp/letsencrypt-siteextension/wiki/How-to-install) and [here](https://www.troyhunt.com/everything-you-need-to-know-about-loading-a-free-lets-encrypt-certificate-into-an-azure-website/). You can skip the parts about configuring the Azure Storage account and the site extension, but while you're there note down the parameters you'll need for the WebJob configuration below: `SubscriptionId`, `TenantId`, `ResourceGroup`,  `WebApp`, `ClientId`, and `ClientSecret`.

## Configuration
The `letsencrypt-webapp-renewer` WebJob is configured via [Web App Settings](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-configure#application-settings). You might as well configure it before installing so that it doesn't run with no/partial configuration by mistake. Note that these settings should be configured on the Web App where the `letsencrypt-webapp-renewer` WebJob is deployed (NOT on the Web Apps to be renewed).
1. Set `letsencrypt:webApps` to a semicolon-delimited list of Azure Web App names for which certificate renewal should take place.
1. For each Web App specified in `letsencrypt:webApps`, set the following app setting with the proper values as noted down in the  preparation above (replacing `webAppName` with the actual Web App name):
   1. `letsencrypt:webAppName-subscriptionId`
   1. `letsencrypt:webAppName-tenantId`
   1. `letsencrypt:webAppName-resourceGroup`
   1. `letsencrypt:webAppName-hosts` (semicolon-delimited)
   1. `letsencrypt:webAppName-email` (will be used for both Let's Encrypt registration and e-mail notifications)
   1. `letsencrypt:webAppName-clientId`
   1. `letsencrypt:webAppName-clientSecret` (should be set as a **connection string**)
   1. (optional) `letsencrypt:webAppName-servicePlanResourceGroup`
   1. (optional) `letsencrypt:webAppName-siteSlotName`
   1. (optional) `letsencrypt:webAppName-useIpBasedSsl`
   1. (optional) `letsencrypt:webAppName-rsaKeyLength`
   1. (optional) `letsencrypt:webAppName-acmeBaseUri`

For more information about the various renewal settings see: https://github.com/sjkp/letsencrypt-siteextension.

### Sample configuration
- `letsencrypt:webApps`: `ohadsoft;howlongtobeatsteam`
- `letsencrypt:ohadsoft-subscriptionId`: `e432f869-4777-4380-a654-3440216992a2`
- `letsencrypt:ohadsoft-tenantId`: `ohadsoft.onmicrosoft.com`
- `letsencrypt:ohadsoft-resourceGroup`: `ohadsoft-rg`
- `letsencrypt:ohadsoft-hosts`: `www.ohadsoft.com;ohadsoft.com`
- `letsencrypt:ohadsoft-email`: `renewal@ohadsoft.com`
- `letsencrypt:ohadsoft-clientId`: `5e1346b6-7db5-4eae-b9fa-7b3d5e42e6c7`
- (**connection string**) `letsencrypt:ohadsoft-clientSecret`: `MySecretPassword123`
- `letsencrypt:howlongtobeatsteam-subscriptionId`: `e432f869-4777-4380-a654-3440216992a2`
- `letsencrypt:howlongtobeatsteam-tenantId`: `ohadsoft.onmicrosoft.com`
- `letsencrypt:howlongtobeatsteam-resourceGroup`: `hltbs-rg`
- `letsencrypt:howlongtobeatsteam-hosts`: `www.howlongtobeatsteam.com;howlongtobeatsteam.com`
- `letsencrypt:howlongtobeatsteam-email`: `renewal@howlongtobeatsteam.com`
- `letsencrypt:howlongtobeatsteam-clientId`: `5e1346b6-7db5-4eae-b9fa-7b3d5e42e6c7`
- (**connection string**) `letsencrypt:howlongtobeatsteam-clientSecret`: `MySecretPassword123`

## Installation
1. (**optional but highly recommended**) Create a new dedicated Web App for cert renewal, to which you will deploy the `letsencrypt-webapp-renewer` WebJob. This will drastically decrease the likelihood of accidental deletion of the renewal WebJob  (e.g. upon deployment of a different app to the same Web App using _Delete Existing files_)
1. Download the latest [`letsencrypt-webapp-renewer` WebJob zip file](https://github.com/ohadschn/letsencrypt-webapp-renewer/releases).
1. Deploy the WebJob zip file you downloaded above to the Web App where you want cert renewal to execute using one of the following scheduling methods:
   1. [CRON based](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-create-web-jobs#CreateScheduledCRON) is simple to set up but **REQUIRES YOUR CERT RENEWAL WEB APP (THE ONE WHERE THE `letsencrypt-webapp-renewer` WEBJOB WILL BE RUNNING) TO BE CONFIGURED AS "ALWAYS ON"** (if that is not acceptable, for example if your tier does not support the _Always On_ feature, use Azure Scheduler as described below). Note that a `settings.job` file as described in the docs is unnecessary - when you [upload the WebJob in the portal](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-create-web-jobs#CreateOnDemand) simply select **Triggered** in the _Type_ field and **Scheduled** in the _Triggers_ field to be given an option to specify a CRON expression. The [recommended Let's Encrypt renewal period is 60 days](https://letsencrypt.org/docs/faq/), so you could use a CRON expression that fires once every two months, for example: `0 0 0 1 1,3,5,7,9,11 *`.
   1. [Azure Scheduler based](http://blog.davidebbo.com/2015/05/scheduled-webjob.html) is slightly more complex to set up but does not require your site to be configured as _Always On_ (so at the time of writing it can work with the Free plan, for example). Note that the instructions in the link mostly explain the process for the old (classic) Azure portal, but it's pretty easy to do the same in the new one (portal.azure.com).

### Notifications
The following are optional but **highly recommended**.
1. In order to receive notifications on successful renewals, set the `letsencrypt:webAppName-SendGridApiKey` Application Setting to your [SendGrid API key](https://sendgrid.com/docs/Classroom/Send/How_Emails_Are_Sent/api_keys.html). At the time of writing, SendGrid offer a free plan in the [Azure Marketplace](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/SendGrid.SendGrid) which should easily suffice for any reasonable SSL renewal notification needs.
1. Set up [Zapier](https://zapier.com/help/windows-azure-web-sites/) to send you notifications on `letsencrypt-webapp-renewer` WebJob runs. While e-mail notifications are supported as described above, **they will only be fired when the WebJob has failed for any reason** (this is intentional - a WebJob cannot reliably handle any possible failure it might encounter). By contrast, Zapier operates externally to the WebJob and should be able to report any error that might have caused the WebJob to fail. At the time of writing, Zapier offer a free account which should easily suffice for any reasonable SSL renewal notification needs.

## Verification
Test the WebJob by [triggering it manually](https://pragmaticdevs.wordpress.com/2016/10/24/triggering-azure-web-jobs-manually/). **You should see a new certificate served when you visit your site**.

## Command Line usage
Wen executed outside of a WebJob context (as determined by the [WEBJOBS_NAME](https://github.com/projectkudu/kudu/wiki/WebJobs#environment-settings) environment variable), the webjob executable (`AzureLetsEncryptRenewer.exe`) functions as a standalone command-line tool:

> AzureLetsEncryptRenewer.exe SubscriptionId TenantId ResourceGroup WebApp Hosts Email ClientId ClientSecret [ServicePlanResourceGroupName] [SiteSlotName] [UseIpBasedSsl] [RsaKeyLength] [AcmeBaseUri]

- `Hosts` is a semicolon-delimited list of host names
- `ServicePlanResourceGroupName` is optional and can be empty (`""`) if it is the same as the Web App resource group
- `SiteSlotName` is optional and can be empty (`""`) if site deployment slots are not to be used
- `UseIpBasedSsl` is optional and defaults to false
- `RsaKeyLength` is optional and defaults to 2048
- `AcmeBaseUri` is optional and defaults to https://acme-v01.api.letsencrypt.org/

Exit codes: 
- 0 = Success
- 1 = Argument error
- 2 = Unexpected error

## Telemetry
`letsencrypt-webapp-renewer` gathers anonymous telemetry for usage analysis and error reporting. You can disable it by setting the `LETSENCRYPT_DISABLE_TELEMETRY` to any non-empty value.

## Limitations & Disclaimer 
Since this project relies on https://github.com/sjkp/letsencrypt-siteextension, some of its limitations apply as well:
> This site-extension is NOT supported by Microsoft it is my own work based on https://github.com/ebekker/ACMESharp and https://github.com/Lone-Coder/letsencrypt-win-simple - this means don't expect 24x7 support, I use it for several of my own smaller sites, but if you are running sites that are important you should consider spending the few $ on a certificate and go with a Microsoft supported way of enabling SSL, so you have someone to blame :)

> Note that Let's Encrypt works by providing automated certificates of a short (currently three month) duration. This extension is BETA SOFTWARE. You will need to keep this extension updated or risk losing SSL access when your certificate expires.

> Due to rate limiting of Let's Encrypt servers, you can only request five certificates per domain name per week. Configuration errors or errors in this site extension may render you unable to retrieve a new certificate for seven days. If up-time is critical, have a plan for deploying a SSL certificate from another source in place.

> No support for multi-region web apps, so if you use traffic manager or some other load balancer to route traffic between web apps in different regions please don't use this extension.

> The site-extension will not work with Azure App Service Local Cache

> Please take note that this Site-Extension is beta-software, so use at your own risk.

> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYLEFT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Consult the Let's Encrypt documentation for rate limits: https://letsencrypt.org/docs/rate-limits/
