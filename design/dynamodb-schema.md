# DynamoDB スキーマ構成（お絵描き当てゲーム）

本ファイルは、最新のドメインモデル設計に基づいたDynamoDBスキーマ定義です。

---

## 🎯 設計方針

### 基本原則
- **Single Table Design** を採用し、1つのテーブルで全データを管理
- **1集約 = 1アイテム（または1パーティション）** を原則とする
- **GSIは必要最小限** に抑制してコストを削減
- **イベント駆動アーキテクチャ** に対応したスキーマ設計
- **AWS無償プラン** 内での運用を考慮

---

## 🗄️ メインテーブル: `EsiritoriGame`

### テーブル設定
```typescript
TableName: "EsiritoriGame"
BillingMode: "PAY_PER_REQUEST"  // 無償プラン対応
PointInTimeRecovery: true
StreamSpecification: {
  StreamEnabled: true,
  StreamViewType: "NEW_AND_OLD_IMAGES"
}
```

---

## 📊 データ構造

### 1. ゲームメタデータ
```typescript
PK: "GAME#<gameId>"
SK: "META"
{
  // 基本情報
  gameId: string,                    // ゲームID（UUID）
  status: "waiting" | "playing" | "finished",
  createdAt: number,                 // Unix timestamp
  updatedAt: number,                 // Unix timestamp
  
  // ゲーム設定（制約付き）
  settings: {
    timeLimit: number,               // 制限時間（秒）30-300
    roundCount: number,              // ラウンド数 1-10
    playerCount: number              // 最大プレイヤー数 2-10
  },
  
  // 現在のラウンド・ターン情報
  currentRound: {
    id: string,                      // ラウンドID（UUID）
    roundNumber: number,             // 1-10
    startedAt: number,
    endedAt?: number,
    currentTurn: {
      id: string,                    // ターンID（UUID）
      turnNumber: number,            // 1-プレイヤー数
      drawerId: string,              // 出題者のプレイヤーID
      answer?: string,               // お題（ひらがな、1-50文字）
      status: "not_started" | "setting_answer" | "drawing" | "finished",
      timeLimit: number,             // このターンの制限時間
      startedAt?: number,
      endedAt?: number,
      correctPlayerIds: string[]     // 正解者のプレイヤーIDリスト
    }
  },
  
  // プレイヤー情報
  players: {
    id: string,                      // プレイヤーID（UUID）
    name: string,                    // プレイヤー名（1-10文字）
    status: "ready" | "not_ready",
    joinedAt: number,
    connectionId?: string            // WebSocket接続ID
  }[],
  
  // スコア履歴
  scoreHistories: {
    playerId: string,
    roundNumber: number,
    turnNumber: number,
    points: number,                  // 獲得ポイント（正の整数）
    reason: "correct_answer" | "drawer_penalty",
    timestamp: number
  }[],
  
  // 描画データ（最新のみ保持）
  currentDrawing?: {
    drawerId: string,
    data: string,                    // base64エンコードされた描画データ
    updatedAt: number
  },
  
  // GSI用フィールド
  GSI1PK?: string,                   // "STATUS#<status>"
  GSI1SK?: string,                   // "CREATED#<createdAt>"
  
  // TTL設定（完了したゲームは7日後に削除）
  ttl?: number
}
```

### 2. ラウンド履歴
```typescript
PK: "GAME#<gameId>"
SK: "ROUND#<roundNumber>"
{
  id: string,                        // ラウンドID
  gameId: string,
  roundNumber: number,
  startedAt: number,
  endedAt: number,
  
  // このラウンドで実行されたターン履歴
  turns: {
    id: string,                      // ターンID
    turnNumber: number,
    drawerId: string,
    drawerName: string,
    answer: string,
    status: "finished",
    timeLimit: number,
    startedAt: number,
    endedAt: number,
    correctPlayerIds: string[],
    finalDrawing?: string            // 最終的な描画データ
  }[],
  
  // このラウンドのスコア
  roundScores: {
    playerId: string,
    playerName: string,
    points: number
  }[]
}
```

### 3. チャットメッセージ
```typescript
PK: "GAME#<gameId>"
SK: "CHAT#<timestamp>#<messageId>"
{
  id: string,                        // メッセージID（UUID）
  gameId: string,
  playerId: string,
  playerName: string,
  content: string,                   // メッセージ内容（1-500文字）
  type: "normal" | "system" | "guess" | "correct",
  createdAt: number,
  
  // 推測メッセージの場合
  isGuess?: boolean,
  isCorrect?: boolean,
  roundNumber?: number,
  turnNumber?: number
}
```

### 4. プレイヤー接続情報
```typescript
PK: "PLAYER#<playerId>"
SK: "CONNECTION"
{
  playerId: string,
  playerName: string,
  gameId: string,
  connectionId: string,              // WebSocket接続ID
  connectedAt: number,
  lastActiveAt: number,
  
  // GSI用フィールド
  GSI2PK: string,                    // "PLAYER#<playerId>"
  GSI2SK: string,                    // "GAME#<gameId>"
  
  // TTL設定（24時間後に自動削除）
  ttl: number
}
```

