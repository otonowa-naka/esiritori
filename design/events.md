# お絵描き当てゲーム イベント定義

## 1. ゲーム管理イベント

### GameCreatedEvent
```typescript
interface GameCreatedEvent {
  eventId: string;
  eventType: 'GameCreated';
  timestamp: number;
  gameId: string;
  settings: {
    timeLimit: number;      // 制限時間（秒）
    roundCount: number;     // ラウンド数
    playerCount: number;    // 最大プレイヤー数
  };
  createdBy: {
    playerId: string;
    playerName: string;
  };
}
```

### GameStartedEvent
```typescript
interface GameStartedEvent {
  eventId: string;
  eventType: 'GameStarted';
  timestamp: number;
  gameId: string;
  players: {
    playerId: string;
    playerName: string;
    isReady: boolean;
  }[];
}
```

### GameEndedEvent
```typescript
interface GameEndedEvent {
  eventId: string;
  eventType: 'GameEnded';
  timestamp: number;
  gameId: string;
  finalScores: {
    playerId: string;
    playerName: string;
    totalScore: number;
    rank: number;
  }[];
}
```

## 2. プレイヤー管理イベント

### PlayerJoinedEvent
```typescript
interface PlayerJoinedEvent {
  eventId: string;
  eventType: 'PlayerJoined';
  timestamp: number;
  gameId: string;
  player: {
    playerId: string;
    playerName: string;
    isReady: boolean;
  };
}
```

### PlayerLeftEvent
```typescript
interface PlayerLeftEvent {
  eventId: string;
  eventType: 'PlayerLeft';
  timestamp: number;
  gameId: string;
  playerId: string;
}
```

### PlayerReadyEvent
```typescript
interface PlayerReadyEvent {
  eventId: string;
  eventType: 'PlayerReady';
  timestamp: number;
  gameId: string;
  playerId: string;
}
```

## 3. ラウンド管理イベント

### RoundStartedEvent
```typescript
interface RoundStartedEvent {
  eventId: string;
  eventType: 'RoundStarted';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  players: {
    playerId: string;
    playerName: string;
    isDrawer: boolean;
  }[];
}
```

### RoundEndedEvent
```typescript
interface RoundEndedEvent {
  eventId: string;
  eventType: 'RoundEnded';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  roundScores: {
    playerId: string;
    playerName: string;
    score: number;
  }[];
}
```

## 4. ターン管理イベント

### TurnStartedEvent
```typescript
interface TurnStartedEvent {
  eventId: string;
  eventType: 'TurnStarted';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  turnNumber: number;
  drawer: {
    playerId: string;
    playerName: string;
  };
  timeLimit: number;
}
```

### AnswerSetEvent
```typescript
interface AnswerSetEvent {
  eventId: string;
  eventType: 'AnswerSet';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  turnNumber: number;
  drawerId: string;
  answer: string;
}
```

### DrawingUpdatedEvent
```typescript
interface DrawingUpdatedEvent {
  eventId: string;
  eventType: 'DrawingUpdated';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  turnNumber: number;
  drawerId: string;
  drawingData: string;  // base64形式の描画データ
}
```

### AnswerSubmittedEvent
```typescript
interface AnswerSubmittedEvent {
  eventId: string;
  eventType: 'AnswerSubmitted';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  turnNumber: number;
  playerId: string;
  playerName: string;
  answer: string;
  isCorrect: boolean;
}
```

### TurnEndedEvent
```typescript
interface TurnEndedEvent {
  eventId: string;
  eventType: 'TurnEnded';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  turnNumber: number;
  drawerId: string;
  answer: string;
  correctAnswers: {
    playerId: string;
    playerName: string;
    answer: string;
    score: number;
  }[];
  drawerPenalty: number;
}
```

## 5. スコア管理イベント

### ScoreAddedEvent
```typescript
interface ScoreAddedEvent {
  eventId: string;
  eventType: 'ScoreAdded';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  turnNumber: number;
  playerId: string;
  playerName: string;
  points: number;
  reason: 'correct_answer' | 'drawer_penalty';
  currentTotal: number;
}
```

## 6. チャット管理イベント

### ChatMessageSentEvent
```typescript
interface ChatMessageSentEvent {
  eventId: string;
  eventType: 'ChatMessageSent';
  timestamp: number;
  gameId: string;
  message: {
    messageId: string;
    playerId: string;
    playerName: string;
    content: string;
    type: 'normal' | 'system' | 'correct';
  };
}
```

## 7. システムイベント

### TimeLimitWarningEvent
```typescript
interface TimeLimitWarningEvent {
  eventId: string;
  eventType: 'TimeLimitWarning';
  timestamp: number;
  gameId: string;
  roundNumber: number;
  turnNumber: number;
  remainingSeconds: number;
}
```

### ErrorOccurredEvent
```typescript
interface ErrorOccurredEvent {
  eventId: string;
  eventType: 'ErrorOccurred';
  timestamp: number;
  gameId: string;
  error: {
    code: string;
    message: string;
    details?: any;
  };
}
```

## イベントの特徴

1. **不変性**
   - すべてのイベントは発生後に変更不可
   - イベントIDとタイムスタンプで一意性を保証

2. **CQRS対応**
   - コマンド側：状態変更を引き起こすイベント
   - クエリ側：読み取りモデル更新用のイベント

3. **画面設計との整合**
   - 各画面の状態更新に必要な情報を包含
   - リアルタイム更新に必要なデータを提供

4. **トレーサビリティ**
   - すべてのイベントに発生時刻と関連IDを付与
   - イベントの発生順序を追跡可能 