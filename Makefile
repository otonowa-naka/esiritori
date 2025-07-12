# Esiritori Project Makefile
# お絵描き当てゲーム開発用のMakeファイル

.PHONY: help setup dev clean test lint build docker-up docker-down docker-logs

# デフォルトターゲット
.DEFAULT_GOAL := help

# ヘルプ表示
help: ## ヘルプを表示
	@echo "Esiritori プロジェクト用Makefile"
	@echo ""
	@echo "使用可能なコマンド:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

# =======================================
# セットアップ関連
# =======================================

setup: ## 初回セットアップ（依存関係のインストール）
	@echo "📦 依存関係をインストールしています..."
	cd frontend && pnpm install
	cd frontend/mock && npm install
	cd backend && dotnet restore
	@echo "✅ セットアップ完了"

setup-db: ## DynamoDBテーブルの作成（LocalStack用）
	@echo "🗄️ DynamoDBテーブルを作成しています..."
	chmod +x scripts/create-dynamodb-table.sh
	./scripts/create-dynamodb-table.sh
	@echo "✅ DynamoDBテーブル作成完了"

setup-lambda: ## Lambda関数の準備（Test Tool等）
	@echo "🔧 Lambda Test Toolをインストールしています..."
	dotnet tool install -g amazon.lambda.testtool-8.0 || echo "Lambda Test Tool既にインストール済み"
	@echo "✅ Lambda環境準備完了"

# =======================================
# 開発サーバー起動
# =======================================

dev: ## 開発環境全体を起動（Docker）
	@echo "🚀 開発環境を起動しています..."
	docker compose up

dev-detached: ## 開発環境をバックグラウンドで起動（Docker）
	@echo "🚀 開発環境をバックグラウンドで起動しています..."
	docker compose up -d

dev-frontend: ## フロントエンド開発サーバーのみ起動
	@echo "🌐 フロントエンド開発サーバーを起動しています..."
	cd frontend && pnpm dev

dev-frontend-debug: ## フロントエンド開発サーバーをデバッグモードで起動
	@echo "🌐 フロントエンド開発サーバーをデバッグモードで起動しています..."
	./scripts/debug-frontend.sh

dev-frontend-wsl: ## フロントエンド開発サーバーをWSL用で起動（Windows からアクセス可能）
	@echo "🌐 フロントエンド開発サーバーをWSL用で起動しています..."
	@echo "Windows ブラウザからアクセス: http://localhost:3000"
	cd frontend && pnpm run dev-wsl

dev-api: ## C# API開発サーバーのみ起動（通常のASP.NET Core）
	@echo "⚙️ C# API開発サーバーを起動しています..."
	cd backend && dotnet run --project EsiritoriApi.Api

dev-api-wsl: ## C# API開発サーバーをWSL用で起動（Windows からアクセス可能）
	@echo "⚙️ C# API開発サーバーをWSL用で起動しています..."
	@echo "Windows ブラウザからアクセス: http://localhost:5073/swagger"
	cd backend && dotnet run --project EsiritoriApi.Api --launch-profile http-wsl

dev-lambda: ## Lambda関数をローカルでデバッグ実行
	@echo "🔗 Lambda関数デバッグ環境を起動しています..."
	./scripts/debug-lambda-local.sh

dev-lambda-test-tool: ## Lambda Test Toolで起動
	@echo "🔧 Lambda Test Toolを起動しています..."
	cd backend/EsiritoriApi.Api && dotnet lambda-test-tool-8.0

dev-mock: ## APIモック開発サーバーのみ起動
	@echo "🔧 APIモック開発サーバーを起動しています..."
	cd frontend/mock && npm run dev

dev-storybook: ## Storybookを起動
	@echo "📚 Storybookを起動しています..."
	cd frontend && pnpm storybook

dev-localstack: ## LocalStackのみ起動（DynamoDB + Lambda）
	@echo "☁️ LocalStackを起動しています..."
	docker compose up -d localstack

dev-dynamodb-admin: ## DynamoDB Admin UIを起動（LocalStack必須）
	@echo "🔍 DynamoDB Admin UIを起動しています..."
	docker compose up -d dynamodb-admin

# =======================================
# ビルド関連
# =======================================

