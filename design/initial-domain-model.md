# お絵描き当てゲーム 初期ドメインモデル

## ドメインモデル図

```mermaid
classDiagram
    class Room {
        <<Aggregate Root>>
        +RoomId id
        +RoomStatus status
        +GameSettings settings
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
        +TurnId turnId
        +PlayerId playerId
        +sendMessage()
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

    class RoomStatus {
        <<Enumeration>>
        WAITING
        PLAYING
        FINISHED
    }

    class PlayerStatus {
        <<Enumeration>>
        NOT_READY
        READY
        DRAWING
        GUESSING
    }

    class DrawingStatus {
        <<Enumeration>>
        IN_PROGRESS
        COMPLETED
    }

    class ChatType {
        <<Enumeration>>
        NORMAL
        CORRECT_ANSWER
        SYSTEM
    }

    class GameService {
        <<Domain Service>>
        +calculateScore()
        +validateAnswer()
        +manageRound()
        +manageTurn()
        +controlGameProgress()
        +coordinateAggregates()
    }

    Room "1" -- "*" Player : contains
    Room "1" -- "*" Drawing : contains
    Room "1" -- "*" Chat : contains
    Room "1" -- "1" GameSettings : has
    GameSettings -- TimeLimit : has
    GameSettings -- RoundCount : has
    GameSettings -- PlayerCount : has

    GameService ..> Room : uses
    GameService ..> Player : uses
    GameService ..> Drawing : uses
    GameService ..> Chat : uses
```

## エンティティの説明

### Room(ルーム)
- ゲームの基本単位となる集約ルート
- プレイヤー、描画、チャットを管理
- ゲーム設定を保持
- ルームの状態を管理

### Player(プレイヤー)
- プレイヤー情報を管理する集約ルート
- スコア、状態、名前などの属性を持つ
- ルームへの参加・退出を管理

### Drawing(描画)
- 描画情報を管理する集約ルート
- 描画データと状態を管理
- ターンと描画者の情報を保持

### Chat(チャット)
- チャットメッセージを管理する集約ルート
- メッセージタイプを管理
- ターンとプレイヤーの情報を保持

### GameSettings(ゲーム設定)
- 制限時間、ラウンド数、プレイヤー数の設定
- 設定値のバリデーション

### GameService(ゲームサービス)
- ゲーム全体の進行を管理
- スコア計算と回答検証
- ラウンドとターンの管理
- 集約間の協調

## 不変条件
- 各集約は独立して存在可能
- 集約間の参照はIDのみを使用
- 集約の整合性は各集約ルートが保証
- ゲームの進行はドメインサービスが制御 