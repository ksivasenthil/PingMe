# Manage ASP NET Providers
# Source - http://www.ilovesharepoint.com/2010/03/manage-aspnet-providers-with-powershell.html

param($appConfigPath = $null)

#App config path have to be set before loading System.Web.dll
[System.AppDomain]::CurrentDomain.SetData("APP_CONFIG_FILE", $appConfigPath)

[void][System.Reflection.Assembly]::LoadWithPartialName("System.Web")

function global:Get-MembershipProivder($providerName = $null, [switch]$all) {
 if($all){
  return [System.Web.Security.Membership]::Providers
 }
 if($providerName -eq $null) {
  return [System.Web.Security.Membership]::Provider
 } else {
  return [System.Web.Security.Membership]::Providers[$providerName]
 }
}

function global:Add-MembershipUser($login=$(throw "-login is required"), $password=$(throw "-password is required"), $mail=$(throw "-mail id is required"), $question, $answer, $approved=$true) {
 $provider = $input | select -First 1
 if($provider -isnot [System.Web.Security.MembershipProvider]) {
  $provider = Get-MembershipProvider
 }
 $status = 0
 $provider.CreateUser($login, $password, $mail, $question, $answer, $approved, $null, [ref]$status)
 return [System.Web.Security.MembershipCreateStatus]$status
}

function global:Get-MembershipUser($identifier, $maxResult=100) {
 $provider = $input | select -First 1
 if($provider -isnot [System.Web.Security.MembershipProvider]) {
  $provider = Get-MembershipProvider
 }
 if($identifier -ne $null) {
  $name = $provider.GetUserNameByEmail($identifier)
  if($name -ne $null) {
   $identifier = $name
  }
 }
 $totalUsers = 0
 $users = $provider.GetAllUsers(0,$maxResult, [ref]$totalUsers)
 $users
 if($totalUsers -gt $maxResult) {
  throw "-maxResult limit exceeded"
 }
}

function global:Reset-MembershipUser-Password($identifier=$(throw "-identifier is required"), $questionAnswer) {
 $provider = $input | select -First 1
 if($provider -isnot [System.Web.Security.MembershipProvider]) {
  $provider = Get-MembershipProvider
 }
 $name = $provider.GetUserNameByEmail($identifier)
 if($name -ne $null) {
  $identifier = $name
 }
 return $provider.ResetPassword($identifier, $questionAnswer)
}

function global:Get-ProfileProvider($providerName=$null) {
 if($all) {
  return [System.Web.Security.ProfileManager]::Providers
 }
 if($providerName -eq $null) {
  return [System.Web.Profile.ProfileManager]::Provider
 }else{
  return [System.Web.Profile.ProfileManager]::Provider[$providerName]
 }
}
function global:Set-ProfileProperties($login=$(throw "-login is required"), $property=$(throw "-property key is required"), $propertyvalue=$(throw "-value for property is required")) {
 $provider = $input | select -First 1
 if($provider -isnot [System.Web.Profile.ProfileManager]) {
  $provider = Get-ProfileProvider
 }
 $status = 0
 $userProfile = [System.Web.Profile.ProfileBase]::Create($login)
 $userProfile.SetPropertyValue($property, $propertyvalue)
 $userProfile.Save()
 return "Value of the saved property " + $userProfile.GetPropertyValue($property)
}
# SIG # Begin signature block
# MIIFuQYJKoZIhvcNAQcCoIIFqjCCBaYCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUY9rQwA2ElmOGd1CjEoj7jXfk
# gPegggNCMIIDPjCCAiqgAwIBAgIQRoL/U48ybrREjFOtpAUQoTAJBgUrDgMCHQUA
# MCwxKjAoBgNVBAMTIVBvd2VyU2hlbGwgTG9jYWwgQ2VydGlmaWNhdGUgUm9vdDAe
# Fw0xNTA0MDUwODE4NTBaFw0zOTEyMzEyMzU5NTlaMBoxGDAWBgNVBAMTD1Bvd2Vy
# U2hlbGwgVXNlcjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAJqhx6JB
# y3HofXWYK8IZHrSwBmw60sB3LrSMEQonHpZgs+u5pTMSLGbldLgMSb81Mse646IW
# ONX3zCXRwxMyCzkO3yDJG96uCEOKDskz/cKPHvRElBwLSwtjIrkpoCyNh8dEp8iQ
# f7w4xOKm4nx6UJHub7wyqw4q4QY5/TWmgwG/UgQ7emmb58QiE9XSGBi08BVftHjx
# Y/bloIiVcWiKdvGNscrJnSh1WkTb24qOb+n/ub0MYyeWQVn4SW2cHD2765eK5ki6
# jZ+fQ4j4pjeHGR3qUK5ki8He+6dfRRy6eY0+24hI7iboYuaHlhdcVVve8v5xE/BP
# YuwDpp9OqzQvMP8CAwEAAaN2MHQwEwYDVR0lBAwwCgYIKwYBBQUHAwMwXQYDVR0B
# BFYwVIAQB83iu2nBPrylTAYgEFMxIqEuMCwxKjAoBgNVBAMTIVBvd2VyU2hlbGwg
# TG9jYWwgQ2VydGlmaWNhdGUgUm9vdIIQRdTHxQXOPo1H+cZZg69KejAJBgUrDgMC
# HQUAA4IBAQB1IlQPLTHVAlnEepHN9cBc6JhxN/ljvLiG7xPjYYPU3aIHrbhNJ9fT
# ojCYdQZX7RLQFNw5uoV9RePvsoj519p6y/5jfWtFcQDzK9gKvnYjXDCdqLCULdTe
# 8bO0meXbMyw3SAL0PDDn+zcpLKtjnIOMShasIn7m9Z3gCmSY4PtS81v4j5XKRWze
# 8VCVhA5afBN/fgNdZpT1lWUoB9FF6tBvpiEKu7oxkrzCXU/P3KLOXXRKJ2cibu3x
# iRlIEbn6inoWSBfroZj+kY8fSz+DlkkeSmn/oOSK3ZTQO3fhKKPID5fv0Rnk81bc
# jCDP0f9QN4k/LGbklvf7mlfuC154pm8bMYIB4TCCAd0CAQEwQDAsMSowKAYDVQQD
# EyFQb3dlclNoZWxsIExvY2FsIENlcnRpZmljYXRlIFJvb3QCEEaC/1OPMm60RIxT
# raQFEKEwCQYFKw4DAhoFAKB4MBgGCisGAQQBgjcCAQwxCjAIoAKAAKECgAAwGQYJ
# KoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQB
# gjcCARUwIwYJKoZIhvcNAQkEMRYEFKNKtLMLMmANnh6wSayvT/R/5Og7MA0GCSqG
# SIb3DQEBAQUABIIBAAhDy6eLZ4BP4/mO15nKdHtsUGFXJF9IHxFP3dUfhS7aRS3u
# sl7t19HkNj+HwjnL20SAFI6vh5FTGZn98ocp+qLSX95tPykWdBDZu5fEsgStCz1V
# l54PbWO3CI7KIxDdCsSR2jsGGPOZtSdoBhi8tSbk7tCQi5ycxf5iadh+HX7qu9Ms
# eUceHgsWsoknZiO3GknIXnfyD68sD7zzKGOqjaXKiOfNN5VUhRJDKBibW7tFBJO3
# AnKw09FkhvHpXWAl1W0FSxVEC82dCOC2/8XcFY0EDitoBrrlW3LaSipZA/YNV+bm
# KG5jJRe0Q3f7shdhaM5uRnnFqgGZaYwZdQ+pUqA=
# SIG # End signature block
