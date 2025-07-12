#!/bin/bash

# Lambda用のビルドスクリプト
set -e

echo "Building Lambda package..."

# バックエンドディレクトリに移動
cd backend

# ビルド
dotnet publish EsiritoriApi.Api/EsiritoriApi.Api.csproj \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained false \
    --output lambda-package

# Lambda配布用のZIPファイルを作成
cd lambda-package
zip -r ../esiritori-api-lambda.zip .
cd ..

echo "Lambda package created: backend/esiritori-api-lambda.zip"