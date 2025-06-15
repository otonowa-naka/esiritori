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
  gameId: GameId
  status: 'waiting' | 'playing' | 'finished'
  settings: {
    timeLimit: TimeLimit      // 制限時間（秒）
    roundCount: RoundNumber   // ラウンド数
    playerCount: number       // 最大プレイヤー数
  }
  currentRound: {
    id: RoundId
    roundNumber: RoundNumber
    currentTurn: {
      id: TurnId
      turnNumber: TurnNumber
      drawerId: PlayerId
      answer: Answer
      status: 'not_started' | 'setting_answer' | 'drawing' | 'finished'
      timeLimit: TimeLimit
      startedAt: number
      endedAt: number
    }
    startedAt: number
    endedAt: number
  }
  players: Player[]
  scoreHistories: ScoreHistory[]
  startedAt: number
  updatedAt: number
}

// Player型
{
  id: PlayerId
  name: PlayerName
  status: 'ready' | 'not_ready'
}

// ScoreHistory型
{
  playerId: PlayerId
  roundNumber: RoundNumber
  turnNumber: TurnNumber
  points: Points
  reason: string
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
  id: RoundId
  gameId: GameId
  roundId: RoundId
  roundNumber: RoundNumber
  currentTurn: {
    id: TurnId
    turnNumber: TurnNumber
    drawerId: PlayerId
    answer: Answer
    status: 'not_started' | 'setting_answer' | 'drawing' | 'finished'
    timeLimit: TimeLimit
    startedAt: number
    endedAt: number
  }
  startedAt: number
  endedAt: number
}
```

---

## TurnHistory テーブル
- ターンごとの履歴を保存

```typescript
PK: "ROUND#<roundId>"
SK: "TURN#<turnNumber>"
{
  id: TurnId
  roundId: RoundId
  turnId: TurnId
  turnNumber: TurnNumber
  drawerId: PlayerId
  answer: Answer
  status: 'not_started' | 'setting_answer' | 'drawing' | 'finished'
  timeLimit: TimeLimit
  correctPlayerIds: PlayerId[]
  startedAt: number
  endedAt: number
}
```

---

## ChatMessage テーブル
- 1発言 = 1アイテム

```typescript
PK: "GAME#<gameId>"
SK: "CHAT#<timestamp>"
{
  id: ChatMessageId
  gameId: GameId
  playerId: PlayerId
  message: Message
  type: 'system' | 'player' | 'guess' | 'correct'
  createdAt: number
}
```

---

## 備考
- プレイヤーはGameアイテム内の配列で管理
- スコアはゲーム全体の履歴として保持
- GSI例: プレイヤーIDでの検索用GSI（必要に応じて） 