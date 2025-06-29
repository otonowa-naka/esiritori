# 概要
このドキュメントは、AIエージェントに対して指定機能の実装方針を明確に伝えるための仕様書です。
バックエンドは **クリーンアーキテクチャ**・**ドメイン駆動設計（DDD）** に基づき、**リーダブルコード**の原則を踏まえて設計・実装します。

---

## 1. 要件定義

### 機能名
- ○○○（例：応募者のプロフィール更新）

### ユースケース（アクター視点）
- ユーザーがプロフィール情報を更新できる

### 完了条件
- バリデーションに通過した内容がDBに保存される
- イベントが発行される（例：`ProfileUpdatedEvent`）

---

## 2. 実装方針

### アーキテクチャ
- クリーンアーキテクチャに従い、以下の層を意識して実装
  - `Usecase（Application層）`
  - `Domain（Entity, ValueObject, DomainService）`
  - `Interface（Controller, Presenter, Repository interfaceなど）`
  - `Infrastructure（DB, API, etc）`

### 入出力
- 入力：HTTPリクエスト（JSON）
- 出力：HTTPレスポンス（JSON）

### ドメインモデル（概略）
- `Profile` エンティティ
  - `Name`（Value Object）
  - `Email`（Value Object）
  - `Birthday`（Value Object）

---

## 3. クラス設計のヒント

### Entity
- ビジネスルールを内包する
- 不変性を保つように設計

### Value Object
- 同値性は値で判断する
- Immutableにする

### Usecase
- 外部インターフェースや永続化処理の詳細には依存しない
- 入出力DTOを介して情報をやり取り

---

## 4. 非機能要件 / 制約事項

- **命名は英語**、かつ**ドメインの意味に沿うこと**
- **コメント不要**（意味のある名前をつけることで読解可能に）
- **テストファースト**：ユースケースレベルでテストを書く（ユニットテスト）

---

## 5. テスト仕様（例）

### テスト観点
- 入力バリデーション
- 正常系でDBに保存されること
- イベントが発行されること

---

## 6. 補足

- 使用言語：Go（または任意指定）
- ORM：SQLBoilerなど（必要に応じて明記）
- その他外部API：なし（ある場合は明記）

---

## 7. C# 実装ガイドライン

### 言語・フレームワーク
- **使用言語**: C# (.NET 8.0)
- **Webフレームワーク**: ASP.NET Core
- **テストフレームワーク**: xUnit
- **モックライブラリ**: Moq
- **カバレッジツール**: Coverlet

### アーキテクチャ構成
C#実装では以下のプロジェクト構成でクリーンアーキテクチャを実現：

```
EsiritoriApi.sln
├── EsiritoriApi.Domain/          # ドメイン層
│   ├── Entities/                 # エンティティ
│   └── ValueObjects/             # 値オブジェクト
├── EsiritoriApi.Application/     # アプリケーション層
│   ├── DTOs/                     # データ転送オブジェクト
│   ├── Interfaces/               # リポジトリインターフェース
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

### ドメインモデルアプローチ
- **イベント駆動アーキテクチャではなく、ドメインモデルアプローチを採用**
- エンティティと値オブジェクトでビジネスロジックをカプセル化
- ドメインサービスでエンティティ間の複雑な操作を実装
- リポジトリパターンでデータ永続化を抽象化

### C# 固有の実装規約

#### 値オブジェクト (Value Objects)
```csharp
public sealed record GameId(string Value)
{
    public GameId() : this(string.Empty) { }
    
    public GameId(string value) : this(ValidateAndFormat(value)) { }
    
    private static string ValidateAndFormat(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ゲームIDは空にできません");
        return value;
    }
}
```

#### エンティティ (Entities)
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
    public void AddPlayer(PlayerId playerId, PlayerName playerName) { }
}
```

#### ユースケース (Use Cases)
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
    }
}
```

### テスト仕様（C# 版）

#### 日本語テスト仕様（生きたドキュメント）
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
        var exception = Assert.Throws<ArgumentException>(() => 
            new GameSettings(60, 3, 1)); // 最小プレイヤー数未満
        
        Assert.Contains("プレイヤー数は2人から8人の間で設定してください", exception.Message);
    }
}
```

#### テストカテゴリ分類
- `[Trait("Category", "ドメインモデル")]`: ドメインロジックのテスト
- `[Trait("Category", "ユースケース")]`: アプリケーション層のテスト
- `[Trait("Category", "API")]`: コントローラーのテスト
- `[Trait("Category", "統合テスト")]`: エンドツーエンドテスト

