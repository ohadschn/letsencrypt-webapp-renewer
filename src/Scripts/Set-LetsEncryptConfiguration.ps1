<#
.SYNOPSIS
Sets up the confguration for a Let's Encrypt renewal web app (https://github.com/ohadschn/letsencrypt-webapp-renewer)

.DESCRIPTION
The Set-LetsEncryptSettings script sets up your letsencrypt-webapp-renewer web app with the required renewal Settings.
More information is available at https://github.com/ohadschn/letsencrypt-webapp-renewer#configuration.

.PARAMETER LetsEncryptSubscriptionId
Required. The ID of the subscription containing the letsencrypt-webapp-renewer Web App.

.PARAMETER LetsEncryptResourceGroup
Required. The name of the resource group containing the letsencrypt-webapp-renewer Web App.

.PARAMETER LetsEncryptWebApp
Required. The name of the letsencrypt-webapp-renewer Web App.

.PARAMETER SubscriptionId
Required. The ID of the subscription containing the Web App for which the certificate will be generated.

.PARAMETER ResourceGroup
Required. The name of the resource group containing the Web App for which the certificate will be generated.

.PARAMETER TenantId
Required. The ID of the tenant containing the letsencrypt-webapp-renewer Web App and the Web App(s) for which the certificate(s) will be generated.

.PARAMETER WebApp
Required. A semicolon-delimited list of App Service names for which the certificates will be generated.

.PARAMETER Hosts
Required. The hostnames to add to the SAN certificate(s).

Remarks: For multiple web apps, e.g. "foo;bar", use this syntax:

<WebAppName>:<Hostname>;<Hostname>,<WebAppName>:<Hostname>;<Hostname>.

Example: -Hosts "foo:foo.com;www.foo.com,bar:bar.com;www.bar.com"

.PARAMETER ClientId
Required. The ID of the AAD Service Principal for the WebJob.

.PARAMETER ClientSecret
Required. The token of the AAD Service Principal for the WebJob.

.PARAMETER Email
Required. E-mail for Let's Encrypt registration and expiry notifications.

.PARAMETER ServicePlanResourceGroup
Optional. The name of the Service Plan Resource Group.

Default: The value of the ResourceGroup parameter.

.PARAMETER UseIpBasedSsl
Optional. Indicates whether to use IP Based SSL.

Default: false

.PARAMETER RsaKeyLength
Optional. The length of the certificate's RSA key.

Default: 2048

.PARAMETER AcmeBaseUri
Optional. The ACME base URI.

Default: https://acme-v02.api.letsencrypt.org/directory
Staging: https://acme-staging-v02.api.letsencrypt.org/directory

.PARAMETER WebRootPath
Optional. Web Root Path for the HTTP challenge answer.

.PARAMETER RenewXNumberOfDaysBeforeExpiration
Optional. The number of days before certificate expiry to renew.

Default: -1
Remarks: Use a negative value to force renewal regardless of certificate expiry time.

.PARAMETER AzureDnsZoneName
Optional. The name of the Azure DNS Zone (e.g. domain.com)

.PARAMETER AzureDnsRelativeRecordSetName
Optional. The name of the Azure DNS Relative Record Set (e.g. subdomain).

.PARAMETER AzureDnsTenantId
Optional. The ID of the Azure DNS Tenant ID.

Default: The value of the TenantId parameter.

.PARAMETER AzureDnsSubscriptionId
Optional. The ID of the Azure DNS Subscription.

Default: The value of the SubscriptionId parameter.

.PARAMETER AzureDnsResourceGroup
Optional. The name of the Azure DNS Resource Group.

Default: The value of the ResourceGroup parameter.

.PARAMETER AzureDnsClientId
Optional. The ID of the Azure DNS Client for the WebJob.

Default: The value of the ClientId parameter.

.PARAMETER AzureDnsClientSecret
Optional. The token of the Azure DNS Client Secret for the WebJob.

Default: The value of the ClientSecret parameter.

.PARAMETER SendGridApiKey
Optional. The SendGrid API key for sending email notifications.

#>

