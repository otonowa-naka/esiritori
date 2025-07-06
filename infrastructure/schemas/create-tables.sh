#!/bin/bash

# DynamoDB テーブル作成スクリプト
# infrastructure/schemas/dynamodb-table-definitions.json を読み込んでテーブルを作成します

set -e

# 引数の処理
ENDPOINT_URL=${1:-"http://localhost:4566"}
REGION=${2:-"ap-northeast-1"}
SCHEMA_FILE=${3:-"$(dirname "$0")/dynamodb-table-definitions.json"}

echo "Creating DynamoDB tables from schema file: $SCHEMA_FILE"
echo "Endpoint: $ENDPOINT_URL"
echo "Region: $REGION"

# スキーマファイルの存在チェック
if [ ! -f "$SCHEMA_FILE" ]; then
    echo "Error: Schema file not found: $SCHEMA_FILE"
    exit 1
fi

# jq の存在チェック
if ! command -v jq &> /dev/null; then
    echo "Error: jq is required but not installed"
    exit 1
fi

# テーブル数を取得
TABLE_COUNT=$(jq '.tables | length' "$SCHEMA_FILE")
echo "Found $TABLE_COUNT table(s) to create"

# EsiritoriGameテーブルを作成
TABLE_NAME="EsiritoriGame"
echo "Creating table: $TABLE_NAME"

# テーブルが既に存在するかチェック
if aws dynamodb describe-table \
    --endpoint-url "$ENDPOINT_URL" \
    --region "$REGION" \
    --table-name "$TABLE_NAME" \
    --output json &>/dev/null; then
    echo "Table $TABLE_NAME already exists, skipping..."
else
    # テーブルを作成
    aws dynamodb create-table \
        --endpoint-url "$ENDPOINT_URL" \
        --region "$REGION" \
        --table-name "$TABLE_NAME" \
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
            'IndexName=GSI1-ActiveGameIndex,KeySchema=[{AttributeName=GSI1PK,KeyType=HASH},{AttributeName=GSI1SK,KeyType=RANGE}],Projection={ProjectionType=KEYS_ONLY},ProvisionedThroughput={ReadCapacityUnits=5,WriteCapacityUnits=5}' \
            'IndexName=GSI2-PlayerIndex,KeySchema=[{AttributeName=GSI2PK,KeyType=HASH},{AttributeName=GSI2SK,KeyType=RANGE}],Projection={ProjectionType=ALL},ProvisionedThroughput={ReadCapacityUnits=5,WriteCapacityUnits=5}' \
        --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 > /dev/null
    
    echo "Table $TABLE_NAME created successfully"
    
    # テーブルがアクティブになるまで待機
    echo "Waiting for table $TABLE_NAME to become active..."
    aws dynamodb wait table-exists \
        --endpoint-url "$ENDPOINT_URL" \
        --region "$REGION" \
        --table-name "$TABLE_NAME"
fi

echo "All tables created successfully!"

# テーブル一覧を表示
echo "Current tables:"
aws dynamodb list-tables \
    --endpoint-url "$ENDPOINT_URL" \
    --region "$REGION" \
    --output table