### コードカバレッジ要件
- **C1カバレッジ（条件分岐網羅）で80%以上を達成**
- Coverletを使用してカバレッジ測定
- 各層ごとのカバレッジ目標：
  - Domain層: 85%以上
  - Application層: 90%以上
  - Infrastructure層: 100%
  - Api層: 70%以上（コントローラーの例外ハンドリング含む）

### 依存性注入設定
```csharp
// Program.cs
builder.Services.AddScoped<IGameRepository, InMemoryGameRepository>();
builder.Services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
```

### エラーハンドリング
```csharp
[HttpPost]
public async Task<ActionResult<CreateGameResponse>> CreateGame([FromBody] CreateGameRequest request)
{
    try
    {
        var response = await _createGameUseCase.ExecuteAsync(request);
        return CreatedAtAction(nameof(CreateGame), new { id = response.Game.Id }, response);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "内部サーバーエラーが発生しました", details = ex.Message });
    }
}
```

### Value Object の実装ルール

#### 1. WithXXX関数の禁止

**❌ 禁止**
```csharp
// 個別のWithXXX関数は禁止
public Turn WithStatus(TurnStatus status)
public Turn WithAnswer(string answer)
public Turn WithStartedAt(DateTime startedAt)
```

**✅ 推奨**
```csharp
// ドメインの操作を表現する関数を使用
public Turn StartSettingAnswer()
public Turn StartDrawing(DateTime startTime)
public Turn SetAnswerAndStartDrawing(string answer, DateTime startTime)
public Turn FinishTurn(DateTime endedAt)
```

#### 2. 理由

- **可読性**: ドメインの意図が明確に伝わる
- **保守性**: 同時に変更すべき値が明確
- **エラー防止**: 不適切な状態変更を防げる
- **DDD準拠**: ドメインの操作として表現される

#### 3. 実装パターン

```csharp
public sealed class Turn : IEquatable<Turn>
{
    // Private Setを使用して不変性を保証
    public TurnStatus Status { get; private set; }
    
    // Clone関数で新しいインスタンスを作成
    private Turn Clone()
    {
        return new Turn(TurnNumber, DrawerId, Answer, Status, TimeLimit, StartedAt, EndedAt, CorrectPlayerIds);
    }
    
    // ドメインの操作を表現する関数
    public Turn StartSettingAnswer()
    {
        var clone = Clone();
        clone.Status = TurnStatus.SettingAnswer;
        return clone;
    }
}
```

#### 4. 命名規則

- **操作を表現**: `StartDrawing`, `FinishTurn`, `AddCorrectPlayer`
- **状態変更を表現**: `SetAnswerAndStartDrawing`
- **動詞 + 名詞**: 何をするかを明確に表現

#### 5. テスト

各ドメイン操作に対して、適切なテストを作成する：

```csharp
[Fact]
public void お題設定を開始できる()
{
    var turn = Turn.CreateInitial(drawerId, 60);
    var updatedTurn = turn.StartSettingAnswer();
    
    Assert.Equal(TurnStatus.SettingAnswer, updatedTurn.Status);
}
```

### 値オブジェクト（ValueObject）共通実装ルール
- 値オブジェクトは必ず不変（immutable）とする。
- 値のバリデーションはコンストラクタで必ず行う。
- nullや不正な値が渡された場合はArgumentException等の例外を投げる。
- 同値性（Equals, ==, !=）は値（Valueプロパティ等）で判定する。
- ToString()は値の文字列表現を返す。
- ドメイン層以外から直接newせず、原則としてバリューオブジェクト経由で生成・利用する。
- 未設定状態はOption<T>型などで明示的に表現し、nullや特殊値で未設定を表さない。

### ID生成の責務分離ルール

#### 1. ID生成はValueObject側に集約する
- **❌ 禁止**: エンティティやユースケースで独自のID生成ロジックを実装
- **✅ 推奨**: ValueObjectの`NewId()`メソッドを使用

**❌ 禁止例**
```csharp
// CreateGameUseCase.cs
private static string GenerateGameId()
{
    return Random.Shared.Next(100000, 999999).ToString();
}

private static string GeneratePlayerId()
{
    return Guid.NewGuid().ToString("N")[..12];
}

// Player.cs
private static string GeneratePlayerId()
{
    return Guid.NewGuid().ToString("N")[..12];
}
```

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

#### 2. ValueObjectのNewId()メソッド実装
```csharp
public sealed class GameId : IEquatable<GameId>
{
    /// <summary>
    /// 新しいGameIdを生成します（GUID）
    /// </summary>
    public static GameId NewId()
    {
        return new GameId(Guid.NewGuid().ToString());
    }
}

public sealed class PlayerId : IEquatable<PlayerId>
{
    /// <summary>
    /// 新しいPlayerIdを生成します（GUID）
    /// </summary>
    public static PlayerId NewId()
    {
        return new PlayerId(Guid.NewGuid().ToString());
    }
}
```

