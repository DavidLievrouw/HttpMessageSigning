$token = '<Bearer Token from AppVeyor>'
$accountName = 'DavidLievrouw'
$projectSlug = 'httpmessagesigning'
$branch = 'dev_24'
$commitId = 'ea77f827d59bc1a385333003aee76096fb750c3d'

$headers = @{
  "Authorization" = "Bearer $token"
  "Content-type" = "application/json"
}

$body = @{
  accountName=$accountName
  projectSlug=$projectSlug
  branch=$branch
  commitId=$commitId
}
$body = $body | ConvertTo-Json

Invoke-RestMethod -Uri 'https://ci.appveyor.com/api/account/$accountName/builds' -Headers $headers -Body $body -Method POST