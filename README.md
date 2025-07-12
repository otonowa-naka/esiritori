# Esiritori（お絵描き当てゲーム）

リアルタイム多人数参加型のお絵描き当てゲームです。一人が与えられたお題を描き、他の参加者がそれを当てるゲームです。

## 📋 プロジェクト構成

このプロジェクトは以下の4つのコンポーネントで構成されています：

- **フロントエンド**: Next.js 15 + React 19 + TypeScript + Tailwind CSS
- **APIモック**: Node.js + Express（開発用）
- **C# API**: .NET 8.0 + Clean Architecture + DynamoDB（本格実装）
- **インフラ**: Docker Compose + DynamoDB Local（AWS DynamoDBローカル環境）

## 🛠️ 環境構築

### 前提条件

以下のソフトウェアがインストールされている必要があります：

- **Docker & Docker Compose**: コンテナ環境用
- **Node.js 20+**: ローカル開発用
- **pnpm**: フロントエンド依存関係管理用
- **.NET 8.0 SDK**: C# API開発・テスト用（オプション）

### Makefileを使用した構築（推奨）

1. リポジトリをクローン:
```bash
git clone <repository-url>
cd esiritori
```

2. クイックスタート（初回セットアップ + 開発環境起動）:
```bash
make quick-start
```

3. 利用可能なコマンドを確認:
```bash
make help
```

### Docker環境での構築

1. 依存関係のインストール:
```bash
make setup
```

2. Docker Composeでサービスを起動:
```bash
make dev-detached
# または
make docker-up
```

3. DynamoDBテーブルの作成を確認:
```bash
# テーブル作成の確認
docker logs esiritori-dynamodb-init
```

### ローカル開発環境での構築

#### フロントエンド

1. フロントエンドディレクトリに移動:
```bash
cd frontend
```

2. 依存関係をインストール:
```bash
pnpm install
```

3. 開発サーバーを起動:
```bash
pnpm dev
```

#### APIモック

1. APIディレクトリに移動:
```bash
cd frontend/mock
```

2. 依存関係をインストール:
```bash
npm install
```

3. 開発サーバーを起動:
```bash
npm run dev
```

#### C# API

1. C# APIディレクトリに移動:
```bash
cd backend
```

2. 依存関係を復元:
```bash
dotnet restore
```

3. APIを起動:
```bash
dotnet run --project EsiritoriApi.Api
```

## 🚀 実行手順

### Makefileを使用した実行（推奨）

```bash
# 開発環境全体を起動
make dev

# バックグラウンドで起動
make dev-detached

# 個別サービスの起動
make dev-frontend    # フロントエンドのみ
make dev-api        # C# APIのみ  
make dev-mock       # APIモックのみ
make dev-storybook  # Storybookのみ

# サービス停止
make docker-down

# URLを確認
make urls

# プロジェクト状態を確認
make status
```

### Docker環境での実行

1. 全サービスを起動:
```bash
docker compose up
```

2. アクセス先:
- **フロントエンド**: http://localhost:3000
- **APIモック**: http://localhost:3001
- **DynamoDB Local**: http://localhost:8000
- **DynamoDB Admin UI**: http://localhost:8001

3. サービス停止:
```bash
docker compose down
```

### ローカル環境での実行

各コンポーネントを個別に起動:

1. **DynamoDB Local**:
```bash
docker run --rm -p 8000:8000 amazon/dynamodb-local:latest -jar DynamoDBLocal.jar -sharedDb -inMemory
```

**DynamoDB Admin UI** (オプション):
```bash
docker run --rm -p 8001:8001 -e DYNAMO_ENDPOINT=http://localhost:8000 aaronshaf/dynamodb-admin:latest
```

2. **フロントエンド**:
```bash
cd frontend && pnpm dev
```

3. **APIモック**:
```bash
cd frontend/mock && npm run dev
```

4. **C# API** (オプション):
```bash
cd backend && dotnet run --project EsiritoriApi.Api
```

## 🧪 テスト手順

### Makefileを使用したテスト（推奨）

