# Esiritori（お絵描き当てゲーム）

リアルタイム多人数参加型のお絵描き当てゲームです。一人が与えられたお題を描き、他の参加者がそれを当てるゲームです。

## 📋 プロジェクト構成

このプロジェクトは以下の3つのコンポーネントで構成されています：

- **フロントエンド**: Next.js + TypeScript + Tailwind CSS
- **APIモック**: Node.js + Express（開発用）
- **C# API**: .NET 8.0 + Clean Architecture（本格実装）
- **インフラ**: Docker Compose + LocalStack（AWS サービスモック）

## 🛠️ 環境構築

### 前提条件

以下のソフトウェアがインストールされている必要があります：

- **Docker & Docker Compose**: コンテナ環境用
- **Node.js 20+**: ローカル開発用
- **pnpm**: フロントエンド依存関係管理用
- **.NET 8.0 SDK**: C# API開発・テスト用（オプション）

### Docker環境での構築（推奨）

1. リポジトリをクローン:
```bash
git clone <repository-url>
cd esiritori
```

2. 環境変数ファイルを確認:
```bash
cp .env.local .env.local
# 必要に応じて設定を調整
```

3. Docker Composeでサービスを起動:
```bash
docker compose up -d
```

### ローカル開発環境での構築

#### フロントエンド

1. 依存関係をインストール:
```bash
pnpm install
```

2. 開発サーバーを起動:
```bash
pnpm dev
```

#### APIモック

1. APIディレクトリに移動:
```bash
cd api
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
cd api-csharp
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

### Docker環境での実行

1. 全サービスを起動:
```bash
docker compose up
```

2. アクセス先:
- **フロントエンド**: http://localhost:3000
- **APIモック**: http://localhost:3001
- **LocalStack**: http://localhost:4566

3. サービス停止:
```bash
docker compose down
```

### ローカル環境での実行

各コンポーネントを個別に起動:

1. **LocalStack** (AWS サービスモック):
```bash
docker run --rm -p 4566:4566 localstack/localstack
```

2. **フロントエンド**:
```bash
pnpm dev
```

3. **APIモック**:
```bash
cd api && npm run dev
```

4. **C# API** (オプション):
```bash
cd api-csharp && dotnet run --project EsiritoriApi.Api
```

## 🧪 テスト手順

### フロントエンドのテスト

現在、フロントエンド専用のテストフレームワークは設定されていません。

**型チェック**:
```bash
npx tsc --noEmit
```

**リント**:
```bash
pnpm lint
# 注意: 依存関係をインストール後に実行してください
```

### C# APIのテスト

包括的なテストスイートが用意されています：

1. **全テスト実行**:
```bash
cd api-csharp
dotnet test
```

2. **カバレッジ付きテスト実行**:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

3. **特定のテストクラス実行**:
```bash
dotnet test --filter "ClassName=GameTests"
```

#### テスト構成

- **単体テスト**: ドメインエンティティ、値オブジェクト
- **統合テスト**: API エンドポイント
- **ユースケーステスト**: アプリケーション層
- **リポジトリテスト**: データアクセス層

### APIモックのテスト

APIモックには専用のテストは設定されていませんが、手動でエンドポイントをテストできます：

```bash
curl http://localhost:3001/api/games
```

## 📁 プロジェクト構造

```
esiritori/
├── src/                    # Next.js フロントエンドソース
├── api/                    # Node.js APIモック
├── api-csharp/            # .NET 8.0 C# API
│   ├── EsiritoriApi.Api/          # Web API層
│   ├── EsiritoriApi.Application/  # アプリケーション層
│   ├── EsiritoriApi.Domain/       # ドメイン層
│   ├── EsiritoriApi.Infrastructure/ # インフラ層
│   └── EsiritoriApi.Tests/        # テストプロジェクト
├── design/                # 設計ドキュメント
├── docs/                  # 開発ガイドライン
├── docker-compose.yml     # Docker Compose設定
├── Dockerfile.dev         # フロントエンド開発用Dockerfile
└── .env.local            # 環境変数設定
```

## 🔧 開発ガイド

### 技術スタック

- **フロントエンド**: Next.js 15, React 19, TypeScript, Tailwind CSS
- **APIモック**: Node.js, Express, WebSocket
- **C# API**: .NET 8.0, Clean Architecture, DDD
- **テスト**: xUnit, Moq (C#)
- **インフラ**: Docker, LocalStack, DynamoDB

### 開発フロー

1. 機能開発は主にC# APIで実装
2. フロントエンドは設計に基づいて実装
3. APIモックは開発初期のプロトタイピング用
4. LocalStackでAWSサービスをローカルでモック

## 📚 関連ドキュメント

- [設計ドキュメント](./design/) - システム設計とアーキテクチャ
- [開発ガイドライン](./docs/Coderule.md) - コーディング規約
- [API仕様](./design/openapi.yaml) - REST API仕様

## 🐛 トラブルシューティング

### よくある問題

**Docker Composeが起動しない**:
- Dockerデーモンが起動しているか確認
- ポート3000, 3001, 4566が使用されていないか確認

**pnpmコマンドが見つからない**:
```bash
npm install -g pnpm
```

**.NET SDKが見つからない**:
- .NET 8.0 SDKがインストールされているか確認
- `dotnet --version`で確認

**LocalStackに接続できない**:
- LocalStackコンテナが起動しているか確認
- 環境変数`LOCALSTACK_ENDPOINT`が正しく設定されているか確認