---

## 🔍 Global Secondary Index (GSI)

### GSI1: アクティブゲーム検索用
```typescript
IndexName: "GSI1-ActiveGameIndex"
PartitionKey: "GSI1PK"             // "STATUS#<status>"
SortKey: "GSI1SK"                  // "CREATED#<createdAt>"
ProjectionType: "KEYS_ONLY"

// 使用例：待機中のゲーム一覧取得（ゲーム参加画面用）
```

### GSI2: プレイヤー検索用
```typescript
IndexName: "GSI2-PlayerIndex"
PartitionKey: "GSI2PK"             // "PLAYER#<playerId>"
SortKey: "GSI2SK"                  // "GAME#<gameId>"
ProjectionType: "ALL"

// 使用例：特定プレイヤーが参加中のゲーム一覧取得
```

---

## 📋 データ制約・バリデーション

### 制約条件
```typescript
// プレイヤー名
playerName: {
  type: "string",
  minLength: 1,
  maxLength: 10,
  pattern: "^[\\p{L}\\p{N}\\p{M}\\s]+$"  // Unicode文字、数字、空白のみ
}

// お題
answer: {
  type: "string",
  minLength: 1,
  maxLength: 50,
  pattern: "^[ひらがな]+$"  // ひらがなのみ
}

// チャットメッセージ
chatContent: {
  type: "string",
  minLength: 1,
  maxLength: 500
}

// 制限時間
timeLimit: {
  type: "number",
  minimum: 30,
  maximum: 300
}

// ラウンド数
roundCount: {
  type: "number",
  minimum: 1,
  maximum: 10
}

// プレイヤー数
playerCount: {
  type: "number",
  minimum: 2,
  maximum: 10
}
```

### TTL設定
```typescript
// プレイヤー接続情報：24時間後に自動削除
ttl: Math.floor(Date.now() / 1000) + (24 * 60 * 60)

// 完了したゲーム：7日後に自動削除
ttl: Math.floor(Date.now() / 1000) + (7 * 24 * 60 * 60)
```

---

## 🚀 主要アクセスパターン

### 1. ゲーム作成
```typescript
PutItem: {
  PK: "GAME#<gameId>",
  SK: "META",
  GSI1PK: "STATUS#waiting",
  GSI1SK: "CREATED#<timestamp>",
  // ... ゲームデータ
}
```

### 2. ゲーム参加
```typescript
// 1. ゲームにプレイヤー追加
UpdateItem: {
  PK: "GAME#<gameId>",
  SK: "META",
  UpdateExpression: "SET players = list_append(players, :player)",
  // ...
}

// 2. プレイヤー接続情報記録
PutItem: {
  PK: "PLAYER#<playerId>",
  SK: "CONNECTION",
  GSI2PK: "PLAYER#<playerId>",
  GSI2SK: "GAME#<gameId>",
  // ...
}
```

### 3. リアルタイム描画更新
```typescript
UpdateItem: {
  PK: "GAME#<gameId>",
  SK: "META",
  UpdateExpression: "SET currentDrawing = :drawing, updatedAt = :timestamp",
  // ...
}
```

### 4. チャット送信
```typescript
PutItem: {
  PK: "GAME#<gameId>",
  SK: "CHAT#<timestamp>#<messageId>",
  // ...
}
```

### 5. 待機中ゲーム一覧取得
```typescript
Query: {
  IndexName: "GSI1-ActiveGameIndex",
  KeyConditionExpression: "GSI1PK = :status",
  ExpressionAttributeValues: {
    ":status": "STATUS#waiting"
  }
}
```

---

## 📈 パフォーマンス最適化

### 読み取り最適化
- **Eventually Consistent Read** を基本とする（コスト削減）
- **Strongly Consistent Read** はゲーム状態更新時のみ使用
- **Batch操作** でチャット履歴取得を効率化

### 書き込み最適化
- **Conditional Write** で競合状態を防止
- **UpdateItem** で部分更新を活用
- **TransactWrite** で複数アイテムの整合性保証

### 容量最適化
```typescript
// 描画データ圧縮
drawingData: {
  compression: "gzip",
  maxSize: "100KB",  // base64エンコード後
  format: "svg" | "canvas-commands"
}

// チャット履歴制限
chatHistory: {
  maxMessages: 1000,  // ゲームあたり
  autoCleanup: true
}
```

---

## 🔒 セキュリティ・運用

### 監視項目
- **ConsumedReadCapacityUnits**
- **ConsumedWriteCapacityUnits**
- **ThrottledRequests**
- **UserErrors**

### バックアップ設定
- **Point-in-time Recovery** 有効化
- **DynamoDB Streams** でイベント駆動処理
- **CloudWatch Logs** での詳細ログ記録

---

## 📝 実装時の注意点

1. **DynamoDB Streams** を活用したイベント駆動処理
2. **WebSocket API** との連携でリアルタイム更新
3. **Lambda関数** での業務ロジック実装
4. **条件付き書き込み** で競合状態を防止
5. **適切なエラーハンドリング** とリトライ機構   