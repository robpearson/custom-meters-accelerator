# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for license information.

#
# Powershell script to deploy the resources - Customer portal, Publisher portal and the Azure SQL Database
#

#.\Deploy.ps1 `
# -WebAppNamePrefix "amp_saas_accelerator_<unique>" `
# -Location "<region>" `
# -PublisherAdminUsers "<your@email.address>"

Param(  
   [string][Parameter(Mandatory)]$WebAppNamePrefix, # Prefix used for creating web applications
   [string][Parameter()]$ResourceGroupForDeployment, # Name of the resource group to deploy the resources
   [string][Parameter(Mandatory)]$Location, # Location of the resource group
   [string][Parameter(Mandatory)]$PublisherAdminUsers, # Provide a list of email addresses (as comma-separated-values) that should be granted access to the Publisher Portal
   [string][Parameter()]$TenantID, # The value should match the value provided for Active Directory TenantID in the Technical Configuration of the Transactable Offer in Partner Center
   [string][Parameter()]$AzureSubscriptionID, # Subscription where the resources be deployed
   [string][Parameter()]$ADApplicationID, # The value should match the value provided for Active Directory Application ID in the Technical Configuration of the Transactable Offer in Partner Center
   [string][Parameter()]$ADApplicationSecret, # Secret key of the AD Application
   [string][Parameter()]$PCADApplicationID, # PC Active Directory Application ID
   [string][Parameter()]$PCADApplicationSecret, # PC Active Directory Application ID
   [string][Parameter()]$PCTenantID, # PC Active Directory Application ID
   [string][Parameter()]$SQLServerName, # Name of the database server (without database.windows.net)
   [string][Parameter()]$SQLDatabaseName, # Name of the database (Defaults to AMPSaaSDB)
   [string][Parameter()]$SQLAdminLoginPassword, # SQL Admin login password
   [string][Parameter()]$SQLAdminLogin, # SQL Admin login
   [string][Parameter()]$LogoURLpng,  # URL for Publisher .png logo
   [string][Parameter()]$LogoURLico,  # URL for Publisher .ico logo
   [string][Parameter()]$KeyVault, # Name of KeyVault
   [switch][Parameter()]$Quiet #if set, only show error / warning output from script commands
)

# Make sure to install Az Module before running this script
# Install-Module Az
# Install-Module -Name AzureAD

$ErrorActionPreference = "Stop"
$startTime = Get-Date
#region Set up Variables and Default Parameters

if ($ResourceGroupForDeployment -eq "") {
    $ResourceGroupForDeployment = $WebAppNamePrefix 
}

if ($SQLServerName -eq "") {
    $SQLServerName = $WebAppNamePrefix + "-sql"
}
if ($SQLAdminLogin -eq "") {
    $SQLAdminLogin = "saasdbadmin" + $(Get-Random -Minimum 1 -Maximum 1000)
}
if ($SQLAdminLoginPassword -eq "") {
    $SQLAdminLoginPassword = ([System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid))))+"="
}
if ($SQLDatabaseName -eq "") {
    $SQLDatabaseName = "custommetersdb"
}



if($KeyVault -eq "")
{
   $KeyVault=$WebAppNamePrefix+"-kv"
}

$AMAApiConfiguration_CodeHash= git log --format='%H' -1
$azCliOutput = if($Quiet){'none'} else {'json'}

#endregion

#region Validate Parameters

if($WebAppNamePrefix.Length -gt 21) {
    Throw "🛑 Web name prefix must be less than 21 characters."
    Exit
}


if(!($KeyVault -match "^[a-zA-Z][a-z0-9-]+$")) {
    Throw "🛑 KeyVault name only allows alphanumeric and hyphens, but cannot start with a number or special character."
    Exit
}
#endregion 

Write-Host "Starting AMA Scheduler Deployment..."

#region Select Tenant / Subscription for deployment

$currentContext = az account show | ConvertFrom-Json
$currentTenant = $currentContext.tenantId
$currentSubscription = $currentContext.id

