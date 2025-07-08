# エシリトリ開発ガイドライン

このドキュメントは、AIエージェントに対してエシリトリの実装方針を明確に伝えるための仕様書です。
バックエンドは **クリーンアーキテクチャ**・**ドメイン駆動設計（DDD）** に基づき、**リーダブルコード**の原則を踏まえて設計・実装します。

---

## 1. プロジェクト概要

### アーキテクチャ方針
- **クリーンアーキテクチャ**: ビジネスロジックを中心とした層構造
- **ドメイン駆動設計（DDD）**: ドメインモデルアプローチを採用
- **テストファースト**: ユニットテストによる品質保証

### 技術スタック
- **使用言語**: C# (.NET 8.0)
- **Webフレームワーク**: ASP.NET Core
- **テストフレームワーク**: xUnit
- **モックライブラリ**: Moq
- **カバレッジツール**: Coverlet

### プロジェクト構成
```
EsiritoriApi.sln
├── EsiritoriApi.Domain/          # ドメイン層
│   ├── Entities/                 # エンティティ
│   ├── ValueObjects/             # 値オブジェクト
│   └── Game/                     # ゲーム集約
│       ├── Game.cs              # ゲームエンティティ
│       └── IGameRepository.cs   # リポジトリインターフェース
├── EsiritoriApi.Application/     # アプリケーション層
│   ├── DTOs/                     # データ転送オブジェクト
│   └── UseCases/                 # ユースケース
├── EsiritoriApi.Infrastructure/  # インフラストラクチャ層
│   └── Repositories/             # リポジトリ実装
├── EsiritoriApi.Api/            # プレゼンテーション層
│   └── Controllers/              # APIコントローラー
├── EsiritoriApi.Domain.Tests/        # ドメイン層テスト
├── EsiritoriApi.Application.Tests/   # アプリケーション層テスト
├── EsiritoriApi.Infrastructure.Tests/ # インフラ層テスト
├── EsiritoriApi.Api.Tests/          # API層テスト
└── EsiritoriApi.Integration.Tests/  # 統合テスト
```

---

## 2. 基本方針・制約

### 要件定義テンプレート

#### 機能名
- ゲーム機能の実装（例：新しいゲームの作成）

#### ユースケース（アクター視点）
- ユーザーが新しいゲームを作成できる
- ゲームの設定を指定できる

#### 完了条件
- バリデーションに通過したゲームデータがDBに保存される
- イベントが発行される（例：`GameCreatedEvent`）

#### ドメインモデル（概略）
- `Game` エンティティ
  - `GameId`（Value Object）
  - `GameSettings`（Value Object）
  - `GameStatus`（Value Object）

### 開発制約事項
- **命名は英語**、かつ**ドメインの意味に沿うこと**
- **コメント不要**（意味のある名前をつけることで読解可能に）
- **テストファースト**：ユースケースレベルでテストを書く（ユニットテスト）

### 入出力仕様
- **入力**：HTTPリクエスト（JSON）
- **出力**：HTTPレスポンス（JSON）

---

## 3. アーキテクチャ設計

### クリーンアーキテクチャ層構成
```
┌─────────────────────────────────────┐
│        Presentation Layer          │
│    (Controllers, API Endpoints)    │
├─────────────────────────────────────┤
│        Application Layer           │
│     (UseCases, DTOs, Interfaces)   │
├─────────────────────────────────────┤
│          Domain Layer              │
│   (Entities, ValueObjects, Rules)  │
├─────────────────────────────────────┤
│       Infrastructure Layer         │
│    (Repositories, External APIs)   │
└─────────────────────────────────────┘
```

### 依存関係ルール
- **Presentation** → **Application** → **Domain**
- **Infrastructure** → **Domain**
- **Domain**は他の層に依存しない
- **Application**は**Infrastructure**に依存しない

### ドメインモデルアプローチ
- **イベント駆動アーキテクチャではなく、ドメインモデルアプローチを採用**
- エンティティと値オブジェクトでビジネスロジックをカプセル化
- ドメインサービスでエンティティ間の複雑な操作を実装
- リポジトリパターンでデータ永続化を抽象化

