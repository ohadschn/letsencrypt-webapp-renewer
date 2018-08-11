param(
	[string]$WebAppTarget,
	[string]$WebAppHosts,
	[string]$RenewXNumberOfDaysBeforeExpiration = "-1", # set this to e.g. 65 after first request to avoid hitting the cert-limit if the job is triggerd manually more than 5 times
	[string]$TenantId = "<default tenant Id>",
	[string]$SubscriptionId = "<default subscription Id>",
	[string]$WebAppLetsEncrypt = "<default letsencrypt web app>",
	[string]$ResourceGroupLetsEncrypt = "<default resource group of letsencrypt web app>",
	[string]$ClientId = "<default client Id>",
	[string]$ClientSecret = "<default client secret>",
	[string]$ResourceGroupTarget = "<default target resource group>",
	[string]$Email = "<default email>",
	[string]$WebAppSlot = "production")

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Login-AzureRmAccount

Set-AzureRmContext -SubscriptionId $SubscriptionId

# Load Existing Web App settings for source and target
$webAppSource = Get-AzureRmWebAppSlot -ResourceGroupName $ResourceGroupLetsEncrypt -Name $WebAppLetsEncrypt -Slot $WebAppSlot

# Get reference to the source app settings
$appSettingsSource = $webAppSource.SiteConfig.AppSettings

# Create Hash variable for App Settings
$appSettingsTarget = @{}

# Copy over all Existing App Settings to the Hash
ForEach ($appSettingSource in $appSettingsSource) {
    $appSettingsTarget[$appSettingSource.Name] = $appSettingSource.Value
}

# Add new settings
$letsEncryptPrefix = "letsencrypt:"
$appSettingsTarget[$letsEncryptPrefix + $WebAppTarget + "-clientId"] = $ClientId
$appSettingsTarget[$letsEncryptPrefix + $WebAppTarget + "-email"] = $Email
$appSettingsTarget[$letsEncryptPrefix + $WebAppTarget + "-hosts"] = $WebAppHosts
$appSettingsTarget[$letsEncryptPrefix + $WebAppTarget + "-renewXNumberOfDaysBeforeExpiration"] = $RenewXNumberOfDaysBeforeExpiration
$appSettingsTarget[$letsEncryptPrefix + $WebAppTarget + "-resourceGroup"] = $ResourceGroupTarget
$appSettingsTarget[$letsEncryptPrefix + $WebAppTarget + "-subscriptionId"] = $SubscriptionId
$appSettingsTarget[$letsEncryptPrefix + $WebAppTarget + "-tenantId"] = $TenantId 
if ($appSettingsTarget.ContainsKey("letsencrypt:webApps")) {
	if ($appSettingsTarget["letsencrypt:webApps"].IndexOf($WebAppTarget, StringComparison.OrdinalIgnoreCase) < 0) {
		$appSettingsTarget["letsencrypt:webApps"] = $appSettingsTarget["letsencrypt:webApps"] + ";" + $WebAppTarget
	}
}
else {
	$appSettingsTarget["letsencrypt:webApps"] = $WebAppTarget
}

# Save Settings to Target
Set-AzureRmWebAppSlot -ResourceGroupName $ResourceGroupTarget -Name $WebAppLetsEncrypt -Slot $WebAppSlot -AppSettings $appSettingsTarget

# Get reference to the source Connection Strings
$cConnectionStringsSource = $webAppSource.SiteConfig.ConnectionStrings

# Create Hash variable for Connection Strings
$connectionStringsTarget = @{}

# Copy over all Existing Connection Strings to the Hash
ForEach($ConnStringSource in $connectionStringsSource) {
    $connectionStringsTarget[$ConnStringSource.Name] = @{ Type = $ConnStringSource.Type.ToString(); Value = $ConnStringSource.ConnectionString }
}

# Add new Connection String
$connectionStringsTarget[$letsEncryptPrefix + $WebAppTarget + "-clientSecret"] = @{ Type = "Custom"; Value = $ClientSecret }

# Save Connection Strings to Target
Set-AzureRmWebAppSlot -ResourceGroupName $ResourceGroupTarget -Name $WebAppLetsEncrypt -Slot $WebAppSlot -ConnectionStrings $connectionStringsTarget