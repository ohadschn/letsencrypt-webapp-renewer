param(
	[string]$webAppTarget,
	[string]$webAppHosts,
	[string]$renewXNumberOfDaysBeforeExpiration = "-1", # set this to e.g. 65 after first request to avoid hitting the cert-limit if the job is triggerd manually more than 5 times
	[string]$tenantId = "<default tenant Id>",
	[string]$subscriptionId = "<default subscription Id>",
	[string]$webAppLetsEncrypt = "<default letsencrypt web app>",
	[string]$resourceGroupLetsEncrypt = "<default resource group of letsencrypt web app>",
	[string]$clientId = "<default client Id>",
	[string]$clientSecret = "<default client secret>",
	[string]$resourceGroupTarget = "<default target resource group>",
	[string]$email = "<default email>")

Login-AzureRmAccount

Set-AzureRmContext -SubscriptionId $subscriptionId

# Load Existing Web App settings for source and target
$webAppSource = Get-AzureRmWebAppSlot -ResourceGroupName $resourceGroupLetsEncrypt -Name $webAppLetsEncrypt -Slot "production"

# Get reference to the source app settings
$appSettingsSource = $webAppSource.SiteConfig.AppSettings

# Create Hash variable for App Settings
$appSettingsTarget = @{}

# Copy over all Existing App Settings to the Hash
ForEach ($appSettingSource in $appSettingsSource) {
    $appSettingsTarget[$appSettingSource.Name] = $appSettingSource.Value
}

# Add new settings
$appSettingsTarget["letsencrypt:" + $webAppTarget + "-clientId"] = $clientId
$appSettingsTarget["letsencrypt:" + $webAppTarget + "-email"] = $email
$appSettingsTarget["letsencrypt:" + $webAppTarget + "-hosts"] = $webAppHosts
$appSettingsTarget["letsencrypt:" + $webAppTarget + "-renewXNumberOfDaysBeforeExpiration"] = $renewXNumberOfDaysBeforeExpiration
$appSettingsTarget["letsencrypt:" + $webAppTarget + "-resourceGroup"] = $resourceGroupTarget
$appSettingsTarget["letsencrypt:" + $webAppTarget + "-subscriptionId"] = $subscriptionId
$appSettingsTarget["letsencrypt:" + $webAppTarget + "-tenantId"] = $tenantId 
if ($appSettingsTarget.ContainsKey("letsencrypt:webApps")) {
	if (!$appSettingsTarget["letsencrypt:webApps"].ToLower().Contains($webAppTarget.ToLower())) {
		$appSettingsTarget["letsencrypt:webApps"] = $appSettingsTarget["letsencrypt:webApps"] + ";" + $webAppTarget
	}
}
else {
	$appSettingsTarget["letsencrypt:webApps"] = $webAppTarget
}

# Save Settings to Target
Set-AzureRmWebAppSlot -ResourceGroupName $resourceGroupTarget -Name $webAppLetsEncrypt -Slot "production" -AppSettings $appSettingsTarget

# Get reference to the source Connection Strings
$connectionStringsSource = $webAppSource.SiteConfig.ConnectionStrings

# Create Hash variable for Connection Strings
$connectionStringsTarget = @{}

# Copy over all Existing Connection Strings to the Hash
ForEach($connStringSource in $connectionStringsSource) {
    $connectionStringsTarget[$connStringSource.Name] = @{ Type = $connStringSource.Type.ToString(); Value = $connStringSource.ConnectionString }
}

# Add new Connection String
$connectionStringsTarget["letsencrypt:" + $webAppTarget + "-clientSecret"] = @{ Type = "Custom"; Value = $clientSecret }

# Save Connection Strings to Target
Set-AzureRmWebAppSlot -ResourceGroupName $resourceGroupTarget -Name $webAppLetsEncrypt -Slot "production" -ConnectionStrings $connectionStringsTarget