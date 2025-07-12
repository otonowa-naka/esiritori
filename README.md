# Esiritori - お絵描き当てゲーム

リアルタイム多人数対戦のお絵描き当てゲームです。プレイヤーが順番に絵を描き、他のプレイヤーがその絵が何かを当てるゲームです。

## 🏗️ アーキテクチャ

### システム構成

- **フロントエンド**: Next.js 15 + React 19 + TypeScript + Tailwind CSS
- **バックエンド**: .NET 8.0 + Clean Architecture + DynamoDB
- **インフラ**: LocalStack (AWS services mock) + Docker Compose
- **Lambda**: AWS Lambda Function (LocalStack環境)

### 技術スタック

| 領域 | 技術 |
|------|------|
| フロントエンド | Next.js 15, React 19, TypeScript, Tailwind CSS |
| バックエンド | .NET 8.0, ASP.NET Core, Clean Architecture |
| データベース | DynamoDB (LocalStack) |
| Lambda | AWS Lambda Function, Function URL |
| 開発環境 | Docker Compose, LocalStack |
| テスト | xUnit, Moq, カバレッジ測定 |

## 🚀 クイックスタート

### 必要な環境

- Docker & Docker Compose
- .NET 8.0 SDK
- Node.js (18+) & pnpm
- AWS CLI

### Lambda環境での開発（推奨）

```bash
# Lambda環境のクイックスタート
make lambda-quick-start

# 個別コマンド
make setup-lambda          # Lambda Test Tool (amazon.lambda.testtool-8.0) インストール
make dev-localstack         # LocalStack起動
make setup-db              # DynamoDBテーブル作成
make lambda-deploy         # Lambda関数デプロイ
```

### WSL環境での開発（Windows ユーザー推奨）

```bash
# WSL用フルスタック環境起動

# ターミナル1: バックエンド起動
make fullstack-debug-wsl

# ターミナル2: フロントエンド起動  
make dev-frontend-wsl

# Windows ブラウザからアクセス
# フロントエンド: http://localhost:3000
# バックエンドAPI: http://localhost:5073/swagger
```

### 従来環境での開発

```bash
# クイックスタート（全環境セットアップ + 起動）
make quick-start

# 個別セットアップ
make setup                 # 依存関係インストール
make docker-up            # Docker環境起動
make setup-db             # DynamoDBテーブル作成
```

## 🛠️ 開発コマンド

### Lambda関連

```bash
# Lambda関数をローカルでデバッグ
make dev-lambda

# Lambda Test Toolで起動
make dev-lambda-test-tool

# Lambda関数をビルド
make build-lambda

# LocalStackにデプロイ
make lambda-deploy

# Lambda関数一覧表示
make lambda-list
```

### 開発サーバー

```bash
# 各種開発サーバー起動
make dev-frontend         # Next.js開発サーバー
make dev-frontend-debug   # Next.js開発サーバー（デバッグ用・環境変数修正済み）
make dev-frontend-wsl     # Next.js開発サーバー（WSL用・Windows からアクセス可能）
make dev-api             # ASP.NET Core API
make dev-api-wsl         # ASP.NET Core API（WSL用・Windows からアクセス可能）
make dev-lambda          # Lambda関数デバッグ
make dev-mock            # APIモックサーバー
make dev-localstack      # LocalStackのみ
make fullstack-debug     # フルスタックデバッグ環境
make fullstack-debug-wsl # フルスタックデバッグ環境（WSL用・Windows からアクセス可能）
```

### テスト

```bash
make test                # 全テスト実行
make test-coverage       # カバレッジ付きテスト
make test-domain         # ドメイン層テスト
make test-application    # アプリケーション層テスト
make test-infrastructure # インフラ層テスト
make test-integration    # 統合テスト
```

### ビルド

```bash
make build               # 全プロジェクトビルド
make build-frontend      # フロントエンドビルド
make build-backend       # C# APIビルド
make build-lambda        # Lambda関数ビルド
```

### データベース管理

```bash
make db-tables           # DynamoDBテーブル一覧
make db-describe-games   # EsiritoriGameテーブル詳細
```

### ユーティリティ

```bash
make status              # プロジェクト状態確認
make urls                # アクセス可能URL表示
make clean               # 生成ファイルクリーンアップ
```

## 🌐 アクセス可能なURL

| サービス | URL | 説明 |
|---------|-----|------|
| フロントエンド | http://localhost:3000 | Next.js開発サーバー |
| API Mock | http://localhost:3001 | Node.js APIモック |
| API (ASP.NET Core) | http://localhost:5073/swagger | Swagger UI |
| Lambda Test Tool | http://localhost:5050 | Lambda関数テスト環境 |
| LocalStack | http://localhost:4566 | AWS サービスモック |
| DynamoDB Admin | http://localhost:8001 | DynamoDB管理UI |

## 🏛️ Clean Architecture