```bash
# 全チェック（リント + 型チェック + テスト）
make full-test

# 個別実行
make test           # 全テスト実行
make test-coverage  # カバレッジ付きテスト
make lint          # リント実行
make type-check    # TypeScript型チェック

# C# API層別テスト
make test-domain        # ドメイン層
make test-application   # アプリケーション層
make test-infrastructure # インフラ層
make test-api          # API層
make test-integration  # 統合テスト
```

### フロントエンドのテスト

現在、フロントエンド専用のテストフレームワークは設定されていません。

**型チェック**:
```bash
make type-check
# または
cd frontend && npx tsc --noEmit
```

**リント**:
```bash
make lint
# または  
cd frontend && pnpm lint
```

**Storybook**:
```bash
make dev-storybook
# または
cd frontend && pnpm storybook
```

### C# APIのテスト

包括的なテストスイートが用意されています：

1. **全テスト実行**:
```bash
cd backend
dotnet test
```

2. **カバレッジ付きテスト実行**:
```bash
cd backend
dotnet test --collect:"XPlat Code Coverage"
```

3. **特定のテストクラス実行**:
```bash
cd backend
dotnet test --filter "ClassName=GameTests"
```

#### テスト構成

**層別テストプロジェクト:**
- **EsiritoriApi.Domain.Tests**: ドメインエンティティ、値オブジェクトのテスト (82.89%カバレッジ)
- **EsiritoriApi.Application.Tests**: ユースケース、DTOのテスト (88.81%カバレッジ)
- **EsiritoriApi.Infrastructure.Tests**: リポジトリ、データアクセスのテスト (100%カバレッジ)
- **EsiritoriApi.Api.Tests**: コントローラー、HTTPエンドポイントのテスト (53.94%カバレッジ)
- **EsiritoriApi.Integration.Tests**: エンドツーエンドの統合テスト

**層別テスト実行:**
```bash
# 特定の層のテストを実行
cd backend
dotnet test EsiritoriApi.Domain.Tests/
dotnet test EsiritoriApi.Application.Tests/
dotnet test EsiritoriApi.Infrastructure.Tests/
dotnet test EsiritoriApi.Api.Tests/
dotnet test EsiritoriApi.Integration.Tests/
```

### APIモックのテスト

APIモックには専用のテストは設定されていませんが、手動でエンドポイントをテストできます：

```bash
curl http://localhost:3001/api/games
```

## 📁 プロジェクト構造

```
esiritori/
├── Makefile               # 開発用コマンド定義
├── backend/               # .NET 8.0 C# API
│   ├── EsiritoriApi.Api/          # Web API層
│   ├── EsiritoriApi.Application/  # アプリケーション層
│   ├── EsiritoriApi.Domain/       # ドメイン層
│   ├── EsiritoriApi.Infrastructure/ # インフラ層
│   ├── EsiritoriApi.Domain.Tests/        # ドメイン層テスト
│   ├── EsiritoriApi.Application.Tests/   # アプリケーション層テスト
│   ├── EsiritoriApi.Infrastructure.Tests/ # インフラ層テスト
│   ├── EsiritoriApi.Api.Tests/          # API層テスト
│   └── EsiritoriApi.Integration.Tests/  # 統合テスト
├── frontend/              # Next.js フロントエンド
│   ├── src/               # フロントエンドソース
│   │   ├── app/           # Next.js App Router
│   │   ├── components/    # 再利用可能Reactコンポーネント + Storybook
│   │   └── lib/           # ユーティリティ関数とAPIクライアント
│   ├── mock/              # Node.js APIモック
│   ├── .storybook/        # Storybook設定
│   ├── package.json       # フロントエンド依存関係
│   └── Dockerfile.dev     # フロントエンド開発用Dockerfile
├── infrastructure/        # インフラ設定ファイル
│   ├── api/               # API仕様
│   │   └── openapi.yaml   # OpenAPI仕様書
│   └── schemas/           # データベーススキーマ
│       ├── create-tables.sh
│       └── dynamodb-table-definitions.json
├── scripts/               # 環境セットアップスクリプト
│   └── create-dynamodb-table.sh
├── design/                # 設計ドキュメント
├── docs/                  # 開発ガイドライン
│   └── Coderule.md        # コーディングルール
├── docker-compose.yml     # Docker Compose設定
├── test.runsettings       # .NETテスト設定
├── CLAUDE.md              # AIアシスタント用プロジェクトガイド
├── .gitignore             # Git除外設定
└── .env.local             # 環境変数設定
```

