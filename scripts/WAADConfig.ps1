# Prerequisites:
# Windows Azure Active Directory Module for Windows PowerShell cmdlets (http://aka.ms/aadposh)

# To work with WAAD
Import-Module MSOnline

$WAADName = '[your WAAD tenant name]'
$ACSNamespace = '[your ACS namespace name]'
$ACSServiceAddress = "https://$ACSNamespace.accesscontrol.windows.net/"

# You may want to change this
$UserPassword = 'P@ssw0rd!'

# Display name for the Service Principal
$ACS_SP_Name = 'ACS_SP'

# Connect to WAAD - use the credentials for the Global Administrator you created when you created your WAAD tenant
Connect-MsolService

# Add some users that don't have to change their passwords when they first log on
New-MsolUser -DisplayName "Fred Bloggs" -FirstName Fred -LastName Bloggs -Password $UserPassword -UserPrincipalName "fredb@$WAADName.onmicrosoft.com" -ForceChangePassword $false
New-MsolUser -DisplayName "Mary Jones" -FirstName Mary -LastName Jones -Password $UserPassword -UserPrincipalName "maryj@$WAADName.onmicrosoft.com" -ForceChangePassword $false

# Set up the ServicePrincipal
$replyurl = New-MsolServicePrincipalAddresses -Address $ACSServiceAddress
New-MsolServicePrincipal -DisplayName $ACS_SP_Name -ServicePrincipalName @($ACSServiceAddress) -Addresses $replyurl

# Remove WAAD Service Principal if you want to tidy up
# Remove-MsolServicePrincipal -ServicePrincipalName $ACSServiceAddress