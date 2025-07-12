#!/bin/bash

# フロントエンドをデバッグ実行するためのスクリプト
set -e

echo "Starting Frontend debug environment..."

# 環境変数をクリア・設定
unset NODE_OPTIONS
unset VSCODE_INSPECTOR_OPTIONS

export NODE_ENV=development
export NEXT_PUBLIC_API_BASE_URL=http://localhost:5073/api

# フロントエンドディレクトリに移動
cd /home/nakano/git/esiritori/frontend

echo "Environment variables:"
echo "  NODE_ENV=$NODE_ENV"
echo "  NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL"
echo ""

# Next.jsを起動（WSL用 - 0.0.0.0でバインド）
echo "Starting Next.js development server (WSL-compatible)..."
echo "Access from Windows browser: http://localhost:3000"
exec pnpm run dev-wsl