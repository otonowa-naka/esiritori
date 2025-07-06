#!/bin/bash

# LocalStack用 DynamoDB テーブル作成スクリプト
# EsiritoriGame テーブルとGSIを作成します

set -e

LOCALSTACK_ENDPOINT="http://localhost:4566"
TABLE_NAME="EsiritoriGame"
REGION="ap-northeast-1"

echo "Creating DynamoDB table: $TABLE_NAME"

# メインテーブルの作成
aws dynamodb create-table \
  --endpoint-url $LOCALSTACK_ENDPOINT \
  --region $REGION \
  --table-name $TABLE_NAME \
  --attribute-definitions \
    AttributeName=PK,AttributeType=S \
    AttributeName=SK,AttributeType=S \
    AttributeName=GSI1PK,AttributeType=S \
    AttributeName=GSI1SK,AttributeType=S \
    AttributeName=GSI2PK,AttributeType=S \
    AttributeName=GSI2SK,AttributeType=S \
  --key-schema \
    AttributeName=PK,KeyType=HASH \
    AttributeName=SK,KeyType=RANGE \
  --global-secondary-indexes \
    IndexName=GSI1-ActiveGameIndex,KeySchema=['{AttributeName=GSI1PK,KeyType=HASH},{AttributeName=GSI1SK,KeyType=RANGE}'],Projection='{ProjectionType=KEYS_ONLY}',BillingMode=PAY_PER_REQUEST \
    IndexName=GSI2-PlayerIndex,KeySchema=['{AttributeName=GSI2PK,KeyType=HASH},{AttributeName=GSI2SK,KeyType=RANGE}'],Projection='{ProjectionType=ALL}',BillingMode=PAY_PER_REQUEST \
  --billing-mode PAY_PER_REQUEST \
  --stream-specification StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES

echo "Waiting for table to become active..."

# テーブルがアクティブになるまで待機
aws dynamodb wait table-exists \
  --endpoint-url $LOCALSTACK_ENDPOINT \
  --region $REGION \
  --table-name $TABLE_NAME

echo "DynamoDB table $TABLE_NAME created successfully!"

# テーブル情報の確認
aws dynamodb describe-table \
  --endpoint-url $LOCALSTACK_ENDPOINT \
  --region $REGION \
  --table-name $TABLE_NAME \
  --query 'Table.[TableName,TableStatus,ItemCount]' \
  --output table

echo "Setup complete!"