#### 3. 理由
- **責務分離**: ID生成ロジックが一箇所に集約される
- **保守性**: ID生成方式の変更が容易
- **テスト容易性**: ID生成のテストがValueObject側で完結
- **一貫性**: 同じ種類のIDが同じ方式で生成される
- **重複排除**: 同じ処理のコピーが発生しない

#### 4. 実装チェックリスト
- [ ] ValueObjectに`NewId()`メソッドが実装されている
- [ ] エンティティやユースケースで独自のID生成ロジックがない
- [ ] すべてのID生成箇所で`ValueObject.NewId()`を使用している
- [ ] ID生成方式がドキュメント化されている

## ValueObjectの設計・実装ルール（責務分離・ID生成）

### 1. 識別子（ID）生成の責務はValueObject側に集約する
- GameIdやPlayerIdなど、エンティティの一意性を担保するIDの生成（例：GUID）は、ValueObject側のファクトリメソッド（例：NewId）で行うこと。
- エンティティや集約ルートのコンストラクタ・ファクトリでは、ID生成処理を直接書かず、必ずValueObjectの生成メソッドを呼び出すこと。
- 例：
  ```csharp
  var gameId = GameId.NewId();
  var playerId = PlayerId.NewId();
  ```
- これにより、ID生成の実装が一箇所に集約され、ドメイン層の責務分離・保守性・テスト容易性が向上する。

### 2. ValueObjectの責務
- 値の妥当性検証（null/空文字/範囲チェック等）はValueObjectのコンストラクタやファクトリで必ず行うこと。
- 生成・変換・パースなど、値に関するロジックはValueObject側に集約する。

### 3. エンティティ/集約ルートの責務
- IDや値オブジェクトの生成はValueObject側に委譲し、エンティティ/集約ルートは「集約の構成・状態遷移・ビジネスルール」に集中すること。

---

（例：GameId/PlayerIdのID生成責務分離、値検証の一元化など）

---

### オブジェクトの状態判断のルール

#### 1. オブジェクトの状態判断はドメインの言葉で表現する
- **❌ 禁止**: プロパティの値を直接参照して条件判断
- **✅ 推奨**: ドメインの意図を表現するメソッドを使用

**❌ 禁止例**
```csharp
// プロパティの値を直接参照
if (game.Status == GameStatus.Waiting)
    throw new InvalidOperationException("ゲームは既に開始されています");

if (!Players.All(p => p.IsReady))
    throw new InvalidOperationException("全てのプレイヤーが準備完了状態である必要があります");

if (Players.Count < 2)
    throw new InvalidOperationException("ゲームを開始するには最低2人のプレイヤーが必要です");
```

**✅ 推奨例**
```csharp
// ドメインの意図を表現するメソッド
if (game.IsAlreadyStarted())
    throw new InvalidOperationException("ゲームは既に開始されています");

if (!game.AreAllPlayersReady())
    throw new InvalidOperationException("全てのプレイヤーが準備完了状態である必要があります");

if (!game.HasMinimumPlayers())
    throw new InvalidOperationException("ゲームを開始するには最低2人のプレイヤーが必要です");
```

#### 2. 状態判断メソッドの命名規則
- **Is○○**: 状態の確認（例：`IsReady`, `IsDrawer`, `IsGameStarted`）
- **Has○○**: 存在・所有の確認（例：`HasMinimumPlayers`, `HasAnswer`）
- **Can○○**: 可能・許可の確認（例：`CanStartGame`, `CanAddPlayer`）
- **Are○○**: 複数要素の状態確認（例：`AreAllPlayersReady`）

#### 3. 実装例
```csharp
public sealed class Game
{
    public GameStatus Status { get; private set; }
    public IReadOnlyList<Player> Players { get; private set; }
    
    // ドメインの意図を表現するメソッド
    public bool IsAlreadyStarted() => Status != GameStatus.Waiting;
    public bool AreAllPlayersReady() => Players.All(p => p.IsReady);
    public bool HasMinimumPlayers() => Players.Count >= 2;
    public bool CanStartGame() => Status == GameStatus.Waiting && 
                                  Players.Count >= 2 && 
                                  Players.All(p => p.IsReady);
    public bool CanAddPlayer() => Status == GameStatus.Waiting && 
                                  Players.Count < Settings.PlayerCount;
}
```