## 🔧 開発ガイド

### 技術スタック

- **フロントエンド**: Next.js 15, React 19, TypeScript, Tailwind CSS, Storybook
- **APIモック**: Node.js, Express, WebSocket
- **C# API**: .NET 8.0, Clean Architecture, DDD
- **テスト**: xUnit, Moq (C#)
- **インフラ**: Docker, DynamoDB Local, DynamoDB Admin UI

### 開発フロー

1. 機能開発は主にC# APIで実装
2. フロントエンドは設計に基づいて実装
3. APIモックは開発初期のプロトタイピング用
4. DynamoDB LocalでAWS DynamoDBをローカルでモック
5. StorybookでUIコンポーネントの開発とテスト

### インフラ構成

プロジェクトには以下のインフラ関連ファイルが含まれています：

- `infrastructure/schemas/` - DynamoDBテーブル定義と作成スクリプト
- `scripts/` - 環境セットアップ用スクリプト

#### DynamoDBセットアップ

**手動でテーブル作成** (ローカル開発用):
```bash
# DynamoDB Localが起動していることを確認
chmod +x scripts/create-dynamodb-table.sh
./scripts/create-dynamodb-table.sh

# テーブル作成の確認
aws dynamodb list-tables --endpoint-url http://localhost:8000 --region ap-northeast-1
```

**Docker環境** では`dynamodb-init`サービスが自動でテーブルを作成します。

## 📚 関連ドキュメント

- [設計ドキュメント](./design/) - システム設計とアーキテクチャ
- [開発ガイドライン](./docs/Coderule.md) - コーディング規約とDDD実装ルール
- [API仕様](./infrastructure/api/openapi.yaml) - REST API仕様
- [CLAUDE.md](./CLAUDE.md) - AIアシスタント用プロジェクトガイド

## 🐛 トラブルシューティング

### よくある問題

**Docker Composeが起動しない**:
```bash
# 状態確認
make status

# ポート使用状況確認
netstat -tulpn | grep -E ':(3000|3001|8000|8001)'

# Docker環境クリーンアップ
make docker-clean
```

**pnpmコマンドが見つからない**:
```bash
npm install -g pnpm
```

**.NET SDKが見つからない**:
```bash
# バージョン確認
dotnet --version

# インストール確認
make status
```

**Makeコマンドが見つからない**:
```bash
# Ubuntu/Debian
sudo apt-get install make

# macOS
xcode-select --install
```

**DynamoDBに接続できない**:
- DynamoDB Localコンテナが起動しているか確認 (`docker ps`)
- ポート8000が使用されていないか確認
- 環境変数`DYNAMODB_ENDPOINT`が正しく設定されているか確認

**DynamoDB Admin UIにアクセスできない**:
- DynamoDB Adminコンテナが起動しているか確認
- ポート8001が使用されていないか確認
- http://localhost:8001 でアクセスしてテーブルが表示されるか確認

## 🔧 開発ツール

### サービスURL (Docker環境)

- **フロントエンド**: http://localhost:3000
- **APIモック**: http://localhost:3001  
- **DynamoDB Local**: http://localhost:8000
- **DynamoDB Admin UI**: http://localhost:8001
- **Storybook**: http://localhost:6006 (`pnpm storybook`実行時)

### 環境変数設定

プロジェクトルートと`frontend/`ディレクトリに`.env.local`ファイルがあります。環境に合わせて調整してください。

## 📚 開発ガイドライン

- [コーディングルール](docs/Coderule.md) - Value Objectの実装ルール、DDDパターンなど
