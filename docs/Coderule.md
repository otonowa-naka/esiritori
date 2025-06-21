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
        Assert.Equal("Waiting", result.Game.Status);
        Assert.Single(result.Game.Players);
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

