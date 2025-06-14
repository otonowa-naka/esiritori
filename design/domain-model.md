# お絵描き当てゲーム ドメインモデル

## 1. 集約ルートと集約間の関係

```mermaid
classDiagram
    %% 集約ルート間の関係
    class Game {
        <<Aggregate Root>>
        +GameId id
        +GameStatus status
        +GameSettings settings
        +createGame()
        +startGame()
        +endGame()
        +nextRound()
    }

    class Round {
        <<Aggregate Root>>
        +RoundId id
        +GameId gameId
        +int roundNumber
        +startRound()
        +endRound()
        +nextTurn()
    }

    class Turn {
        <<Aggregate Root>>
        +TurnId id
        +RoundId roundId
        +PlayerId drawerId
        +startTurn()
        +endTurn()
    }

    class Room {
        <<Aggregate Root>>
        +RoomId id
        +RoomStatus status
        +GameId gameId
        +createRoom()
        +joinGame()
        +leaveGame()
    }

    class Player {
        <<Aggregate Root>>
        +PlayerId id
        +PlayerName name
        +Score score
        +joinRoom(roomId)
        +leaveRoom()
    }

    class Drawing {
        <<Aggregate Root>>
        +DrawingId id
        +TurnId turnId
        +PlayerId drawerId
        +saveDrawing()
    }

    class Chat {
        <<Aggregate Root>>
        +ChatId id
        +GameId gameId
        +PlayerId playerId
        +sendMessage()
    }

    %% ドメインサービス
    class GameProgressService {
        <<Domain Service>>
        +startGame()
        +endGame()
        +nextRound()
        +nextTurn()
    }

    class ScoreCalculationService {
        <<Domain Service>>
        +calculateScore()
        +updatePlayerScore()
        +calculateTimeBonus()
    }

    class AnswerValidationService {
        <<Domain Service>>
        +validateAnswer()
        +checkCorrectness()
        +processCorrectAnswer()
    }

    class PlayerManagementService {
        <<Domain Service>>
        +joinGame()
        +leaveGame()
        +setPlayerReady()
        +assignDrawer()
    }

    class ChatManagementService {
        <<Domain Service>>
        +sendMessage()
        +broadcastSystemMessage()
        +notifyCorrectAnswer()
    }

    %% 集約間の関係
    Game "1" -- "*" Round : contains
    Round "1" -- "*" Turn : contains
    Room "1" -- "1" Game : joins
    Game "1" -- "*" Player : has
    Game "1" -- "*" Chat : has
    Turn "1" -- "*" Drawing : has

    %% ドメインサービスとの関係
    GameProgressService ..> Game : uses
    GameProgressService ..> Round : uses
    GameProgressService ..> Turn : uses
    ScoreCalculationService ..> Player : uses
    ScoreCalculationService ..> Turn : uses
    AnswerValidationService ..> Turn : uses
    AnswerValidationService ..> Player : uses
    PlayerManagementService ..> Game : uses
    PlayerManagementService ..> Player : uses
    ChatManagementService ..> Game : uses
    ChatManagementService ..> Chat : uses
    ChatManagementService ..> Player : uses
```

## 2. 集約の詳細

### Game集約
```mermaid
classDiagram
    class Game {
        <<Aggregate Root>>
        +GameId id
        +GameStatus status
        +GameSettings settings
        +Round currentRound
        +createGame()
        +startGame()
        +endGame()
        +nextRound()
    }

    class GameSettings {
        <<Value Object>>
        +TimeLimit timeLimit
        +RoundCount roundCount
        +PlayerCount playerCount
        +validate()
    }

    class TimeLimit {
        <<Value Object>>
        +int seconds
        +validate()
    }

    class RoundCount {
        <<Value Object>>
        +int value
        +validate()
    }

    class PlayerCount {
        <<Value Object>>
        +int min
        +int max
        +validate()
    }

    class GameStatus {
        <<Enumeration>>
        WAITING
        PLAYING
        FINISHED
    }

    Game "1" -- "1" GameSettings : has
    GameSettings -- TimeLimit : has
    GameSettings -- RoundCount : has
    GameSettings -- PlayerCount : has
```

#### Game集約の説明
- **Game(ゲーム)**
  - ゲーム全体を管理する集約ルート
  - ゲーム設定と進行状態を管理
  - ラウンドの進行を制御
  - プレイヤーとチャットの管理

- **GameSettings(ゲーム設定)**
  - 制限時間、ラウンド数、プレイヤー数の設定
  - 設定値のバリデーション

### Round集約
```mermaid
classDiagram
    class Round {
        <<Aggregate Root>>
        +RoundId id
        +GameId gameId
        +int roundNumber
        +RoundStatus status
        +Turn currentTurn
        +startRound()
        +endRound()
        +nextTurn()
    }

    class RoundStatus {
        <<Enumeration>>
        NOT_STARTED
        IN_PROGRESS
        COMPLETED
    }
```