#### 4. 理由
- **可読性**: ドメインの意図が明確に伝わる
- **保守性**: 条件ロジックの変更が容易
- **カプセル化**: オブジェクトの内部実装に依存しない
- **DDD準拠**: ドメインの言葉でビジネスルールを表現

#### 5. 実装チェックリスト
- [ ] プロパティの値を直接参照した条件判断がない
- [ ] 状態判断は`Is○○`, `Has○○`, `Can○○`, `Are○○`メソッドを使用
- [ ] メソッド名がドメインの意図を表現している
- [ ] 複雑な条件は適切にメソッドに分割されている

---

### オブジェクト比較のルール

#### 1. エンティティ・値オブジェクトの比較は適切なレベルで行う
- **❌ 禁止**: 不適切なレベルでの比較
- **✅ 推奨**: 意図に応じた適切なレベルでの比較

**❌ 禁止例**
```csharp
// プレイヤーの重複チェックでID比較を使用
if (Players.Any(p => p.Id.Equals(player.Id)))
    throw new InvalidOperationException("このプレイヤーは既に参加しています");
```

**✅ 推奨例**
```csharp
// プレイヤーの重複チェックはプレイヤー全体で比較
if (Players.Any(p => p.Equals(player)))
    throw new InvalidOperationException("このプレイヤーは既に参加しています");

// プレイヤー検索はID比較が適切
var playerIndex = Players.ToList().FindIndex(p => p.Id.Equals(playerId));
```

#### 2. 比較レベルの使い分け
- **プレイヤー全体の比較**: 重複チェック、存在確認
- **ID比較**: プレイヤー検索、描画者判定、状態更新

#### 3. 理由
- **意図の明確性**: 何を比較したいかが明確になる
- **保守性**: 比較ロジックの変更が容易
- **DDD準拠**: ドメインの意図に沿った比較

---

## ValueObjectの設計・実装ルール（責務分離・ID生成）

### 1. 識別子（ID）生成の責務はValueObject側に集約する
- GameIdやPlayerIdなど、エンティティの一意性を担保するIDの生成（例：GUID）は、ValueObject側のファクトリメソッド（例：NewId）で行うこと。
- エンティティや集約ルートのコンストラクタ・ファクトリでは、ID生成処理を直接書かず、必ずValueObjectの生成メソッドを呼び出すこと。
- 例：
  ```csharp
  var gameId = GameId.NewId();
  var playerId = PlayerId.NewId();
  ```
- これにより、ID生成の実装が一箇所に集約され、ドメイン層の責務分離・保守性・テスト容易性が向上する。

### 2. ValueObjectの責務
- 値の妥当性検証（null/空文字/範囲チェック等）はValueObjectのコンストラクタやファクトリで必ず行うこと。
- 生成・変換・パースなど、値に関するロジックはValueObject側に集約する。

### 3. エンティティ/集約ルートの責務
- IDや値オブジェクトの生成はValueObject側に委譲し、エンティティ/集約ルートは「集約の構成・状態遷移・ビジネスルール」に集中すること。

---

（例：GameId/PlayerIdのID生成責務分離、値検証の一元化など）

---

### 時間に関するルール

#### 1. ドメイン層での時間取得は禁止
- **❌ 禁止**: ドメイン層で`DateTime.UtcNow`を直接使用
- **✅ 推奨**: 時間をパラメータで受け取る

**❌ 禁止例**
```csharp
// ドメイン層で直接時間を取得
public void AddPlayer(Player player)
{
    // ... 処理 ...
    UpdatedAt = DateTime.UtcNow; // ❌ 禁止
}
```

**✅ 推奨例**
```csharp
// 時間をパラメータで受け取る
public void AddPlayer(Player player, DateTime now)
{
    // ... 処理 ...
    UpdatedAt = now; // ✅ 推奨
}
```

#### 2. 理由
- **テスト容易性**: 固定時間でのテストが可能
- **依存性分離**: ドメイン層がインフラ層に依存しない
- **DDD準拠**: ドメイン層は純粋なビジネスロジックに集中
- **決定論**: 同じ入力に対して同じ結果が保証される

#### 3. 実装パターン
- **Application層**: `DateTime.UtcNow`を取得してドメインに渡す
- **Domain層**: 時間をパラメータで受け取り、ビジネスロジックに使用
- **テスト**: 固定時間を使用してテストの再現性を確保

#### 4. 影響を受けるメソッド
- `Game.AddPlayer(Player player, DateTime now)`
- `Game.StartGame(DateTime now)`
- `Game.EndGame(DateTime now)`
- `Game.AddScoreHistory(ScoreHistory scoreHistory, DateTime now)`
- `Game.UpdatePlayerReadyStatus(PlayerId playerId, bool isReady, DateTime now)`

