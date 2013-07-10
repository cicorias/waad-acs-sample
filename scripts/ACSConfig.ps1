# Prerequisites:
# Windows Azure Active Directory Module for Windows PowerShell cmdlets (http://aka.ms/aadposh)
# Windows Azure PowerShell (http://go.microsoft.com/?linkid=9811175)
# A local folder C:\Temp
# You should run the WAADConfig.ps1 script first

# You must provide: 
# - Your WAAD tenant name
# - Your ACS namespace name
# - Your ACS management key

# To work with WAAD
Import-Module MSOnline

# To work with ACS
Import-Module WAPPSCmdlets

$WAADName = '[your WAAD tenant name]'
$ACSNamespace = '[your ACS namespace name]'
$ACSManagementkey = '[your ACS Management Key]'

# Connect to WAAD - use the credentials for the Global Administrator you created when you created your WAAD tenant
Connect-MsolService

$ACSServiceAddress = "https://$ACSNamespace.accesscontrol.windows.net/"
$WAADTenantId = (Get-MsolCompanyInformation | Select -ExpandProperty ObjectId).ToString()
$WAADFederationMetaDataAddress = "https://login.windows.net/$WAADTenantId/federationmetadata/2007-06/federationmetadata.xml"
$FedFile = 'C:\Temp\federationmetadata.xml'
$AppAddress = 'https://localhost:44300/'
$RelyingPartyName = 'MyWebApp'
$IdentityProviderName = 'WAAD'
$RuleGroupName = 'WAADRuleGroup'

#  Configure an Identity Provider, a Relying Party, and a Rule Group in your ACS Namespace
Invoke-WebRequest -Uri $WAADFederationMetaDataAddress -OutFile $FedFile
Add-IdentityProvider -Type WsFederation -WsFederationMetadata $FedFile -AllowedRelyingParties $RelyingPartyName -LoginLinkText "WAAD Login" -ManagementKey $ACSManagementkey -Name $IdentityProviderName -Namespace $ACSNamespace
Add-RelyingParty -Name $RelyingPartyName -AllowedIdentityProviders $IdentityProviderName -ManagementKey $ACSManagementkey -Namespace $ACSNamespace -Realm $AppAddress -ReturnUrl $AppAddress -TokenFormat SAML_2_0 -RuleGroupName $RuleGroupName
Add-DefaultPassthroughRules -GroupName $RuleGroupName -IdentityProviderName $IdentityProviderName  -ManagementKey $ACSManagementkey -Namespace $ACSNamespace
Add-Rule -GroupName $RuleGroupName -IdentityProviderName $IdentityProviderName -Description "Assign Mary Jones to the Managers role" -InputClaimType http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name -InputClaimValue "maryj@$WAADName.onmicrosoft.com" -ManagementKey $ACSManagementkey -Namespace $ACSNamespace -OutputClaimType http://schemas.microsoft.com/ws/2008/06/identity/claims/role -OutputClaimValue Managers
Add-Rule -GroupName $RuleGroupName -IdentityProviderName $IdentityProviderName -Description "Assign Fred Bloggs to the Users role" -InputClaimType http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name -InputClaimValue "fredb@$WAADName.onmicrosoft.com" -ManagementKey $ACSManagementkey -Namespace $ACSNamespace -OutputClaimType http://schemas.microsoft.com/ws/2008/06/identity/claims/role -OutputClaimValue Users



# Remove ACS  if you want to tidy up
# Remove-RuleGroup -Name $RuleGroupName -ManagementKey $ACSManagementkey -Namespace $ACSNamespace
# Remove-RelyingParty -Name $RelyingPartyName -ManagementKey $ACSManagementkey -Namespace $ACSNamespace
# Remove-IdentityProvider -Name $IdentityProviderName -ManagementKey $ACSManagementkey -Namespace $ACSNamespace