---

## 4. ドメイン設計ルール

### Entity設計原則
- ビジネスルールを内包する
- 不変性を保つように設計
- 一意のIDで識別される

```csharp
public sealed class Game
{
    public GameId Id { get; private set; }
    public GameStatus Status { get; private set; }
    
    public Game(GameId id, GameSettings settings, PlayerName creatorName, PlayerId creatorId)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        // ビジネスロジックの実装
    }
    
    // ドメインメソッドでビジネスルールを実装
    public void AddPlayer(PlayerId playerId, PlayerName playerName, DateTime now) { }
}
```

### ValueObject設計・実装ルール

#### 基本原則
- 値オブジェクトは必ず不変（immutable）とする
- 同値性は値で判断する
- 値のバリデーションはコンストラクタで必ず行う
- nullや不正な値が渡された場合はDomainErrorException等の例外を投げる

#### 実装例
```csharp
public sealed record GameId(string Value)
{
    public GameId() : this(string.Empty) { }
    
    public static GameId NewId()
    {
        return new GameId(Guid.NewGuid().ToString());
    }
    
    public GameId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainErrorException(DomainErrorCodes.GameId.Empty, "ゲームIDは空にできません");
        Value = value;
    }
}
```

#### ID生成の責務分離
- **✅ 推奨**: ValueObjectの`NewId()`メソッドを使用
- **❌ 禁止**: エンティティやユースケースで独自のID生成ロジックを実装

**✅ 推奨例**
```csharp
// CreateGameUseCase.cs
var gameId = GameId.NewId();
var playerId = PlayerId.NewId();

// Player.cs
public static Player CreateInitial(PlayerName name)
{
    var id = PlayerId.NewId();
    return new Player(id, name, PlayerStatus.NotReady, false, false);
}
```

#### WithXXX関数の禁止
- **❌ 禁止**: 個別のWithXXX関数
- **✅ 推奨**: ドメインの操作を表現する関数

**✅ 推奨例**
```csharp
public Turn StartSettingAnswer()
public Turn SetAnswerAndStartDrawing(string answer, DateTime startTime)
public Turn FinishTurn(DateTime endedAt)
```

#### 実装パターン
```csharp
public sealed class Turn : IEquatable<Turn>
{
    public TurnStatus Status { get; private set; }
    
    private Turn Clone()
    {
        return new Turn(TurnNumber, DrawerId, Answer, Status, TimeLimit, StartedAt, EndedAt, CorrectPlayerIds);
    }
    
    public Turn StartSettingAnswer()
    {
        var clone = Clone();
        clone.Status = TurnStatus.SettingAnswer;
        return clone;
    }
}
```

#### 命名規則
- **操作を表現**: `StartDrawing`, `FinishTurn`, `AddCorrectPlayer`
- **状態変更を表現**: `SetAnswerAndStartDrawing`
- **動詞 + 名詞**: 何をするかを明確に表現

### Repository配置ルール

#### 配置場所
**✅ 推奨**
```
Domain/
└── Game/
    ├── IGameRepository.cs    // エンティティと同じディレクトリに配置
    ├── Game.cs              // エンティティ
    └── ValueObjects/
        └── GameId.cs
```

**❌ 禁止**
```
Domain/
└── Interfaces/              // 汎用のInterfacesディレクトリは使用しない
    └── IGameRepository.cs
```

#### 名前空間
```csharp
namespace EsiritoriApi.Domain.Game;  // エンティティと同じ名前空間

public interface IGameRepository
{
    Task SaveAsync(Game game, CancellationToken cancellationToken = default);
    Task<Game?> FindByIdAsync(GameId id, CancellationToken cancellationToken = default);
}
```

---

## 5. アプリケーション層ルール

### UseCase実装