build: ## 全プロジェクトをビルド
	@echo "🔨 プロジェクトをビルドしています..."
	cd frontend && pnpm build
	cd backend && dotnet build
	@echo "✅ ビルド完了"

build-frontend: ## フロントエンドのみビルド
	@echo "🔨 フロントエンドをビルドしています..."
	cd frontend && pnpm build

build-backend: ## C# APIのみビルド
	@echo "🔨 C# APIをビルドしています..."
	cd backend && dotnet build

build-storybook: ## Storybookをビルド
	@echo "🔨 Storybookをビルドしています..."
	cd frontend && pnpm build-storybook

build-lambda: ## Lambda関数をビルド
	@echo "🔨 Lambda関数をビルドしています..."
	chmod +x scripts/build-lambda.sh
	./scripts/build-lambda.sh
	@echo "✅ Lambda関数ビルド完了"

# =======================================
# テスト関連
# =======================================

test: ## 全テストを実行
	@echo "🧪 全テストを実行しています..."
	cd backend && dotnet test
	@echo "✅ テスト完了"

test-coverage: ## カバレッジ付きでテストを実行
	@echo "🧪 カバレッジ付きでテストを実行しています..."
	cd backend && dotnet test --collect:"XPlat Code Coverage"
	@echo "✅ テストとカバレッジ完了"

test-domain: ## ドメイン層のテストを実行
	@echo "🧪 ドメイン層のテストを実行しています..."
	cd backend && dotnet test EsiritoriApi.Domain.Tests/

test-application: ## アプリケーション層のテストを実行
	@echo "🧪 アプリケーション層のテストを実行しています..."
	cd backend && dotnet test EsiritoriApi.Application.Tests/

test-infrastructure: ## インフラ層のテストを実行
	@echo "🧪 インフラ層のテストを実行しています..."
	cd backend && dotnet test EsiritoriApi.Infrastructure.Tests/

test-api: ## API層のテストを実行
	@echo "🧪 API層のテストを実行しています..."
	cd backend && dotnet test EsiritoriApi.Api.Tests/

test-integration: ## 統合テストを実行
	@echo "🧪 統合テストを実行しています..."
	cd backend && dotnet test EsiritoriApi.Integration.Tests/

# =======================================
# リント・型チェック関連
# =======================================

lint: ## 全プロジェクトでリントを実行
	@echo "🔍 リントを実行しています..."
	cd frontend && pnpm lint
	@echo "✅ リント完了"

type-check: ## TypeScriptの型チェックを実行
	@echo "🔍 TypeScriptの型チェックを実行しています..."
	cd frontend && pnpm type-check
	@echo "✅ 型チェック完了"

# =======================================
# Docker関連
# =======================================

docker-up: dev-detached ## Docker環境を起動

docker-down: ## Docker環境を停止・削除
	@echo "🛑 Docker環境を停止しています..."
	docker compose down

docker-logs: ## Dockerコンテナのログを表示
	@echo "📋 Dockerコンテナのログを表示しています..."
	docker compose logs -f

docker-ps: ## 実行中のDockerコンテナを表示
	@echo "📋 実行中のDockerコンテナ:"
	docker compose ps

docker-clean: ## Docker環境をクリーンアップ
	@echo "🧹 Docker環境をクリーンアップしています..."
	docker compose down -v --remove-orphans
	docker system prune -f

# =======================================
# データベース関連
# =======================================

db-tables: ## DynamoDBテーブル一覧を表示（LocalStack）
	@echo "📋 DynamoDBテーブル一覧:"
	aws dynamodb list-tables --endpoint-url http://localhost:4566 --region ap-northeast-1

db-describe-games: ## EsiritoriGameテーブルの構造を表示
	@echo "📋 EsiritoriGameテーブルの構造:"
	aws dynamodb describe-table --table-name EsiritoriGame --endpoint-url http://localhost:4566 --region ap-northeast-1

# =======================================
# Lambda関連
# =======================================

lambda-deploy: build-lambda ## Lambda関数をLocalStackにデプロイ
	@echo "🚀 Lambda関数をLocalStackにデプロイしています..."
	chmod +x scripts/deploy-lambda-localstack.sh
	./scripts/deploy-lambda-localstack.sh
	@echo "✅ Lambda関数デプロイ完了"

