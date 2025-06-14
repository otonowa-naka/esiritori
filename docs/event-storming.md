```mermaid
graph LR
    %% アクター
    Player((プレイヤー))
    System((システム))
    
    %% コマンド
    CreateRoom[部屋作成]
    JoinRoom[部屋参加]
    StartGame[ゲーム開始]
    SubmitDrawing[絵を提出]
    SubmitGuess[回答を提出]
    SendChat[チャット送信]
    
    %% イベント
    RoomCreated[部屋が作成された]
    PlayerJoined[プレイヤーが参加した]
    GameStarted[ゲームが開始された]
    DrawingSubmitted[絵が提出された]
    GuessSubmitted[回答が提出された]
    ChatSent[チャットが送信された]
    RoundEnded[ラウンドが終了した]
    GameEnded[ゲームが終了した]
    
    %% ポリシー
    CheckMinPlayers{最小プレイヤー数チェック}
    ValidateAnswer{回答の検証}
    CalculateScore{スコア計算}
    
    %% アクターとコマンドの関係
    Player --> CreateRoom
    Player --> JoinRoom
    Player --> StartGame
    Player --> SubmitDrawing
    Player --> SubmitGuess
    Player --> SendChat
    
    %% コマンドとイベントの関係
    CreateRoom --> RoomCreated
    JoinRoom --> PlayerJoined
    StartGame --> GameStarted
    SubmitDrawing --> DrawingSubmitted
    SubmitGuess --> GuessSubmitted
    SendChat --> ChatSent
    
    %% イベントとポリシーの関係
    PlayerJoined --> CheckMinPlayers
    GuessSubmitted --> ValidateAnswer
    ValidateAnswer --> CalculateScore
    
    %% ポリシーとイベントの関係
    CheckMinPlayers --> GameStarted
    CalculateScore --> RoundEnded
    RoundEnded --> GameEnded
    
    %% スタイル設定
    classDef actor fill:#f9f,stroke:#333,stroke-width:2px
    classDef command fill:#bbf,stroke:#333,stroke-width:2px
    classDef event fill:#bfb,stroke:#333,stroke-width:2px
    classDef policy fill:#fbb,stroke:#333,stroke-width:2px
    
    class Player,System actor
    class CreateRoom,JoinRoom,StartGame,SubmitDrawing,SubmitGuess,SendChat command
    class RoomCreated,PlayerJoined,GameStarted,DrawingSubmitted,GuessSubmitted,ChatSent,RoundEnded,GameEnded event
    class CheckMinPlayers,ValidateAnswer,CalculateScore policy
```

# イベントストーミング図の説明

## アクター
- プレイヤー: ゲームの参加者
- システム: ゲームの進行を管理するシステム

## コマンド
- 部屋作成: 新しいゲームルームを作成
- 部屋参加: 既存のルームに参加
- ゲーム開始: ゲームを開始
- 絵を提出: お題に基づいて絵を描いて提出
- 回答を提出: 描かれた絵に対する回答を提出
- チャット送信: チャットメッセージを送信

## イベント
- 部屋が作成された: 新しいゲームルームが作成された
- プレイヤーが参加した: プレイヤーがルームに参加した
- ゲームが開始された: ゲームが開始された
- 絵が提出された: プレイヤーが絵を提出した
- 回答が提出された: プレイヤーが回答を提出した
- チャットが送信された: チャットメッセージが送信された
- ラウンドが終了した: 1ラウンドが終了した
- ゲームが終了した: ゲームが終了した

## ポリシー
- 最小プレイヤー数チェック: ゲーム開始に必要な最小プレイヤー数を確認
- 回答の検証: 提出された回答が正解かどうかを検証
- スコア計算: プレイヤーのスコアを計算 