```
backend/
├── EsiritoriApi.Domain/          # ドメイン層 - ビジネスエンティティとルール
├── EsiritoriApi.Application/     # アプリケーション層 - ユースケース
├── EsiritoriApi.Infrastructure/  # インフラ層 - 外部システム接続
├── EsiritoriApi.Api/            # プレゼンテーション層 - Web API
└── Tests/                       # 各層ごとのテスト
```

### 特徴

- **ドメイン駆動設計 (DDD)** による設計
- **価値オブジェクト (Value Objects)** でドメインロジックをカプセル化
- **依存性注入** による疎結合
- **高いテストカバレッジ** (80%以上)

## 🧪 テスト戦略

### カバレッジ目標

| 層 | 目標カバレッジ | 現在のカバレッジ |
|----|--------------|-----------------|
| ドメイン層 | 80%+ | 82.89% ✅ |
| アプリケーション層 | 80%+ | 88.81% ✅ |
| インフラ層 | 100% | 100% ✅ |
| API層 | 50%+ | 53.94% ✅ |

### テスト実行

```bash
# 全テスト実行
make test

# カバレッジ付きテスト
make test-coverage

# 層別テスト実行
make test-domain
make test-application
make test-infrastructure
make test-api
make test-integration
```

## 🚀 Lambda関数の開発とデプロイ

### ローカル開発

```bash
# 1. LocalStack環境準備
make dev-localstack
make setup-db

# 2. Lambda関数開発
make dev-lambda-test-tool    # Lambda Test Toolで開発
# または
make dev-api                 # 通常のASP.NET Coreとして開発
```

### LocalStackへのデプロイ

```bash
# Lambda関数をビルド・デプロイ
make lambda-deploy

# Function URLでAPIテスト
curl <FUNCTION_URL>/api/games -H "Content-Type: application/json" \
  -d '{"hostPlayerName": "Player1", "maxPlayers": 4}'
```

### デバッグ方法

1. **標準ASP.NET Core**: `make dev-api` でSwagger UI使用
2. **Lambda Test Tool**: `make dev-lambda-test-tool` でLambda環境シミュレート
3. **IDE デバッグ**: Visual Studio/VSCodeでブレークポイント設定

### VS Code でのデバッグ

#### **WSL環境（推奨）:**
1. **フルスタックデバッグ**: `Full Stack Debug (WSL)` 
2. **フロントエンドのみ**: `Frontend: Next.js (WSL)`
3. **バックエンドのみ**: `Backend: ASP.NET Core (WSL)`

#### **標準環境:**
1. **フルスタックデバッグ**: `Full Stack Debug (Standard)`
2. **フロントエンドのみ**: `Frontend: Next.js (standard)`
3. **バックエンドのみ**: `Backend: ASP.NET Core (standard)`

**操作手順:**
1. F5キーを押す
2. デバッグ設定を選択
3. Windows ブラウザから http://localhost:3000 でアクセス

## 📁 プロジェクト構造

```
esiritori/
├── frontend/                    # Next.js フロントエンド
│   ├── src/app/                # App Router
│   ├── src/components/         # Reactコンポーネント
│   └── mock/                   # APIモックサーバー
├── backend/                    # .NET バックエンド
│   ├── EsiritoriApi.Api/       # Web API層
│   ├── EsiritoriApi.Application/ # アプリケーション層
│   ├── EsiritoriApi.Domain/    # ドメイン層
│   ├── EsiritoriApi.Infrastructure/ # インフラ層
│   └── Tests/                  # テストプロジェクト
├── infrastructure/             # インフラ設定
├── scripts/                    # 自動化スクリプト
├── design/                     # 設計ドキュメント
└── docs/                       # 開発ガイドライン
```

## 🔧 開発ワークフロー

### 新機能開発

1. **設計**: `design/` ディレクトリで仕様確認
2. **ドメイン実装**: ドメイン層からボトムアップで実装
3. **テスト**: 各層のテストを先に作成（TDD）
4. **統合**: API層まで実装
5. **検証**: `make full-test` で全チェック

### Git ワークフロー

```bash
# 開発ブランチ作成
git checkout -b feature/new-feature

# 開発・テスト
make full-test

# コミット・プッシュ
git commit -m "feat: 新機能実装"
git push origin feature/new-feature
```

## 📋 トラブルシューティング

### よくある問題

1. **LocalStackが起動しない**
   ```bash
   docker compose down
   docker compose up -d localstack
   ```

2. **DynamoDBテーブルが見つからない**
   ```bash
   make setup-db
   ```

3. **Lambda関数のデプロイエラー**
   ```bash
   make clean
   make build-lambda
   make lambda-deploy
   ```

4. **依存関係の問題**
   ```bash
   make clean
   make setup
   ```

## 📚 詳細ドキュメント

- [コーディングルール](docs/Coderule.md)
- [システム設計](design/)
- [Claude向け開発ガイド](CLAUDE.md)

## 🤝 貢献

1. Issues で課題を報告
2. Feature ブランチで開発
3. Pull Request で提案
4. レビュー後にマージ

## 📄 ライセンス

MIT License