#!/bin/bash

# LocalStackにLambda関数をデプロイするスクリプト
set -e

FUNCTION_NAME="esiritori-api"
LAMBDA_ZIP_PATH="backend/esiritori-api-lambda.zip"
LOCALSTACK_ENDPOINT="http://localhost:4566"

echo "Deploying Lambda function to LocalStack..."

# Lambdaパッケージが存在するかチェック
if [ ! -f "$LAMBDA_ZIP_PATH" ]; then
    echo "Lambda package not found. Building first..."
    ./scripts/build-lambda.sh
fi

# 既存の関数があれば削除
echo "Checking for existing function..."
aws lambda get-function \
    --function-name $FUNCTION_NAME \
    --endpoint-url $LOCALSTACK_ENDPOINT \
    --region ap-northeast-1 \
    2>/dev/null && {
    echo "Deleting existing function..."
    aws lambda delete-function \
        --function-name $FUNCTION_NAME \
        --endpoint-url $LOCALSTACK_ENDPOINT \
        --region ap-northeast-1
} || echo "No existing function found."

# Lambda関数を作成
echo "Creating Lambda function..."
aws lambda create-function \
    --function-name $FUNCTION_NAME \
    --runtime dotnet6 \
    --role arn:aws:iam::123456789012:role/lambda-role \
    --handler EsiritoriApi.Api::EsiritoriApi.Api.LambdaEntryPoint::FunctionHandlerAsync \
    --zip-file fileb://$LAMBDA_ZIP_PATH \
    --timeout 30 \
    --memory-size 512 \
    --environment 'Variables={DYNAMODB_ENDPOINT=http://localstack:4566,AWS_REGION=ap-northeast-1}' \
    --endpoint-url $LOCALSTACK_ENDPOINT \
    --region ap-northeast-1

# Function URLを作成（HTTP APIとして使用）
echo "Creating Function URL..."
aws lambda create-function-url-config \
    --function-name $FUNCTION_NAME \
    --auth-type NONE \
    --cors '{"AllowCredentials": false, "AllowHeaders": ["*"], "AllowMethods": ["*"], "AllowOrigins": ["*"]}' \
    --endpoint-url $LOCALSTACK_ENDPOINT \
    --region ap-northeast-1

# Function URLを取得
FUNCTION_URL=$(aws lambda get-function-url-config \
    --function-name $FUNCTION_NAME \
    --endpoint-url $LOCALSTACK_ENDPOINT \
    --region ap-northeast-1 \
    --query 'FunctionUrl' \
    --output text)

echo "Lambda function deployed successfully!"
echo "Function URL: $FUNCTION_URL"
echo "You can test the API at: ${FUNCTION_URL}api/games"