#Get TenantID if not set as argument
if(!($TenantID)) {    
    Get-AzTenant | Format-Table
    if (!($TenantID = Read-Host "⌨  Type your TenantID or press Enter to accept your current one [$currentTenant]")) { $TenantID = $currentTenant }    
}
else {
    Write-Host "🔑 Tenant provided: $TenantID"
}

#Get Azure Subscription if not set as argument
if(!($AzureSubscriptionID)) {    
    Get-AzSubscription -TenantId $TenantID | Format-Table
    if (!($AzureSubscriptionID = Read-Host "⌨  Type your SubscriptionID or press Enter to accept your current one [$currentSubscription]")) { $AzureSubscriptionID = $currentSubscription }
}
else {
    Write-Host "🔑 Azure Subscription provided: $AzureSubscriptionID"
}

#Set the AZ Cli context
az account set -s $AzureSubscriptionID
Write-Host "🔑 Azure Subscription '$AzureSubscriptionID' selected."

#endregion


#region Check If KeyVault Exists

$KeyVaultApiUri="https://management.azure.com/subscriptions/$AzureSubscriptionID/providers/Microsoft.KeyVault/checkNameAvailability?api-version=2019-09-01"
$KeyVaultApiBody='{"name": "'+$KeyVault+'","type": "Microsoft.KeyVault/vaults"}'

$kv_check=az rest --method post --uri $KeyVaultApiUri --headers 'Content-Type=application/json' --body $KeyVaultApiBody | ConvertFrom-Json


if( $kv_check.reason -eq "AlreadyExists")
{
	Write-Host ""
	Write-Host "🛑 KeyVault name "  -NoNewline -ForegroundColor Red
	Write-Host "$KeyVault"  -NoNewline -ForegroundColor Red -BackgroundColor Yellow
	Write-Host " already exists." -ForegroundColor Red
	Write-Host "To Purge KeyVault please use the following doc:"
	Write-Host "https://learn.microsoft.com/en-us/cli/azure/keyvault?view=azure-cli-latest#az-keyvault-purge."
	Write-Host "You could use new KeyVault name by using parameter" -NoNewline 
	Write-Host " -KeyVault"  -ForegroundColor Green
 
	Read-Host -Prompt "Press any key to continue or CTRL+C to quit" | Out-Null
}


#region Dowloading assets if provided

# Download Publisher's PNG logo
if($LogoURLpng) { 
    Write-Host "📷 Logo image provided"
	Write-Host "   🔵 Downloading Logo image file"
    Invoke-WebRequest -Uri $LogoURLpng -OutFile "../src/AdminSite/wwwroot/contoso-sales.png"
    Write-Host "   🔵 Logo image downloaded"
}

# Download Publisher's FAVICON logo
if($LogoURLico) { 
    Write-Host "📷 Logo icon provided"
	Write-Host "   🔵 Downloading Logo icon file"
    Invoke-WebRequest -Uri $LogoURLico -OutFile "../src/AdminSite/wwwroot/favicon.ico"
    Write-Host "   🔵 Logo icon downloaded"
}

#endregion
 
#region Create AAD App Registrations

#Record the current ADApps to reduce deployment instructions at the end
$ISADMTApplicationIDProvided = $ADApplicationID