Param(
  [Parameter(Mandatory=$true)]
  [string]$LetsEncryptSubscriptionId,

  [Parameter(Mandatory=$true)]
  [string]$LetsEncryptResourceGroup,

  [Parameter(Mandatory=$true)]
  [string]$LetsEncryptWebApp,

  [Parameter(Mandatory=$true)]
  [string]$WebApp,

  [Parameter(Mandatory=$true)]
  [string]$Hosts,

  [Parameter(Mandatory=$true)]
  [string]$SubscriptionId,

  [Parameter(Mandatory=$true)]
  [string]$ResourceGroup,

  [Parameter(Mandatory=$true)]
  [string]$TenantId,

  [Parameter(Mandatory=$true)]
  [string]$ClientId,

  [Parameter(Mandatory=$true)]
  [string]$ClientSecret,

  [Parameter(Mandatory=$true)]
  [string]$Email,

  [Parameter(Mandatory=$false)]
  [string]$ServicePlanResourceGroup,

  [Parameter(Mandatory=$false)]
  [bool]$UseIpBasedSsl,

  [Parameter(Mandatory=$false)]
  [int]$RsaKeyLength,

  [Parameter(Mandatory=$false)]
  [string]$AcmeBaseUri,

  [Parameter(Mandatory=$false)]
  [string]$WebRootPath,

  [Parameter(Mandatory=$false)]
  [int]$RenewXNumberOfDaysBeforeExpiration,

  [Parameter(Mandatory=$false)]
  [string]$AzureDnsZoneName,

  [Parameter(Mandatory=$false)]
  [string]$AzureDnsRelativeRecordSetName,

  [Parameter(Mandatory=$false)]
  [string]$AzureDnsTenantId,

  [Parameter(Mandatory=$false)]
  [string]$AzureDnsSubscriptionId,

  [Parameter(Mandatory=$false)]
  [string]$AzureDnsResourceGroup,

  [Parameter(Mandatory=$false)]
  [string]$AzureDnsClientId,

  [Parameter(Mandatory=$false)]
  [string]$AzureDnsClientSecret,

  [Parameter(Mandatory=$false)]
  [string]$SendGridApiKey
)



#- Load functions -----------------------------------------------------------------------------------------------------------------------------------
Function Set-LetsEncryptConfig {
  Param(
    [Parameter(Mandatory=$true)]
    [hashtable]$AppSettings,

    [Parameter(Mandatory=$true)]
    [string]$WebApp,

    [Parameter(Mandatory=$true)]
    [string]$Name,

    [Parameter(Mandatory=$false)]
    [object]$Value
  )

  If ($Value) {
    $WebApps = $WebApp.Split(";")
    $WebApps | ForEach-Object {
      $WebApp = $_

      If ($Name.ToLower() -eq "hosts") {
        $Items = $Value.Split(",")
        $Items | ForEach-Object {
          $Item = $_

          $AppService = $Item.Split(":")[0]
          $NewValue = $Item.Split(":")[1]

          If ($AppService -eq $WebApp) {
            Register-Setting -AppSettings $AppSettings -WebApp $WebApp -Name $Name -Value $NewValue
          }
        }
      } Else {
        Register-Setting -AppSettings $AppSettings -WebApp $WebApp -Name $Name -Value $Value
      }
    }
  } Else {
    Write-Information "Value not provided for optional app setting '$Name' - skipping..."
  }
}



Function Register-Setting {
  Param (
    [Parameter(Mandatory=$true)]
    [hashtable]$AppSettings,

    [Parameter(Mandatory=$true)]
    [string]$WebApp,

    [Parameter(Mandatory=$true)]
    [string]$Name,

    [Parameter(Mandatory=$true)]
    [string]$Value
  )

  Write-Information "Setting '$Name' to '$Value'..."
  $AppSettings["$LetsEncryptPrefix$WebApp-$Name"] = $Value.ToString()
}



#- Start script -------------------------------------------------------------------------------------------------------------------------------------
Set-StrictMode -Version Latest
Clear

$InformationPreference = "Continue"
$ErrorActionPreference = "Stop"
$LetsEncryptPrefix = "letsencrypt:"

Write-Information "Signing in to Azure Resource Manager account (use the account that contains your Let's Encrypt renewal web app)..."
Login-AzureRmAccount

Write-Information "Setting context to the Let's Encrypt subscription ID..."
Set-AzureRmContext -SubscriptionId $LetsEncryptSubscriptionId

