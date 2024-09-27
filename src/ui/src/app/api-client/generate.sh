#!/bin/bash

SWAGGER_URL="https://localhost:7076/swagger/v1/swagger.json"
SWAGGER_FILE="swagger.json"
OUTPUT_FILE="api-client.ts"

curl -k -o $SWAGGER_FILE $SWAGGER_URL
nswag openapi2tsclient /input:$SWAGGER_FILE /output:$OUTPUT_FILE

echo "Angular-Client wurde erfolgreich erstellt"