#### Round集約の説明
- **Round(ラウンド)**
  - 1回のしりとりを管理する集約ルート
  - ターンの進行を管理
  - ラウンドの状態を制御

### Turn集約
```mermaid
classDiagram
    class Turn {
        <<Aggregate Root>>
        +TurnId id
        +RoundId roundId
        +PlayerId drawerId
        +Answer answer
        +TurnStatus status
        +TimeLimit timeLimit
        +startTurn()
        +endTurn()
        +validateAnswer()
    }

    class Answer {
        <<Value Object>>
        +String value
        +validate()
    }

    class TurnStatus {
        <<Enumeration>>
        NOT_STARTED
        DRAWING
        GUESSING
        COMPLETED
    }
```

#### Turn集約の説明
- **Turn(ターン)**
  - 1回の描画と回答を管理する集約ルート
  - 描画を管理
  - 正解判定を制御

### Room集約
```mermaid
classDiagram
    class Room {
        <<Aggregate Root>>
        +RoomId id
        +RoomStatus status
        +GameId gameId
        +createRoom()
        +joinGame()
        +leaveGame()
    }

    class RoomStatus {
        <<Enumeration>>
        WAITING
        PLAYING
        FINISHED
    }
```

#### Room集約の説明
- **Room(ルーム)**
  - プレイヤーの集まりを管理する集約ルート
  - ゲームへの参加を管理

### Player集約
```mermaid
classDiagram
    class Player {
        <<Aggregate Root>>
        +PlayerId id
        +PlayerName name
        +Score score
        +PlayerStatus status
        +joinRoom(roomId)
        +leaveRoom()
        +setReady()
        +updateScore()
    }

    class PlayerName {
        <<Value Object>>
        +String value
        +validate()
    }

    class Score {
        <<Value Object>>
        +int value
        +add()
        +subtract()
    }

    class PlayerStatus {
        <<Enumeration>>
        NOT_READY
        READY
        DRAWING
        GUESSING
    }
```

#### Player集約の説明
- **Player(プレイヤー)**
  - プレイヤー情報を管理する独立した集約ルート
  - スコア、状態、名前などの属性を持つ
  - ゲームに直接参加

### Drawing集約
```mermaid
classDiagram
    class Drawing {
        <<Aggregate Root>>
        +DrawingId id
        +TurnId turnId
        +PlayerId drawerId
        +ImageData imageData
        +TimeSpent timeSpent
        +DrawingStatus status
        +saveDrawing()
    }

    class ImageData {
        <<Value Object>>
        +String value
        +validate()
    }

    class TimeSpent {
        <<Value Object>>
        +int seconds
        +validate()
    }

    class DrawingStatus {
        <<Enumeration>>
        IN_PROGRESS
        COMPLETED
    }
```

#### Drawing集約の説明
- **Drawing(描画)**
  - 描画情報を管理する独立した集約ルート
  - 描画データと状態を管理

### Chat集約
```mermaid
classDiagram
    class Chat {
        <<Aggregate Root>>
        +ChatId id
        +GameId gameId
        +PlayerId playerId
        +Message message
        +ChatType type
        +Timestamp createdAt
        +sendMessage()
    }

    class Message {
        <<Value Object>>
        +String value
        +validate()
    }

    class ChatType {
        <<Enumeration>>
        NORMAL
        CORRECT_ANSWER
        SYSTEM
    }
```

#### Chat集約の説明
- **Chat(チャット)**
  - チャットメッセージを管理する独立した集約ルート
  - メッセージタイプを管理
  - ゲーム全体のコミュニケーションを管理

## 3. ドメインサービスの説明

### GameProgressService(ゲーム進行サービス)
- ゲーム全体の進行を管理
- ラウンドとターンの遷移を制御
- ゲームの開始と終了を管理

### ScoreCalculationService(スコア計算サービス)
- プレイヤーのスコア計算
- 時間ボーナスの計算
- スコアの更新処理

### AnswerValidationService(回答検証サービス)
- 回答の正誤判定
- 正解時の処理
- 回答の検証ルール管理

### PlayerManagementService(プレイヤー管理サービス)
- プレイヤーの参加・退出管理
- 準備状態の管理
- 描画者の割り当て

### ChatManagementService(チャット管理サービス)
- チャットメッセージの送信
- システムメッセージの配信
- 正解通知の管理

## 不変条件
- 各集約は独立して存在可能
- 集約間の参照はIDのみを使用
- 集約の整合性は各集約ルートが保証
- ゲームの進行は Game → Round → Turn の順序で制御
- プレイヤーとチャットは直接Gameと連携 