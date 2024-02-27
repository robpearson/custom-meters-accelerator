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

$currentContext = az account show | ConvertFrom-Json
$currentTenant = $currentContext.tenantId
$currentSubscription = $currentContext.id

#Set the AZ Cli context
az account set -s $AzureSubscriptionID
Write-Host "🔑 Azure Subscription '$AzureSubscriptionID' selected."

#endregion

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

$ServerUri = $SQLServerName+".privatelink.database.windows.net"
$Connection="Server=tcp:"+$ServerUri+";Database="+$SQLDatabaseName+";TrustServerCertificate=True;Authentication=Active Directory Managed Identity;"

$aadAdminObjectId=az ad signed-in-user show --query id -o tsv
$aadAdminLogin=az ad signed-in-user show --query mail -o tsv


Write-host "      ➡️ Assign Identity"
$webAppNameAdminId = az webapp identity assign -g $ResourceGroupForDeployment  -n $webAppNameAdmin --identities [system] --query principalId -o tsv

Write-host "      ➡️ Login into SQL Server"

$token = az account get-access-token --resource="https://database.windows.net" --query accessToken --output tsv

$queryAddUser="CREATE USER ["+$webAppNameAdmin+"] FROM EXTERNAL PROVIDER"
$queryAlterUser1="ALTER ROLE db_datareader ADD MEMBER ["+$webAppNameAdmin+"];"
$queryAlterUser2="ALTER ROLE db_ddladmin ADD MEMBER ["+$webAppNameAdmin+"];"
$queryAlterUser3=" ALTER ROLE db_datawriter ADD MEMBER ["+$webAppNameAdmin+"];"


Write-host "      ➡️ Add WebApp MSI to SQL Server"

Invoke-SqlCmd -ServerInstance $SQLServerName  -Database $SQLDatabaseName -AccessToken $token -Query $queryAddUser
Invoke-Sqlcmd -ServerInstance $SQLServerName -database $SQLDatabaseName   -Query $queryAlterUser1 -Username $SQLAdminLogin -Password $SQLAdminLoginPassword 
Invoke-Sqlcmd -ServerInstance $SQLServerName -database $SQLDatabaseName   -Query $queryAlterUser2 -Username $SQLAdminLogin -Password $SQLAdminLoginPassword 
Invoke-Sqlcmd -ServerInstance $SQLServerName -database $SQLDatabaseName   -Query $queryAlterUser3 -Username $SQLAdminLogin -Password $SQLAdminLoginPassword 

Write-host "      ➡️ Execute SQL schema/data script"
Invoke-Sqlcmd -ServerInstance $ServerUri -database $SQLDatabaseName  -inputfile "./schema.sql" -Username $SQLAdminLogin -Password $SQLAdminLoginPassword 


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