lambda-list: ## LocalStackのLambda関数一覧を表示
	@echo "📋 Lambda関数一覧:"
	aws lambda list-functions --endpoint-url http://localhost:4566 --region ap-northeast-1

lambda-logs: ## Lambda関数のログを表示
	@echo "📋 Lambda関数のログ:"
	aws logs describe-log-groups --endpoint-url http://localhost:4566 --region ap-northeast-1

# =======================================
# ユーティリティ
# =======================================

clean: ## 生成ファイルをクリーンアップ
	@echo "🧹 生成ファイルをクリーンアップしています..."
	rm -rf frontend/.next
	rm -rf frontend/storybook-static
	rm -rf backend/*/bin
	rm -rf backend/*/obj
	rm -rf backend/lambda-package
	rm -rf backend/esiritori-api-lambda.zip
	rm -rf TestResults/
	@echo "✅ クリーンアップ完了"

status: ## プロジェクトの状態を確認
	@echo "📊 プロジェクトの状態:"
	@echo ""
	@echo "🐳 Docker コンテナ:"
	@docker compose ps 2>/dev/null || echo "Docker Compose未起動"
	@echo ""
	@echo "📦 Node.js バージョン:"
	@node --version 2>/dev/null || echo "Node.js未インストール"
	@echo ""
	@echo "📦 .NET バージョン:"
	@dotnet --version 2>/dev/null || echo ".NET未インストール"
	@echo ""
	@echo "📦 pnpm バージョン:"
	@pnpm --version 2>/dev/null || echo "pnpm未インストール"

urls: ## アクセス可能なURLを表示
	@echo "🌐 アクセス可能なURL:"
	@echo "Frontend:           http://localhost:3000"
	@echo "API Mock:           http://localhost:3001"
	@echo "API (ASP.NET Core): http://localhost:5073/swagger"
	@echo "Lambda Test Tool:   http://localhost:5050"
	@echo "LocalStack:         http://localhost:4566"
	@echo "DynamoDB Admin UI:  http://localhost:8001"
	@echo "Storybook:          http://localhost:6006 (make dev-storybook実行時)"

# =======================================
# 開発ワークフロー
# =======================================

quick-start: setup setup-lambda docker-up ## クイックスタート（初回セットアップ + 開発環境起動）
	@echo "🎉 開発環境が起動しました！"
	@echo ""
	@$(MAKE) urls

lambda-quick-start: setup-lambda dev-localstack setup-db lambda-deploy ## Lambda環境のクイックスタート
	@echo "🎉 Lambda開発環境が起動しました！"
	@echo ""
	@echo "✅ Lambda関数がLocalStackにデプロイされました"
	@echo "🔧 Lambda Test Toolでローカルデバッグも可能です: make dev-lambda-test-tool"

fullstack-debug: ## フルスタックデバッグ環境を起動（バックエンド + フロントエンド）
	@echo "🚀 フルスタックデバッグ環境を起動しています..."
	@echo "バックエンド: ASP.NET Core (http://localhost:5073)"
	@echo "フロントエンド: Next.js (http://localhost:3000)"
	@echo ""
	@echo "🔧 バックエンドを起動します..."
	@echo "別ターミナルでフロントエンドを起動してください:"
	@echo "  make dev-frontend-debug"
	@echo ""
	cd backend && dotnet run --project EsiritoriApi.Api --urls "http://0.0.0.0:5073"

fullstack-debug-wsl: ## フルスタックデバッグ環境をWSL用で起動（Windows からアクセス可能）
	@echo "🚀 WSL用フルスタックデバッグ環境を起動しています..."
	@echo "バックエンド: ASP.NET Core (http://localhost:5073)"
	@echo "フロントエンド: Next.js (http://localhost:3000)"
	@echo ""
	@echo "🔧 バックエンドを起動します..."
	@echo "別ターミナルでフロントエンドを起動してください:"
	@echo "  make dev-frontend-wsl"
	@echo ""
	@echo "Windowsのブラウザからアクセスできるようになりました！"
	cd backend && dotnet run --project EsiritoriApi.Api --launch-profile http-wsl

full-test: lint type-check test ## 全チェック（リント + 型チェック + テスト）
	@echo "✅ 全チェック完了"

restart: docker-down docker-up ## Docker環境を再起動
	@echo "🔄 Docker環境を再起動しました"