---

## ValueObjectの設計・実装ルール（責務分離・ID生成）

### 1. 識別子（ID）生成の責務はValueObject側に集約する
- GameIdやPlayerIdなど、エンティティの一意性を担保するIDの生成（例：GUID）は、ValueObject側のファクトリメソッド（例：NewId）で行うこと。
- エンティティや集約ルートのコンストラクタ・ファクトリでは、ID生成処理を直接書かず、必ずValueObjectの生成メソッドを呼び出すこと。
- 例：
  ```csharp
  var gameId = GameId.NewId();
  var playerId = PlayerId.NewId();
  ```
- これにより、ID生成の実装が一箇所に集約され、ドメイン層の責務分離・保守性・テスト容易性が向上する。

### 2. ValueObjectの責務
- 値の妥当性検証（null/空文字/範囲チェック等）はValueObjectのコンストラクタやファクトリで必ず行うこと。
- 生成・変換・パースなど、値に関するロジックはValueObject側に集約する。

### 3. エンティティ/集約ルートの責務
- IDや値オブジェクトの生成はValueObject側に委譲し、エンティティ/集約ルートは「集約の構成・状態遷移・ビジネスルール」に集中すること。

---

（例：GameId/PlayerIdのID生成責務分離、値検証の一元化など）

---

### エラーハンドリングのルール

#### 1. ドメインエラーはDomainErrorExceptionを使用する
- **❌ 禁止**: 汎用的な例外（ArgumentException、InvalidOperationException）を使用
- **✅ 推奨**: ドメイン固有のDomainErrorExceptionを使用

**❌ 禁止例**
```csharp
// 汎用的な例外を使用
if (timeLimit < 30)
    throw new ArgumentException("制限時間は30秒以上である必要があります", nameof(timeLimit));

if (Status != GameStatus.Waiting)
    throw new InvalidOperationException("ゲームは既に開始されています");
```

**✅ 推奨例**
```csharp
// ドメイン固有の例外を使用
if (timeLimit < 30)
    throw new DomainErrorException(DomainErrorCodes.GameSettings.InvalidTimeLimit, "制限時間は30秒以上である必要があります");

if (Status != GameStatus.Waiting)
    throw new DomainErrorException(DomainErrorCodes.Game.AlreadyStarted, "ゲームは既に開始されています");
```

#### 2. エラーコードは一箇所に定義する
- **❌ 禁止**: エラーコードを複数箇所に分散
- **✅ 推奨**: `DomainErrorCodes`クラスに一括定義

**✅ 推奨例**
```csharp
public static class DomainErrorCodes
{
    public static class Game
    {
        public const string NotFound = "GAME_NOT_FOUND";
        public const string AlreadyStarted = "GAME_ALREADY_STARTED";
        public const string InsufficientPlayers = "GAME_INSUFFICIENT_PLAYERS";
    }
    
    public static class GameSettings
    {
        public const string InvalidTimeLimit = "GAME_SETTINGS_INVALID_TIME_LIMIT";
        public const string InvalidPlayerCount = "GAME_SETTINGS_INVALID_PLAYER_COUNT";
    }
}
```

#### 3. テストではエラーコードを検証する
- **❌ 禁止**: 例外の型のみを検証
- **✅ 推奨**: エラーコードとメッセージを検証

**❌ 禁止例**
```csharp
// 例外の型のみを検証
var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
// エラーコードが意図したものか不明
```

**✅ 推奨例**
```csharp
// エラーコードとメッセージを検証
var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
Assert.Equal(DomainErrorCodes.Game.AlreadyStarted, exception.ErrorCode);
Assert.Equal("ゲームは既に開始されています", exception.ErrorMessage);
```

#### 4. エラーコードの命名規則
- **形式**: `{エンティティ}_{エラー種別}`
- **例**: `GAME_NOT_FOUND`, `PLAYER_INVALID_NAME`, `TURN_ALREADY_ENDED`

#### 5. 理由
- **テスト容易性**: 意図したエラーパスに入ったかどうかを正確に検証可能
- **保守性**: エラーコードによる明確な分類
- **国際化対応**: エラーコードとメッセージの分離
- **ログ分析**: エラーコードによる統計分析が可能

#### 6. 実装チェックリスト
- [ ] DomainErrorExceptionを使用しているか
- [ ] エラーコードがDomainErrorCodesに定義されているか
- [ ] テストでエラーコードを検証しているか
- [ ] エラーコードの命名が統一されているか
- [ ] エラーメッセージが適切か

