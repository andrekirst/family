$swagger_url="https://localhost:7076/swagger/v1/swagger.json"
$swagger_file="swagger.json"
$output_file="api-client.ts"

Invoke-WebRequest -Uri $swagger_url -OutFile $swagger_file
nswag openapi2tsclient /input:$swagger_file /output:$output_file
Write-Output "Angular-Client wurde erfolgreich erstellt"