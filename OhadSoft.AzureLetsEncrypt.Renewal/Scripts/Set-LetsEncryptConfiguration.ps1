Param(
	[Parameter(Mandatory=$true)]
	[string]$LetsEncryptSubscriptionId,
	
	[Parameter(Mandatory=$true)]
	[string]$LetsEncryptResourceGroup,

	[Parameter(Mandatory=$true)]
	[string]$LetsEncryptWebApp,

	[Parameter(Mandatory=$true)]
	[string]$SubscriptionId,

	[Parameter(Mandatory=$true)]
	[string]$ResourceGroup,

	[Parameter(Mandatory=$true)]
	[string]$WebApp,

	[Parameter(Mandatory=$false)]
	[string]$ServicePlanResourceGroup = $ResourceGroup,

	[Parameter(Mandatory=$true)]
	[string]$TenantId,

	[Parameter(Mandatory=$true)]
	[string]$ClientId,

	[Parameter(Mandatory=$true)]
	[string]$ClientSecret,

	[Parameter(Mandatory=$true)]
	[string]$Hosts,

	[Parameter(Mandatory=$false)]
	[string]$Email,

	[Parameter(Mandatory=$false)]
	[bool]$UseIpBasedSsl,

	[Parameter(Mandatory=$false)]
	[int]$RsaKeyLength,

	[Parameter(Mandatory=$false)]
	[string]$AcmeBaseUri,

	[Parameter(Mandatory=$false)]
	[int]$RenewXNumberOfDaysBeforeExpiration = -1
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$InformationPreference = "Continue"

$letsEncryptPrefix = "letsencrypt:"
Function Set-LetsEncryptConfig
{
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

	if (!$Value)
	{
		Write-Information "Value not provided for app setting '$Name' - skipping..."
		return
	}

	Write-Information "Setting '$Name' to '$Value'..."
	$AppSettings["$letsEncryptPrefix$WebApp-$Name"] = $Value.ToString()
}

Write-Information "Signing in to Azure Resource Manager account (use the account that contains your Let's Encrypt renewal web app)..."
Login-AzureRmAccount

Write-Information "Setting context to the Let's Encrypt subscription ID..."
Set-AzureRmContext -SubscriptionId $LetsEncryptSubscriptionId

Write-Information "Loading existing Let's Encrypt web app settings..."
$letsEncryptWebAppInfo = Get-AzureRmWebApp -ResourceGroupName $LetsEncryptResourceGroup -Name $LetsEncryptWebApp

Write-Information "Copying over existing app settings..."
$updatedAppSettings = @{}
foreach ($appSetting in $letsEncryptWebAppInfo.SiteConfig.AppSettings) {
    $updatedAppSettings[$appSetting.Name] = $appSetting.Value
}

Write-Information "Adding new settings..."

$webAppsKey = $letsEncryptPrefix + "webApps"
if ($updatedAppSettings.ContainsKey($webAppsKey)) 
{
	if ($updatedAppSettings[$webAppsKey] -notlike "*$WebApp*") 
	{
		$updatedAppSettings[$webAppsKey] = $updatedAppSettings[$webAppsKey] + ";" + $WebApp
	}
}
else 
{
	$updatedAppSettings[$webAppsKey] = $WebApp
}

Set-LetsEncryptConfig $updatedAppSettings $WebApp "subscriptionId" $SubscriptionId
Set-LetsEncryptConfig $updatedAppSettings $WebApp "resourceGroup" $ResourceGroup
Set-LetsEncryptConfig $updatedAppSettings $WebApp "servicePlanResourceGroup" $ServicePlanResourceGroup
Set-LetsEncryptConfig $updatedAppSettings $WebApp "tenantId" $TenantId
Set-LetsEncryptConfig $updatedAppSettings $WebApp "clientId" $ClientId
Set-LetsEncryptConfig $updatedAppSettings $WebApp "hosts" $Hosts
Set-LetsEncryptConfig $updatedAppSettings $WebApp "email" $Email
Set-LetsEncryptConfig $updatedAppSettings $WebApp "useIpBasedSsl" $UseIpBasedSsl
Set-LetsEncryptConfig $updatedAppSettings $WebApp "rsaKeyLength" $RsaKeyLength
Set-LetsEncryptConfig $updatedAppSettings $WebApp "acmeBaseUri" $AcmeBaseUri
Set-LetsEncryptConfig $updatedAppSettings $WebApp "renewXNumberOfDaysBeforeExpiration" $RenewXNumberOfDaysBeforeExpiration

Write-Information "Copying over existing connection strings..."
$updatedConnectionStrings = @{}
foreach ($connectionString in $letsEncryptWebAppInfo.SiteConfig.ConnectionStrings) {
    $updatedConnectionStrings[$connectionString.Name] = @{ Type = $connectionString.Type.ToString(); Value = $connectionString.ConnectionString }
}

Write-Information "Adding new connection string..."
$updatedConnectionStrings["$letsEncryptPrefix$WebApp-clientSecret"] = @{ Type = "Custom"; Value = $ClientSecret }

Write-Information "Updating settings..."
Set-AzureRmWebApp -ResourceGroupName $LetsEncryptResourceGroup -Name $LetsEncryptWebApp -AppSettings $updatedAppSettings -ConnectionStrings $updatedConnectionStrings

Write-Information "Let's Encrypt settings updated successfully"