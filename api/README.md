# API Functions

このディレクトリには、ローカル開発用のAPI関数が含まれます。

## 構造

- `games/` - ゲーム関連のAPI関数
- `chat/` - チャット関連のAPI関数
- `websocket/` - WebSocket関連の処理

## 開発時の注意

- 本番環境では AWS Lambda として動作します
- ローカル開発時は Next.js API Routes として動作します
- 共通のビジネスロジックは `src/lib/` に配置してください
