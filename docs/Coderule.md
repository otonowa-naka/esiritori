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
└── EsiritoriApi.Tests/          # テストプロジェクト
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

---

