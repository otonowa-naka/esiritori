# お絵描き当てゲーム データベース設計書

## 📋 概要

本ドキュメントは、お絵描き当てゲームのDynamoDBデータベース設計の詳細仕様です。
既存の`dynamodb-schema.md`を基に、実装に必要な詳細情報を補完しています。

---

## 🎯 設計方針

### 基本原則
- **1集約 = 1アイテム（または1パーティション）** を原則とする
- **Single Table Design** を採用し、GSIを最小限に抑制
- **イベント駆動アーキテクチャ** に対応したスキーマ設計
- **リアルタイム性** を重視したアクセスパターン最適化
- **AWS無償プラン** 内での運用を考慮

### アクセスパターン分析
1. ゲーム情報の取得・更新（高頻度）
2. プレイヤー参加・離脱（中頻度）
3. リアルタイム描画データ更新（高頻度）
4. チャットメッセージ送受信（高頻度）
5. スコア履歴の記録・参照（中頻度）
6. ゲーム履歴の参照（低頻度）

---

## 🗄️ テーブル設計

### メインテーブル: `EsiritoriGame`

#### 基本設定
```typescript
TableName: "EsiritoriGame"
BillingMode: "PAY_PER_REQUEST"  // 無償プラン対応
PointInTimeRecovery: true
StreamSpecification: {
  StreamEnabled: true,
  StreamViewType: "NEW_AND_OLD_IMAGES"
}
```

#### パーティション設計

##### 1. ゲームメタデータ
```typescript
PK: "GAME#<gameId>"
SK: "META"
{
  // 基本情報
  gameId: string,                    // ゲームID（UUID）
  status: "waiting" | "playing" | "finished",
  createdAt: number,                 // Unix timestamp
  updatedAt: number,                 // Unix timestamp
  
  // ゲーム設定
  settings: {
    timeLimit: number,               // 制限時間（秒）1-300
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
  }
}
```

##### 2. ラウンド履歴
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

##### 3. チャットメッセージ
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

##### 4. プレイヤー接続情報
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
  
  // TTL設定（24時間後に自動削除）
  ttl: number
}
```

---

## 🔍 Global Secondary Index (GSI)

### GSI1: プレイヤー検索用
```typescript
IndexName: "GSI1-PlayerIndex"
PartitionKey: "GSI1PK"             // "PLAYER#<playerId>"
SortKey: "GSI1SK"                  // "GAME#<gameId>"
ProjectionType: "ALL"

// 使用例：特定プレイヤーが参加中のゲーム一覧取得
```

### GSI2: アクティブゲーム検索用
```typescript
IndexName: "GSI2-ActiveGameIndex"
PartitionKey: "GSI2PK"             // "STATUS#<status>"
SortKey: "GSI2SK"                  // "CREATED#<createdAt>"
ProjectionType: "KEYS_ONLY"

// 使用例：待機中のゲーム一覧取得（ゲーム参加画面用）
```

---

## 📊 データアクセスパターン

### 1. ゲーム作成
```typescript
// 1. ゲームメタデータ作成
PutItem: {
  PK: "GAME#<gameId>",
  SK: "META",
  // ... ゲームデータ
}

// 2. GSI用データ設定
UpdateItem: {
  PK: "GAME#<gameId>",
  SK: "META",
  GSI2PK: "STATUS#waiting",
  GSI2SK: "CREATED#<timestamp>"
}
```

### 2. プレイヤー参加
```typescript
// 1. ゲームにプレイヤー追加
UpdateItem: {
  PK: "GAME#<gameId>",
  SK: "META",
  // players配列にプレイヤー追加
}

// 2. プレイヤー接続情報記録
PutItem: {
  PK: "PLAYER#<playerId>",
  SK: "CONNECTION",
  // ... 接続情報
}
```

### 3. リアルタイム描画更新
```typescript
// 描画データ更新（頻繁な更新）
UpdateItem: {
  PK: "GAME#<gameId>",
  SK: "META",
  UpdateExpression: "SET currentDrawing = :drawing, updatedAt = :timestamp",
  ExpressionAttributeValues: {
    ":drawing": {
      drawerId: "<playerId>",
      data: "<base64DrawingData>",
      updatedAt: timestamp
    },
    ":timestamp": timestamp
  }
}
```

### 4. チャット送信
```typescript
// チャットメッセージ追加
PutItem: {
  PK: "GAME#<gameId>",
  SK: "CHAT#<timestamp>#<messageId>",
  // ... メッセージデータ
}
```

### 5. チャット履歴取得
```typescript
// 最新50件のチャット取得
Query: {
  KeyConditionExpression: "PK = :gameId AND begins_with(SK, :chatPrefix)",
  ExpressionAttributeValues: {
    ":gameId": "GAME#<gameId>",
    ":chatPrefix": "CHAT#"
  },
  ScanIndexForward: false,  // 降順（最新から）
  Limit: 50
}
```

---

## 🔒 セキュリティ・制約

### バリデーション制約
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
```

### TTL設定
```typescript
// プレイヤー接続情報：24時間後に自動削除
ttl: Math.floor(Date.now() / 1000) + (24 * 60 * 60)

// 完了したゲーム：7日後に自動削除
ttl: Math.floor(Date.now() / 1000) + (7 * 24 * 60 * 60)
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

## 🚀 運用・監視

### CloudWatch メトリクス
- **ConsumedReadCapacityUnits**
- **ConsumedWriteCapacityUnits**
- **ThrottledRequests**
- **UserErrors**

### アラート設定
```typescript
// 読み取り容量アラート
ReadCapacityAlert: {
  threshold: 80,  // 80%使用時
  period: 300,    // 5分間
  evaluationPeriods: 2
}

// エラー率アラート
ErrorRateAlert: {
  threshold: 5,   // 5%エラー率
  period: 300,
  evaluationPeriods: 1
}
```

### バックアップ戦略
- **Point-in-time Recovery** 有効化
- **On-demand Backup** 重要なマイルストーン時
- **Cross-region Replication** は無償プランでは無効

---

## 🧪 テストデータ設計

### 単体テスト用データ
```typescript
// 最小構成ゲーム
minimalGame: {
  players: 2,
  rounds: 1,
  timeLimit: 60
}

// 最大構成ゲーム
maximalGame: {
  players: 10,
  rounds: 10,
  timeLimit: 300
}
```

### 統合テスト用データ
```typescript
// 進行中ゲーム
ongoingGame: {
  status: "playing",
  currentRound: 2,
  currentTurn: 3,
  chatMessages: 50,
  scoreHistories: 20
}
```

---

## 📋 マイグレーション計画

### Phase 1: 基本機能
- ゲーム作成・参加
- 基本的なターン進行
- チャット機能

### Phase 2: 拡張機能
- 詳細なスコア履歴
- ゲーム履歴保存
- パフォーマンス最適化

### Phase 3: 運用改善
- 監視・アラート強化
- 自動スケーリング
- データ分析機能

---

## 🔧 実装時の注意点

1. **DynamoDB Streams** を活用したイベント駆動処理
2. **WebSocket API** との連携でリアルタイム更新
3. **Lambda関数** での業務ロジック実装
4. **CloudWatch Logs** での詳細ログ記録
5. **X-Ray** でのパフォーマンス分析

---

## 📚 参考資料

- [DynamoDB Best Practices](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/best-practices.html)
- [Single Table Design](https://www.alexdebrie.com/posts/dynamodb-single-table/)
- [DynamoDB Streams](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Streams.html)