Write-Information "Reading existing Let's Encrypt app settings..."
$LetsEncryptWebAppInfo = Get-AzureRmWebApp -ResourceGroupName $LetsEncryptResourceGroup -Name $LetsEncryptWebApp

Write-Information "Updating existing app settings..."
$UpdatedAppSettings = @{}
ForEach ($AppSetting in $LetsEncryptWebAppInfo.SiteConfig.AppSettings) {
  $UpdatedAppSettings[$AppSetting.Name] = $AppSetting.Value
}

Write-Information "Adding new app settings..."

$WebAppsKey = $LetsEncryptPrefix + "webApps"

If ($UpdatedAppSettings.ContainsKey($WebAppsKey)) {
  $CurrentWebApps = $UpdatedAppSettings[$WebAppsKey].Split(";")

  $WebApps = $WebApp.Split(";")
  $WebApps | ForEach-Object {
    $NewWebApp = $_

    If ($CurrentWebApps -notcontains $NewWebApp) {
      $CurrentWebApps += $NewWebApp
    }
  }

  $UpdatedAppSettings[$WebAppsKey] = $CurrentWebApps -join ";"
} Else {
  $UpdatedAppSettings[$WebAppsKey] = $WebApp
}

Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "acmeBaseUri"                        -Value $AcmeBaseUri
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "azureDnsClientId"                   -Value $AzureDnsClientId
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "azureDnsRelativeRecordSetName"      -Value $AzureDnsRelativeRecordSetName
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "azureDnsResourceGroup"              -Value $AzureDnsResourceGroup
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "azureDnsSubscriptionId"             -Value $AzureDnsSubscriptionId
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "azureDnsTenantId"                   -Value $AzureDnsTenantId
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "azureDnsZoneName"                   -Value $AzureDnsZoneName
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "clientId"                           -Value $ClientId
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "email"                              -Value $Email
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "hosts"                              -Value $Hosts
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "renewXNumberOfDaysBeforeExpiration" -Value $RenewXNumberOfDaysBeforeExpiration
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "resourceGroup"                      -Value $ResourceGroup
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "rsaKeyLength"                       -Value $RsaKeyLength
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "servicePlanResourceGroup"           -Value $ServicePlanResourceGroup
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "subscriptionId"                     -Value $SubscriptionId
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "tenantId"                           -Value $TenantId
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "useIpBasedSsl"                      -Value $UseIpBasedSsl
Set-LetsEncryptConfig -AppSettings $UpdatedAppSettings -WebApp $WebApp -Name "webRootPath"                        -Value $WebRootPath

Write-Information "Copying over existing connection strings..."

$UpdatedConnectionStrings = @{}

ForEach ($ConnectionString in $LetsEncryptWebAppInfo.SiteConfig.ConnectionStrings) {
  $UpdatedConnectionStrings[$ConnectionString.Name] = @{ Type = $ConnectionString.Type.ToString(); Value = $ConnectionString.ConnectionString }
}

Write-Information "Adding new connection string..."
If ($ClientSecret) {
  $WebApps = $WebApp.Split(";")
  $WebApps | ForEach-Object {
    $NewWebApp = $_
    $UpdatedConnectionStrings["$LetsEncryptPrefix$NewWebApp-clientSecret"] = @{ Type = "Custom"; Value = $ClientSecret }
  }
}

If ($AzureDnsClientSecret) {
  $WebApps = $WebApp.Split(";")
  $WebApps | ForEach-Object {
    $NewWebApp = $_
    $UpdatedConnectionStrings["$LetsEncryptPrefix$NewWebApp-azureDnsClientSecret"] = @{ Type = "Custom"; Value = $AzureDnsClientSecret }
  }
}

If ($SendGridApiKey) {
  $UpdatedConnectionStrings[$LetsEncryptPrefix + "SendGridApiKey"] = @{ Type = "Custom"; Value = $SendGridApiKey }
}

Write-Information "Updating settings..."
Set-AzureRmWebApp -ResourceGroupName $LetsEncryptResourceGroup -Name $LetsEncryptWebApp -AppSettings $UpdatedAppSettings -ConnectionStrings $UpdatedConnectionStrings

Write-Information "Let's Encrypt settings updated successfully"