```csharp
public interface ICreateGameUseCase
{
    Task<CreateGameResponse> ExecuteAsync(CreateGameRequest request, CancellationToken cancellationToken = default);
}

public class CreateGameUseCase : ICreateGameUseCase
{
    private readonly IGameRepository _gameRepository;
    
    public CreateGameUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }
    
    public async Task<CreateGameResponse> ExecuteAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        // ユースケースロジックの実装
        var creator = Player.CreateInitial(request.CreatorName);
        var game = Game.NewGame(request.Settings, creator, DateTime.UtcNow);
        
        await _gameRepository.SaveAsync(game, cancellationToken);
        
        return CreateGameResponse.FromGame(game);
    }
}
```

### DTOマッピングルール

#### MapToResponse関数はDTO側に定義する
- **❌ 禁止**: UseCaseクラスでMapToResponse関数を定義
- **✅ 推奨**: DTO自体にFromEntity/FromDomainファクトリメソッドを定義

**✅ 推奨例**
```csharp
// CreateGameResponse.cs
public static CreateGameResponse FromGame(Game game)
{
    return new CreateGameResponse
    {
        Game = GameDto.FromGame(game),
        Player = PlayerDto.FromPlayer(game.Players.First())
    };
}

// GameDto.cs
public static GameDto FromGame(Game game)
{
    return new GameDto
    {
        Id = game.Id.Value,
        Status = game.Status.ToString(),
        Settings = GameSettingsDto.FromGameSettings(game.Settings),
        CurrentRound = RoundDto.FromRound(game.CurrentRound),
        Players = game.Players.Select(PlayerDto.FromPlayer).ToList(),
        ScoreRecords = game.ScoreHistories.Select(ScoreRecordDto.FromScoreHistory).ToList()
    };
}
```

#### 階層化されたDTOマッピング
- **複合DTOの場合**: 上位DTOから下位DTOのファクトリメソッドを呼び出す
- **単純DTOの場合**: 直接ドメインオブジェクトから値をマッピング

#### 命名規則
- **FromXXX**: ドメインオブジェクトからDTOを生成（例：`FromGame`, `FromPlayer`）
- **ToXXX**: DTOからドメインオブジェクトを生成（例：`ToGameSettings`）

### エラーハンドリング

#### ドメインエラーはDomainErrorExceptionを使用
- **❌ 禁止**: 汎用的な例外（ArgumentException、InvalidOperationException）
- **✅ 推奨**: ドメイン固有のDomainErrorException

**✅ 推奨例**
```csharp
// ドメイン固有の例外を使用
if (timeLimit < 30)
    throw new DomainErrorException(DomainErrorCodes.GameSettings.InvalidTimeLimit, "制限時間は30秒以上である必要があります");

// エラーコード定義
public static class DomainErrorCodes
{
    public static class Game
    {
        public const string NotFound = "GAME_NOT_FOUND";
        public const string AlreadyStarted = "GAME_ALREADY_STARTED";
    }
    
    public static class GameSettings
    {
        public const string InvalidTimeLimit = "GAME_SETTINGS_INVALID_TIME_LIMIT";
        public const string InvalidPlayerCount = "GAME_SETTINGS_INVALID_PLAYER_COUNT";
    }
}
```

#### APIレベルでのエラーハンドリング
```csharp
[HttpPost]
public async Task<ActionResult<CreateGameResponse>> CreateGame([FromBody] CreateGameRequest request)
{
    try
    {
        var response = await _createGameUseCase.ExecuteAsync(request);
        return CreatedAtAction(nameof(CreateGame), new { id = response.Game.Id }, response);
    }
    catch (DomainErrorException ex)
    {
        return BadRequest(new { error = ex.Message, code = ex.ErrorCode });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "内部サーバーエラーが発生しました", details = ex.Message });
    }
}
```

#### テストでのエラーコード検証
**✅ 推奨例**
```csharp
// エラーコードとメッセージを検証
var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
Assert.Equal(DomainErrorCodes.Game.AlreadyStarted, exception.ErrorCode);
Assert.Equal("ゲームは既に開始されています", exception.ErrorMessage);
```

---

## 6. 開発実装ルール

### オブジェクト状態判断のルール

#### ドメインの言葉で表現する
- **❌ 禁止**: プロパティの値を直接参照して条件判断
- **✅ 推奨**: ドメインの意図を表現するメソッドを使用

