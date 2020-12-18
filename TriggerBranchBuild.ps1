$token = '<AppVeyor Bearer Token>'
$accountName = 'DavidLievrouw'
$projectSlug = 'httpmessagesigning'
$branch = 'dev_24'
$commitId = 'd82cd2b75930f23505a640102a2134053e91039d'

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

Invoke-RestMethod -Uri "https://ci.appveyor.com/api/account/$accountName/builds" -Headers $headers -Body $body -Method POST
