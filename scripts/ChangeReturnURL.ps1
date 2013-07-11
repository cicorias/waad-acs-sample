# Change the return URL for your relying party in ACS

# Prerequisites:
# Windows Azure PowerShell (http://go.microsoft.com/?linkid=9811175)

# You must provide: 
# - The new return URL for your relying party
# - Your ACS namespace name
# - Your ACS management key
# - The ACS name for your relying party


# To work with ACS
Import-Module WAPPSCmdlets

$AppAddress = '[the new return URL for your application]'
$ACSNamespace = '[your ACS namespace name]'
$ACSManagementkey = '[your ACS Management Key]'
$RelyingPartyName = '[the name of your relying party in ACS]'

#  Change the return URL for your relying party
$relyingParty = Get-RelyingParty -ManagementKey $ACSManagementkey -Name $RelyingPartyName -Namespace $ACSNamespace
$relyingParty.ReturnUrl = $AppAddress
Remove-RelyingParty -Name $RelyingPartyName -ManagementKey $ACSManagementkey -Namespace $ACSNamespace
Add-RelyingParty -RelyingParty $relyingParty -ManagementKey $ACSManagementkey -Namespace $ACSNamespace