**✅ 推奨例**
```csharp
public sealed class Game
{
    // ドメインの意図を表現するメソッド
    public bool IsAlreadyStarted() => Status != GameStatus.Waiting;
    public bool AreAllPlayersReady() => Players.All(p => p.IsReady);
    public bool HasMinimumPlayers() => Players.Count >= 2;
    public bool CanStartGame() => Status == GameStatus.Waiting && 
                                  Players.Count >= 2 && 
                                  Players.All(p => p.IsReady);
}

// 使用例
if (game.IsAlreadyStarted())
    throw new DomainErrorException(DomainErrorCodes.Game.AlreadyStarted, "ゲームは既に開始されています");
```

#### 命名規則
- **Is○○**: 状態の確認（例：`IsReady`, `IsDrawer`）
- **Has○○**: 存在・所有の確認（例：`HasMinimumPlayers`）
- **Can○○**: 可能・許可の確認（例：`CanStartGame`）
- **Are○○**: 複数要素の状態確認（例：`AreAllPlayersReady`）

### オブジェクト比較のルール

#### 適切なレベルで比較する
- **プレイヤー全体の比較**: 重複チェック、存在確認
- **ID比較**: プレイヤー検索、描画者判定、状態更新

**✅ 推奨例**
```csharp
// プレイヤーの重複チェックはプレイヤー全体で比較
if (Players.Any(p => p.Equals(player)))
    throw new DomainErrorException(DomainErrorCodes.Game.PlayerAlreadyExists, "このプレイヤーは既に参加しています");

// プレイヤー検索はID比較が適切
var playerIndex = Players.ToList().FindIndex(p => p.Id.Equals(playerId));
```

### 時間処理ルール

#### ドメイン層での時間取得は禁止
- **❌ 禁止**: ドメイン層で`DateTime.UtcNow`を直接使用
- **✅ 推奨**: 時間をパラメータで受け取る

**✅ 推奨例**
```csharp
// Application層で時間を取得
public async Task<CreateGameResponse> ExecuteAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
{
    var now = DateTime.UtcNow;  // Application層で取得
    var game = Game.NewGame(request.Settings, creator, now);  // ドメインに渡す
}

// Domain層では時間をパラメータで受け取る
public void AddPlayer(Player player, DateTime now)
{
    // ... 処理 ...
    UpdatedAt = now;
}
```

#### 影響を受けるメソッド例
- `Game.AddPlayer(Player player, DateTime now)`
- `Game.StartGame(DateTime now)`
- `Game.EndGame(DateTime now)`
- `Game.AddScoreHistory(ScoreHistory scoreHistory, DateTime now)`
- `Game.UpdatePlayerReadyStatus(PlayerId playerId, bool isReady, DateTime now)`

### ファクトリメソッドルール

#### UseCaseでのドメインオブジェクト生成
- **❌ 禁止**: UseCaseでドメインオブジェクトのコンストラクタを直接呼び出し
- **✅ 推奨**: 生成ルールが埋め込まれたファクトリメソッドを使用

**✅ 推奨例**
```csharp
// Game.cs
public static Game NewGame(GameSettings settings, Player initialPlayer, DateTime now)
{
    var id = GameId.NewId();  // ID生成を内部化
    var players = new List<Player> { initialPlayer };
    var initialTurn = Turn.CreateInitial(initialPlayer.Id, settings.TimeLimit, now);
    var initialRound = Round.CreateInitial(initialTurn, now);
    return new Game(id, settings, GameStatus.Waiting, initialRound, players, new List<ScoreHistory>(), now, now);
}

// Player.cs
public static Player CreateInitial(PlayerName name)
{
    var id = PlayerId.NewId();  // ID生成を内部化
    return new Player(id, name, PlayerStatus.NotReady, false, false);
}
```

#### 命名規則
- **NewXXX**: 完全に新しいオブジェクトの生成（例：`Game.NewGame`, `GameId.NewId`）
- **CreateInitial**: 初期状態のオブジェクトの生成（例：`Player.CreateInitial`）
- **CreateXXX**: 特定の状態・目的のオブジェクトの生成（例：`Round.CreateInitial`）

