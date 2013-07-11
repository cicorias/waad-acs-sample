# Enable Microsoft ID (Windows Live ID) as an Identity Provider
# Add Google ID as an identity provider

# Prerequisites:
# Windows Azure PowerShell (http://go.microsoft.com/?linkid=9811175)

# You must provide: 
# - Your ACS namespace name
# - Your ACS management key
# - The ACS name for your relying party


# To work with ACS
Import-Module WAPPSCmdlets

$ACSNamespace = '[your ACS namespace name]'
$ACSManagementkey = '[your ACS Management Key]'
$RelyingPartyName = '[the name of your relying party in ACS]'

$WAADIdentityProvider = 'WAAD'
$RuleGroupName = 'WAADRuleGroup'

# Add the Google ID identity provider
Add-IdentityProvider -PreconfiguredIpType Google -Type Preconfigured -AllowedRelyingParties @($RelyingPartyName) -LoginLinkText "Login with your Google account" -ManagementKey $ACSManagementkey -Namespace $ACSNamespace

#  Make sure all the identity providers are enabled for the relying party
$waadip = Get-IdentityProvider -ManagementKey $ACSManagementkey -Name $WAADIdentityProvider -Namespace $ACSNamespace
$googleip = Get-IdentityProvider -ManagementKey $ACSManagementkey -Name Google -Namespace $ACSNamespace
$liveip = Get-IdentityProvider -ManagementKey $ACSManagementkey -Name 'Windows Live ID' -Namespace $ACSNamespace

$relyingParty = Get-RelyingParty -ManagementKey $ACSManagementkey -Name $RelyingPartyName -Namespace $ACSNamespace
$relyingParty.IdentityProviders = @($waadip, $googleip, $liveip)
Remove-RelyingParty -Name $RelyingPartyName -ManagementKey $ACSManagementkey -Namespace $ACSNamespace
Add-RelyingParty -RelyingParty $relyingParty -ManagementKey $ACSManagementkey -Namespace $ACSNamespace

# Generate the default rules for the Google and Microsoft IDs
Add-DefaultPassthroughRules -GroupName $RuleGroupName -IdentityProviderName Google  -ManagementKey $ACSManagementkey -Namespace $ACSNamespace
Add-DefaultPassthroughRules -GroupName $RuleGroupName -IdentityProviderName 'Windows Live ID'  -ManagementKey $ACSManagementkey -Namespace $ACSNamespace