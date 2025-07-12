#!/bin/bash

# Lambda関数をローカルでデバッグ実行するためのスクリプト
set -e

echo "Setting up local Lambda debugging environment..."

# Lambda Test Toolをインストール（まだの場合）
if ! dotnet tool list -g | grep -q "amazon.lambda.testtool"; then
    echo "Installing Lambda Test Tool..."
    dotnet tool install -g amazon.lambda.testtool-8.0
fi

# LocalStackが起動しているかチェック
echo "Checking LocalStack..."
if ! curl -s http://localhost:4566/_localstack/health > /dev/null; then
    echo "Warning: LocalStack is not running. Starting with docker compose..."
    docker compose up -d localstack
    
    # LocalStackの準備ができるまで待機
    echo "Waiting for LocalStack to be ready..."
    until curl -s http://localhost:4566/_localstack/health > /dev/null; do
        echo "Waiting for LocalStack..."
        sleep 2
    done
fi

# DynamoDBテーブルが存在するかチェック
echo "Checking DynamoDB tables..."
if ! aws dynamodb describe-table --table-name EsiritoriGame --endpoint-url http://localhost:4566 --region ap-northeast-1 > /dev/null 2>&1; then
    echo "Creating DynamoDB tables..."
    ./scripts/create-dynamodb-table.sh
fi

echo "Local debugging environment is ready!"
echo ""
echo "Debug options:"
echo "1. Standard ASP.NET Core debugging:"
echo "   cd backend && dotnet run --project EsiritoriApi.Api"
echo "   API available at: http://localhost:5073/swagger"
echo ""
echo "2. Lambda Test Tool debugging:"
echo "   cd backend/EsiritoriApi.Api && dotnet lambda-test-tool-8.0"
echo "   Lambda Test Tool available at: http://localhost:5050"
echo ""
echo "3. Visual Studio/VSCode:"
echo "   Open EsiritoriApi.Api.csproj and select 'http' or 'Lambda Test Tool' profile"
echo ""
echo "Environment variables set for LocalStack:"
echo "  - DynamoDB:ServiceURL=http://localhost:4566"
echo "  - AWS:Region=ap-northeast-1"