#### ファクトリメソッドの責務
- **ID生成**: 必要なIDを内部で生成
- **初期値設定**: 適切な初期状態を設定
- **依存オブジェクト生成**: 関連する子オブジェクトを生成
- **バリデーション**: 生成時の制約チェック

---

## 7. テスト実装ガイド

### テスト構成

#### テストカテゴリ分類
- `[Trait("Category", "ドメインモデル")]`: ドメインロジックのテスト
- `[Trait("Category", "ユースケース")]`: アプリケーション層のテスト
- `[Trait("Category", "API")]`: コントローラーのテスト
- `[Trait("Category", "統合テスト")]`: エンドツーエンドテスト

### 日本語テスト仕様（生きたドキュメント）

```csharp
public sealed class GameCreationTests
{
    [Fact]
    [Trait("Category", "ゲーム作成機能")]
    public void 有効な設定でゲームが正常に作成される()
    {
        // Arrange: テストデータの準備
        var request = new CreateGameRequest
        {
            CreatorName = "テストプレイヤー",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };
        
        // Act: 実行
        var result = await _useCase.ExecuteAsync(request);
        
        // Assert: 検証
        Assert.NotNull(result);
        Assert.NotEmpty(result.GameId);
    }
    
    [Fact]
    [Trait("Category", "ゲーム作成機能")]
    public void 無効なプレイヤー数の場合はエラーが発生する()
    {
        // 境界値テストとエラーハンドリングの検証
        var exception = Assert.Throws<DomainErrorException>(() => 
            new GameSettings(60, 3, 1)); // 最小プレイヤー数未満
        
        Assert.Equal(DomainErrorCodes.GameSettings.InvalidPlayerCount, exception.ErrorCode);
        Assert.Contains("プレイヤー数は2人から8人の間で設定してください", exception.Message);
    }
}
```

### ValueObject操作のテスト例

```csharp
[Fact]
public void お題設定を開始できる()
{
    var turn = Turn.CreateInitial(drawerId, 60);
    var updatedTurn = turn.StartSettingAnswer();
    
    Assert.Equal(TurnStatus.SettingAnswer, updatedTurn.Status);
}
```

### テスト観点
- **入力バリデーション**: 境界値・異常値のテスト
- **正常系**: ビジネスロジックの正しい動作確認
- **異常系**: エラーハンドリングの確認
- **状態遷移**: オブジェクトの状態変化の確認

### コードカバレッジ要件
- **C1カバレッジ（条件分岐網羅）で80%以上を達成**
- Coverletを使用してカバレッジ測定
- 各層ごとのカバレッジ実績・目標：
  - **Domain層**: 実績82.89%、目標85%以上
  - **Application層**: 実績88.81%、目標90%以上
  - **Infrastructure層**: 実績100%、目標100%
  - **API層**: 実績53.94%、目標70%以上

### 依存性注入設定
```csharp
// Program.cs
builder.Services.AddScoped<IGameRepository, InMemoryGameRepository>();
builder.Services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
```

---

## 実装チェックリスト

### ValueObject実装
- [ ] ValueObjectに`NewId()`メソッドが実装されている
- [ ] エンティティやユースケースで独自のID生成ロジックがない
- [ ] WithXXX関数を使用していない
- [ ] ドメインの操作を表現するメソッドを使用している

### ドメイン設計
- [ ] プロパティの値を直接参照した条件判断がない
- [ ] 状態判断は`Is○○`, `Has○○`, `Can○○`, `Are○○`メソッドを使用
- [ ] Repository は Domain層のエンティティと同じディレクトリに配置

### アプリケーション層
- [ ] UseCaseにMapToResponse関数がない
- [ ] DTOにFromXXXファクトリメソッドが実装されている
- [ ] DomainErrorExceptionを使用している
- [ ] エラーコードがDomainErrorCodesに定義されている

### テスト
- [ ] テストでエラーコードを検証している
- [ ] 日本語のテストメソッド名を使用している
- [ ] 適切なTraitカテゴリを設定している
- [ ] ファクトリメソッドを使用してテストオブジェクトを生成している