#Create  App Registration for AMA Metered Submission
if (!($ADApplicationID)) {  
    Write-Host "🔑 Creating AMA Metered API App Registration"
    try {
	
		$appCreateRequestBodyJson = @"
{
	"displayName" : "$WebAppNamePrefix-meteredMeter",
	"api": 
	{
		"requestedAccessTokenVersion" : 2
	},
	"signInAudience" : "AzureADandPersonalMicrosoftAccount",
	"web":
	{ 
		"redirectUris": 
		[
			"https://$WebAppNamePrefix-admin.azurewebsites.net",
			"https://$WebAppNamePrefix-admin.azurewebsites.net/",
			"https://$WebAppNamePrefix-admin.azurewebsites.net/Home/Index",
			"https://$WebAppNamePrefix-admin.azurewebsites.net/Home/Index/"
		],
		"logoutUrl": "https://$WebAppNamePrefix-admin.azurewebsites.net/logout",
		"implicitGrantSettings": 
			{ "enableIdTokenIssuance" : true }
	},
	"requiredResourceAccess":
	[{
		"resourceAppId": "00000003-0000-0000-c000-000000000000",
		"resourceAccess":
			[{ 
				"id": "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
				"type": "Scope" 
			}]
	}]
}
"@	
		if ($PsVersionTable.Platform -ne 'Unix') {
			#On Windows, we need to escape quotes and remove new lines before sending the payload to az rest. 
			# See: https://github.com/Azure/azure-cli/blob/dev/doc/quoting-issues-with-powershell.md#double-quotes--are-lost
			$appCreateRequestBodyJson = $appCreateRequestBodyJson.replace('"','\"').replace("`r`n","")
		}

		$ADApplication = $(az rest --method POST --headers "Content-Type=application/json" --uri https://graph.microsoft.com/v1.0/applications --body $appCreateRequestBodyJson  ) | ConvertFrom-Json
	
		$ADApplicationID = $ADApplication.appId
		$ADObjectID = $ADApplication.id
	
        sleep 5 #this is to give time to AAD to register
        $ADApplicationSecret = az ad app credential reset --id $ADApplicationID --append --display-name 'AMAMeteredMeter' --years 2 --query password --only-show-errors --output tsv
				
        Write-Host "   🔵 AMA App Registration created."
		Write-Host "      ➡️ Application ID:" $ADApplicationID  
        Write-Host "      ➡️ App Secret:" $ADApplicationSecret

		# Download Publisher's AppRegistration logo
        if($LogoURLpng) { 
			Write-Host "   🔵 Logo image provided. Setting the Application branding logo"
			Write-Host "      ➡️ Setting the Application branding logo"
			$token=(az account get-access-token --resource "https://graph.microsoft.com" --query accessToken --output tsv)
			$logoWeb = Invoke-WebRequest $LogoURLpng
			$logoContentType = $logoWeb.Headers["Content-Type"]
			$logoContent = $logoWeb.Content
			
			$uploaded = Invoke-WebRequest `
			  -Uri "https://graph.microsoft.com/v1.0/applications/$ADObjectID/logo" `
			  -Method "PUT" `
			  -Header @{"Authorization"="Bearer $token";"Content-Type"="$logoContentType";} `
			  -Body $logoContent
		    
			Write-Host "      ➡️ Application branding logo set."
        }

		$PC

    }
    catch [System.Net.WebException],[System.IO.IOException] {
        Write-Host "🚨🚨   $PSItem.Exception"
        break;
    }
}

if(!($PCADApplicationID))
{
	Write-host "📜 Use Metered AD as PC AD"	
	$PCADApplicationID=$ADApplicationID
	$PCADApplicationSecret=$ADApplicationSecret
	$PCTenantID=$TenantID
}
#endregion

#region Prepare Code Packages
Write-host "📜 Prepare publish files for the application"
if (!(Test-Path '../Publish')) {		
	Write-host "   🔵 Preparing Admin Site"  
	dotnet publish ../src/AdminSite/AdminSite.csproj -c debug -o ../Publish/AdminSite/ -v q

	Write-host "   🔵 Preparing Metered Scheduler"
	dotnet publish ../src/MeteredTriggerJob/MeteredTriggerJob.csproj -c debug -o ../Publish/AdminSite/app_data/jobs/triggered/MeteredTriggerJob/ -v q --runtime win-x64 --self-contained true 

	Write-host "   🔵 Zipping packages"
	Compress-Archive -Path ../Publish/AdminSite/* -DestinationPath ../Publish/AdminSite.zip -Force

}
#endregion

#region Deploy Azure Resources Infrastructure
Write-host "☁ Deploy Azure Resources"

#Set-up resource name variables
$webAppNameService=$WebAppNamePrefix+"-asp"
$webAppNameAdmin=$WebAppNamePrefix+"-admin"
$vnetName=$WebAppNamePrefix+"-net"
$subnetName=$WebAppNamePrefix+"-default"
$subnetWebName=$WebAppNamePrefix+"-web"
$privateEndpointName=$WebAppNamePrefix+"-db-pe"
$privateDnsZoneName="privatelink.database.windows.net"
$privatelink =$WebAppNamePrefix+"-db-link"
$ServerUri = $SQLServerName+".database.windows.net"
$ServerUriPrivate = $SQLServerName+".privatelink.database.windows.net"
$Connection="Server=tcp:"+$ServerUriPrivate+";Database="+$SQLDatabaseName+";TrustServerCertificate=True;Authentication=Active Directory Managed Identity;"

$ADApplicationSecretKeyVault="@Microsoft.KeyVault(VaultName=$KeyVault;SecretName=ADApplicationSecret)"
$PCADApplicationSecretKeyVault="@Microsoft.KeyVault(VaultName=$KeyVault;SecretName=PCADApplicationSecret)"
# $DefaultConnectionKeyVault="@Microsoft.KeyVault(VaultName=$KeyVault;SecretName=DefaultConnection) "
$aadAdminObjectId=az ad signed-in-user show --query id -o tsv
$aadAdminLogin=az ad signed-in-user show --query mail -o tsv

Write-host "   🔵 Resource Group"
Write-host "      ➡️ Create Resource Group"
az group create --location $Location --name $ResourceGroupForDeployment --output $azCliOutput

# Create a virtual network and a subnet
az network vnet create --name $vnetName --resource-group $ResourceGroupForDeployment --location $location --address-prefix 10.0.0.0/16
az network vnet subnet create --name $subnetName --resource-group $ResourceGroupForDeployment --vnet-name $vnetName --address-prefixes 10.0.1.0/24
az network vnet subnet create --name $subnetWebName --resource-group $ResourceGroupForDeployment --vnet-name $vnetName --address-prefixes 10.0.2.0/24

$Sig= ([System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid))))


Write-host "   🔵 SQL Server"
Write-host "      ➡️ Create Sql Server"
az sql server create --name $SQLServerName --resource-group $ResourceGroupForDeployment --location $Location --admin-user $SQLAdminLogin --admin-password $SQLAdminLoginPassword --output $azCliOutput
Write-host "      ➡️ Set minimalTlsVersion to 1.2"
az sql server update --name $SQLServerName --resource-group $ResourceGroupForDeployment --set minimalTlsVersion="1.2"
Write-host "      ➡️ Create SQL DB"
az sql db create --resource-group $ResourceGroupForDeployment --server $SQLServerName --name $SQLDatabaseName  --edition Standard  --capacity 10 --zone-redundant false --output $azCliOutput

# Set AAD admin
az sql server ad-admin create --server $SQLServerName --resource-group $ResourceGroupForDeployment --display-name $aadAdminLogin --object-id $aadAdminObjectId

Write-host "      ➡️ Add SQL Server Firewall rules"
az sql server firewall-rule create --resource-group $ResourceGroupForDeployment --server $SQLServerName -n AllowAzureIP --start-ip-address "0.0.0.0" --end-ip-address "0.0.0.0" --output $azCliOutput

Write-host "   🔵 KeyVault"
Write-host "      ➡️ Create KeyVault"
az keyvault create --name $KeyVault --resource-group $ResourceGroupForDeployment --output $azCliOutput
Write-host "      ➡️ Add Secrets"

if($ADApplicationSecret)
{
	az keyvault secret set --vault-name $KeyVault  --name ADApplicationSecret --value $ADApplicationSecret --output $azCliOutput
}
else {
	Write-Error "  Single Tenant Secret could not be added to KeyVault since it is blank. Please add it manually."
}

if($PCADApplicationSecret)
{
	az keyvault secret set --vault-name $KeyVault  --name PCADApplicationSecret --value $PCADApplicationSecret --output $azCliOutput
}
else {
	Write-Error "  PC Secret could not be added to KeyVault since it is blank. Please add it manually."
}


Write-host "   🔵 App Service Plan"
Write-host "      ➡️ Create App Service Plan"
az appservice plan create -g $ResourceGroupForDeployment -n $webAppNameService --sku B1 --output $azCliOutput

Write-host "   🔵 Scheduler WebApp"
Write-host "      ➡️ Create Web App"
az webapp create -g $ResourceGroupForDeployment -p $webAppNameService -n $webAppNameAdmin  --runtime dotnet:6 --output $azCliOutput

Write-host "      ➡️ Assign Identity"
$webAppNameAdminId = az webapp identity assign -g $ResourceGroupForDeployment  -n $webAppNameAdmin --identities [system] --query principalId -o tsv

Write-host "      ➡️ Set Configuration"
az webapp config connection-string set -g $ResourceGroupForDeployment -n $webAppNameAdmin -t SQLAzure --output $azCliOutput --settings DefaultConnection=$Connection 
az webapp config appsettings set -g $ResourceGroupForDeployment  -n $webAppNameAdmin --output $azCliOutput --settings AdAuthenticationEndPoint=https://login.microsoftonline.com/ KnownUsers=$PublisherAdminUsers Marketplace_Uri=https://marketplaceapi.microsoft.com/api/usageEvent?api-version=2018-08-31 GrantType=client_credentials ClientId=$ADApplicationID ClientSecret=$ADApplicationSecretKeyVault TenantId=$TenantID Scope=20e940b3-4c77-4b0b-9a53-9e16a1b010a7/.default  PC_ClientId=$PCADApplicationID PC_ClientSecret=$PCADApplicationSecretKeyVault PC_TenantId=$PCTenantID PC_Scope=https://api.partner.microsoft.com/.default Signature=$Sig AMAApiConfiguration_CodeHash=$AMAApiConfiguration_CodeHash 
az webapp config set -g $ResourceGroupForDeployment -n $webAppNameAdmin --always-on true  --output $azCliOutput

#endregion

#region Deploy Code
Write-host "📜 Deploy Code"

Write-host "   🔵 Deploy Code to Admin Portal"
az webapp deploy --resource-group $ResourceGroupForDeployment --name $webAppNameAdmin --src-path "../Publish/AdminSite.zip" --type zip --output $azCliOutput

Write-host "   🔵 Integrate with web"
az webapp vnet-integration add --name $webAppNameAdmin --resource-group $ResourceGroupForDeployment --vnet $vnetName --subnet $subnetWebName

Write-host "   🔵 Set KeyVault to selected Subnet"

az keyvault set-policy --name $KeyVault  --resource-group $ResourceGroupForDeployment --subscription $AzureSubscriptionID --object-id $webAppNameAdminId --secret-permissions get list --key-permissions get list --output $azCliOutput

az network vnet subnet update --resource-group $ResourceGroupForDeployment --vnet-name $vnetName --name $subnetWebName --service-endpoints "Microsoft.KeyVault"

$subnetid=$(az network vnet subnet show --resource-group $ResourceGroupForDeployment --vnet-name $vnetName --name $subnetWebName --query id --output tsv)
az keyvault network-rule add --resource-group $ResourceGroupForDeployment --name $KeyVault --subnet $subnetid

az keyvault update --resource-group $ResourceGroupForDeployment --name $KeyVault --bypass AzureServices

az keyvault update --resource-group $ResourceGroupForDeployment --name $KeyVault --default-action Deny

Write-host "      ➡️ Login into SQL Server"

$token = az account get-access-token --resource="https://database.windows.net" --query accessToken --output tsv

$queryAddUser="CREATE USER ["+$webAppNameAdmin+"] FROM EXTERNAL PROVIDER"
$queryAlterUser1="ALTER ROLE db_datareader ADD MEMBER ["+$webAppNameAdmin+"];"
$queryAlterUser2="ALTER ROLE db_ddladmin ADD MEMBER ["+$webAppNameAdmin+"];"
$queryAlterUser3=" ALTER ROLE db_datawriter ADD MEMBER ["+$webAppNameAdmin+"];"


Write-host "      ➡️ Add WebApp MSI to SQL Server"

Invoke-SqlCmd -ServerInstance $ServerUri  -Database $SQLDatabaseName -AccessToken $token -Query $queryAddUser
Invoke-Sqlcmd -ServerInstance $ServerUri -database $SQLDatabaseName   -Query $queryAlterUser1 -AccessToken $token
Invoke-Sqlcmd -ServerInstance $ServerUri -database $SQLDatabaseName   -Query $queryAlterUser2 -AccessToken $token
Invoke-Sqlcmd -ServerInstance $ServerUri -database $SQLDatabaseName   -Query $queryAlterUser3 -AccessToken $token

Write-host "      ➡️ Execute SQL schema/data script"
Invoke-Sqlcmd -ServerInstance $ServerUri -database $SQLDatabaseName  -inputfile "./schema.sql"  -AccessToken $token


#Setup Private Endpoint

# Get SQL Server
$sqlServerId=az sql server show --name $SQLServerName --resource-group $ResourceGroupForDeployment --query id -o tsv

# Create a private endpoint
az network private-endpoint create --name $privateEndpointName --resource-group $ResourceGroupForDeployment --vnet-name $vnetName --subnet $subnetName --private-connection-resource-id $sqlServerId --group-ids sqlServer --connection-name sqlConnection


# Create a private DNS zone
az network private-dns zone create --name $privateDnsZoneName --resource-group $ResourceGroupForDeployment

# Link the private DNS zone to the VNet
az network private-dns link vnet create --name $privatelink --resource-group $ResourceGroupForDeployment --virtual-network $vnetName --zone-name $privateDnsZoneName --registration-enabled false

az network private-endpoint dns-zone-group create --resource-group $ResourceGroupForDeployment --endpoint-name $privateEndpointName --name "sql-zone-group"   --private-dns-zone $privateDnsZoneName   --zone-name "sqlserver"


if ($env:ACC_CLOUD -eq $null){
    Write-host "      ➡️ Running in local environment - Add current IP to firewall"
	$publicIp = (Invoke-WebRequest -uri "http://ifconfig.me/ip").Content
    az sql server firewall-rule create --resource-group $ResourceGroupForDeployment --server $SQLServerName -n AllowIP --start-ip-address "$publicIp" --end-ip-address "$publicIp" --output $azCliOutput
}

#Enable Private Endpoint


Write-host "   🔵 Clean up"
Remove-Item -Path ../Publish -recurse -Force

#endregion

#region Present Output

Write-host "✅ If the intallation completed without error complete the folllowing checklist:"
if ($ISADMTApplicationIDProvided) {  #If provided then show the user where to add the landing page in AAD, otherwise script did this already for the user.
	Write-host "   🔵 Add The following URLs to the metered Meter AAD App Registration in Azure Portal:"
	Write-host "      ➡️ https://$WebAppNamePrefix-admin.azurewebsites.net"
	Write-host "      ➡️ https://$WebAppNamePrefix-admin.azurewebsites.net/"
	Write-host "      ➡️ https://$WebAppNamePrefix-admin.azurewebsites.net/Home/Index"
	Write-host "      ➡️ https://$WebAppNamePrefix-admin.azurewebsites.net/Home/Index/"
	Write-host "   🔵 Verify ID Tokens checkbox has been checked-out ?"
}

Write-host "   🔵 Add The following AAD info in PartnerCenter AMA Technical Configuration"
Write-host "      ➡️ Tenant ID:                  $TenantID"
Write-host "      ➡️ AAD Application ID section: $ADApplicationID"
Write-host ""
Write-host "   🔵 Add The following AAD in PartnerCenter User Management Section"
Write-host "      ➡️ Tenant ID:                  $PCTenantID"
Write-host "      ➡️ AAD Application ID section: $PCADApplicationID"
Write-host ""
Write-host "   🔵 Add The following URL in PartnerCenter AMA Plan Technical Configuration"
Write-host "      ➡️ Notification Webhook section: https://$WebAppNamePrefix-admin.azurewebsites.net/api?sig=$Sig"
Write-host ""
Write-host "   🔵 Use the Following WebApp to access the Admin Portal"
Write-host "      ➡️ Admin Portal: https://$WebAppNamePrefix-admin.azurewebsites.net"




$duration = (Get-Date) - $startTime
Write-Host "Deployment Complete in $($duration.Minutes)m:$($duration.Seconds)s"
Write-Host "DO NOT CLOSE THIS SCREEN.  Please make sure you copy or perform the actions above before closing."
#endregion
