# DynamoDB スキーマ構成（お絵描き当てゲーム）

本ファイルは、最新のドメインモデル設計に基づいたDynamoDBスキーマ定義です。

---

## 集約とテーブル設計方針
- 1集約 = 1アイテム（または1パーティション）を原則とする
- 履歴やメッセージなどはサブアイテムまたは別パーティションで管理
- GSIは必要最小限

---

## Game テーブル
- ゲーム全体の状態、ラウンド・ターン・プレイヤー情報を1アイテムで管理
- 履歴は別アイテム

```typescript
PK: "GAME#<gameId>"
SK: "META"
{
  gameId: string
  status: 'waiting' | 'playing' | 'finished'
  settings: {
    timeLimit: number      // 制限時間（秒）
    roundCount: number     // ラウンド数
  }
  players: Player[]
  currentRound: {
    roundNumber: number
    startedAt: number
    endedAt: number
    currentTurn: {
      turnNumber: number
      status: 'not_started' | 'setting_answer' | 'drawing' | 'finished'
      drawerId: string
      answer: string
      startedAt: number
      endedAt: number
    }
  }
  scores: Score[]  // ゲーム全体のスコア履歴
  startedAt: number
  updatedAt: number
}

// Player型
{
  playerId: string
  name: string
  isReady: boolean
  isDrawer: boolean
}

// Score型
{
  playerId: string
  roundNumber: number
  turnNumber: number
  points: number
  reason: 'correct_answer' | 'drawer_penalty'  // ポイント獲得理由
  timestamp: number
}
```

---

## RoundHistory テーブル
- ラウンドごとの履歴を保存

```typescript
PK: "GAME#<gameId>"
SK: "ROUND#<roundNumber>"
{
  roundNumber: number
  turns: TurnHistory[]
  startedAt: number
  endedAt: number
}

// TurnHistory型
{
  turnNumber: number
  drawerId: string
  answer: string
  drawings: string[] // base64画像データ等
  guesses: Guess[]
  startedAt: number
  endedAt: number
}

// Guess型
{
  playerId: string
  answer: string
  isCorrect: boolean
  timestamp: number
}
```

---

## ChatMessage テーブル
- 1発言 = 1アイテム

```typescript
PK: "GAME#<gameId>"
SK: "CHAT#<timestamp>"
{
  messageId: string
  playerId: string
  name: string
  message: string
  isCorrect: boolean
  timestamp: number
}
```

---

## 備考
- ルーム集約は廃止したため、Roomテーブルは不要
- プレイヤーはGameアイテム内の配列で管理
- スコアはゲーム全体の履歴として保持
- GSI例: プレイヤーIDでの検索用GSI（必要に応じて） 