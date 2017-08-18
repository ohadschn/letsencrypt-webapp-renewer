# letsencrypt-webapp-renewer
A simple WebJob-ready console application for renewing Azure Web App TLS/SSL certificates (based on letsencrypt-siteextension)
## Motivation
HTTPS is the pervasibe standard for all websites, regardless of size or field. 
The Mozilla foundation has gone so far as to [announce their intent to completely phase out HTTP](https://blog.mozilla.org/security/2015/04/30/deprecating-non-secure-http/). 
Unfortunately, the procurement, maintenance, and renewal of SSL/TLS certificates has been an expensive and manual process for many.

Enter [Let's Encrypt](https://letsencrypt.org/) - a free, automated, and open Certificate Authority. Shortly after its release, Simon J.K. Pedersen created the excellent [letsencrypt-siteextension](https://github.com/sjkp/letsencrypt-siteextension) Azure Web App extension for easy integration with Azure Web Apps. However, at the time of writing it suffers from several issues:

- The extension has to be installed on the same web app as your site.
  - This means you must install the extension on each and every Web App you own.
  - Worse, if you happen to publish your Web App with the "Delete Existing files", it will silently delete the webjob created by the extension, effectively nullifying it.
- There are no e-mail notifications (you could set some basic ones with Zapier but they won't contain details on the actual renewals that took place).
- It relies on an Azure Storage account which has to be [configured in a certain way](https://github.com/sjkp/letsencrypt-siteextension/issues/148), which is an unneeded possible point of failure.
- The extension can only be run in the context of a web app. You might want to run it as a command-line tool (e.g. from your CI system).

## Solution
`letsencrypt-webapp-renewer` is a WebJob-ready commandline that offers the following features:
- Install on any Web App (doesn't have to be the same web app for which you want to manage SSL certs)
  - Multiple Web App management is supported
  - Publishing with "Delete Existing files" has no effect then the webjob is deployed to a different (preferably dedicated) Web App.
- E-mail notifications are build it (via SendGrid)
- No external dependencies other than Let's Encrypt
- Can be executed as a plain command-line tool from any environment

## Preparation
1. Download the latest [`letsencrypt-webapp-renewer` WebJob zip file](https://github.com/ohadschn/letsencrypt-webapp-renewer/releases).
2. Decide on the WebJob scheduling option that works for you
   1. [CRON based](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-create-web-jobs#CreateScheduledCRON) is simple to set up but **REQUIRES YOUR WEB APP TO BE CONFIGURED AS "ALWAYS ON"**.
      1. If that is acceptable, all you have to do is edit the `settings.job` file in the `letsencrypt-webapp-renewer` WebJob zip file and edit the schedule to your liking. The default schedule follows the [recommended Let's Encrypt renewal period of 60 days](https://letsencrypt.org/docs/faq/) (once every two months).
      1. If that is not acceptible (for example, your tier might not support the _Always On_ feature), use Azure Scheduler as described below.
   2. [Azure Scheduler based](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-create-web-jobs#CreateScheduled) is slightly more complex to set up but does not require your site to be configured as _Always On_. Make sure to delete the `settings.job` file from the `letsencrypt-webapp-renewer` WebJob zip file if you use this option, as to prevent needless executions.

## Configuration


## Installation
1. [Deploy](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-create-web-jobs) the latest  as to the Web App in which you want certificate renewal to execute (preferably a dedicated Web App created solely for this purpose) as **triggered (AKA on-demand)**. 
  1. Note that by default,  is used. .
  1.
You have several options for webjob deployment:
  1. Upload the  directly [to your Web App].
  1. Clone the reposi
## Command Line usage
The webjob executable (`AzureLetsEncryptRenewer.exe`) can be used as a standalone command-line tool:

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

Consult the Let's Encrypt documentation for rate limits: https://letsencrypt.org/docs/rate-limits/

