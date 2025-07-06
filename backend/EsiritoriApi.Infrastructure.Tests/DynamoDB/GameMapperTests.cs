namespace EsiritoriApi.Tests.Infrastructure.DynamoDB;

using Amazon.DynamoDBv2.DocumentModel;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;
using EsiritoriApi.Domain.Shared.ValueObjects;
using EsiritoriApi.Infrastructure.DynamoDB;
using Xunit;

[Trait("Category", "インフラストラクチャテスト")]
public sealed class GameMapperTests
{
    private static Game CreateTestGame()
    {
        var gameId = new GameId("test-game-123");
        var settings = new GameSettings(60, 3, 4);
        var creatorId = new PlayerId("creator-456");
        var creatorName = new PlayerName("テスト作成者");
        var creator = new Player(creatorId, creatorName, PlayerStatus.Ready, true, false);
        
        var secondPlayerId = new PlayerId("player-789");
        var secondPlayerName = new PlayerName("二番目のプレイヤー");
        var secondPlayer = new Player(secondPlayerId, secondPlayerName, PlayerStatus.NotReady, false, true);
        
        var players = new[] { creator, secondPlayer };
        
        var answer = new Answer("てすと");
        var initialTurn = new Turn(
            1, 
            creatorId, 
            Option<Answer>.Some(answer), 
            TurnStatus.Drawing, 
            60,
            DateTime.UtcNow.AddMinutes(-5),
            Option<DateTime>.Some(DateTime.UtcNow),
            new[] { secondPlayerId }
        );
        
        var initialRound = new Round(1, initialTurn, DateTime.UtcNow.AddMinutes(-10), Option<DateTime>.None());
        
        var scoreHistories = new[]
        {
            new ScoreHistory(secondPlayerId, 1, 1, 10, ScoreReason.CorrectAnswer, DateTime.UtcNow.AddMinutes(-2)),
            new ScoreHistory(creatorId, 1, 1, -5, ScoreReason.DrawerPenalty, DateTime.UtcNow.AddMinutes(-1))
        };
        
        return new Game(
            gameId, 
            settings, 
            GameStatus.Playing, 
            initialRound, 
            players, 
            scoreHistories, 
            DateTime.UtcNow.AddHours(-1), 
            DateTime.UtcNow.AddMinutes(-3)
        );
    }

    [Fact]
    public void ToDocument変換時_Game情報が正しくDynamoDBDocumentに変換される()
    {
        var game = CreateTestGame();

        var document = GameMapper.ToDocument(game);

        Assert.NotNull(document);
        Assert.Equal("GAME#test-game-123", document["PK"].AsString());
        Assert.Equal("META", document["SK"].AsString());
        Assert.Equal("test-game-123", document["gameId"].AsString());
        Assert.Equal("playing", document["status"].AsString());
        
        // ゲーム設定の確認
        var settingsDoc = document["settings"].AsDocument();
        Assert.Equal(60, settingsDoc["timeLimit"].AsInt());
        Assert.Equal(3, settingsDoc["roundCount"].AsInt());
        Assert.Equal(4, settingsDoc["playerCount"].AsInt());
        
        // プレイヤー情報の確認
        var playersList = document["players"].AsDynamoDBList();
        Assert.Equal(2, playersList.Entries.Count);
        
        var firstPlayer = playersList.Entries[0].AsDocument();
        Assert.Equal("creator-456", firstPlayer["id"].AsString());
        Assert.Equal("テスト作成者", firstPlayer["name"].AsString());
        Assert.Equal("ready", firstPlayer["status"].AsString());
        Assert.True(firstPlayer["isReady"].AsBoolean());
        Assert.False(firstPlayer["isDrawer"].AsBoolean());
        
        // スコア履歴の確認
        var scoreHistoriesList = document["scoreHistories"].AsDynamoDBList();
        Assert.Equal(2, scoreHistoriesList.Entries.Count);
        
        var firstScore = scoreHistoriesList.Entries[0].AsDocument();
        Assert.Equal("player-789", firstScore["playerId"].AsString());
        Assert.Equal(1, firstScore["roundNumber"].AsInt());
        Assert.Equal(1, firstScore["turnNumber"].AsInt());
        Assert.Equal(10, firstScore["points"].AsInt());
        Assert.Equal("correct_answer", firstScore["reason"].AsString());
        
        // GSI用フィールドの確認
        Assert.Equal("STATUS#playing", document["GSI1PK"].AsString());
        Assert.StartsWith("CREATED#", document["GSI1SK"].AsString());
    }

    [Fact]
    public void ToEntity変換時_DynamoDBDocumentからGame情報が正しく復元される()
    {
        var originalGame = CreateTestGame();
        var document = GameMapper.ToDocument(originalGame);

        var restoredGame = GameMapper.ToEntity(document);

        Assert.NotNull(restoredGame);
        Assert.Equal(originalGame.Id.Value, restoredGame.Id.Value);
        Assert.Equal(originalGame.Status, restoredGame.Status);
        
        // ゲーム設定の確認
        Assert.Equal(originalGame.Settings.TimeLimit, restoredGame.Settings.TimeLimit);
        Assert.Equal(originalGame.Settings.RoundCount, restoredGame.Settings.RoundCount);
        Assert.Equal(originalGame.Settings.PlayerCount, restoredGame.Settings.PlayerCount);
        
        // プレイヤー情報の確認
        Assert.Equal(originalGame.Players.Count, restoredGame.Players.Count);
        var originalCreator = originalGame.Players.First();
        var restoredCreator = restoredGame.Players.First(p => p.Id.Value == originalCreator.Id.Value);
        Assert.Equal(originalCreator.Name.Value, restoredCreator.Name.Value);
        Assert.Equal(originalCreator.Status, restoredCreator.Status);
        Assert.Equal(originalCreator.IsReady, restoredCreator.IsReady);
        Assert.Equal(originalCreator.IsDrawer, restoredCreator.IsDrawer);
        
        // ラウンド・ターン情報の確認
        Assert.Equal(originalGame.CurrentRound.RoundNumber, restoredGame.CurrentRound.RoundNumber);
        Assert.Equal(originalGame.CurrentRound.CurrentTurn.TurnNumber, restoredGame.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal(originalGame.CurrentRound.CurrentTurn.DrawerId.Value, restoredGame.CurrentRound.CurrentTurn.DrawerId.Value);
        Assert.Equal(originalGame.CurrentRound.CurrentTurn.Status, restoredGame.CurrentRound.CurrentTurn.Status);
        Assert.Equal(originalGame.CurrentRound.CurrentTurn.TimeLimit, restoredGame.CurrentRound.CurrentTurn.TimeLimit);
        
        // お題の確認
        Assert.True(restoredGame.CurrentRound.CurrentTurn.Answer.HasValue);
        Assert.Equal("てすと", restoredGame.CurrentRound.CurrentTurn.Answer.Value.Value);
        
        // 正解者の確認
        Assert.Single(restoredGame.CurrentRound.CurrentTurn.CorrectPlayerIds);
        Assert.Equal("player-789", restoredGame.CurrentRound.CurrentTurn.CorrectPlayerIds.First().Value);
        
        // スコア履歴の確認
        Assert.Equal(originalGame.ScoreHistories.Count, restoredGame.ScoreHistories.Count);
        var originalFirstScore = originalGame.ScoreHistories.First();
        var restoredFirstScore = restoredGame.ScoreHistories.First(s => s.PlayerId.Value == originalFirstScore.PlayerId.Value);
        Assert.Equal(originalFirstScore.Points, restoredFirstScore.Points);
        Assert.Equal(originalFirstScore.Reason, restoredFirstScore.Reason);
    }

    [Fact]
    public void ToDocument変換時_お題が設定されていない場合空文字列が設定される()
    {
        var gameId = new GameId("test-game-no-answer");
        var settings = new GameSettings(60, 3, 4);
        var creatorId = new PlayerId("creator-456");
        var creatorName = new PlayerName("テスト作成者");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        
        var initialTurn = Turn.CreateInitial(creatorId, 60, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);

        var document = GameMapper.ToDocument(game);

        var currentRoundDoc = document["currentRound"].AsDocument();
        var currentTurnDoc = currentRoundDoc["currentTurn"].AsDocument();
        Assert.Equal("", currentTurnDoc["answer"].AsString());
    }

    [Fact]
    public void ToEntity変換時_お題が空文字列の場合Noneが設定される()
    {
        var gameId = new GameId("test-game-empty-answer");
        var settings = new GameSettings(60, 3, 4);
        var creatorId = new PlayerId("creator-456");
        var creatorName = new PlayerName("テスト作成者");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        
        var initialTurn = Turn.CreateInitial(creatorId, 60, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var originalGame = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);
        
        var document = GameMapper.ToDocument(originalGame);

        var restoredGame = GameMapper.ToEntity(document);

        Assert.False(restoredGame.CurrentRound.CurrentTurn.Answer.HasValue);
    }

    [Fact]
    public void GetPartitionKey生成時_正しい形式で生成される()
    {
        var gameId = new GameId("test-game-123");

        var partitionKey = GameMapper.GetPartitionKey(gameId);

        Assert.Equal("GAME#test-game-123", partitionKey);
    }

    [Fact]
    public void ExtractGameId抽出時_パーティションキーから正しくGameIdが抽出される()
    {
        var partitionKey = "GAME#test-game-456";

        var gameId = GameMapper.ExtractGameId(partitionKey);

        Assert.Equal("test-game-456", gameId.Value);
    }

    [Fact]
    public void ExtractGameId抽出時_不正なパーティションキーの場合例外が発生する()
    {
        var invalidPartitionKey = "INVALID#test-game-456";

        var exception = Assert.Throws<ArgumentException>(() => GameMapper.ExtractGameId(invalidPartitionKey));
        Assert.Contains("Invalid partition key format", exception.Message);
    }

    [Fact]
    public void ToDocument変換時_終了したゲームの場合TTLが設定される()
    {
        var game = CreateTestGame();
        // ゲームを終了状態にする
        game.EndGame(DateTime.UtcNow);

        var document = GameMapper.ToDocument(game);

        Assert.True(document.ContainsKey("ttl"));
        Assert.True(document["ttl"].AsLong() > 0);
    }

    [Fact]
    public void ToDocument変換時_待機中ゲームの場合TTLが設定されない()
    {
        var gameId = new GameId("test-waiting-game");
        var settings = new GameSettings(60, 3, 4);
        var creatorId = new PlayerId("creator-456");
        var creatorName = new PlayerName("テスト作成者");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        
        var initialTurn = Turn.CreateInitial(creatorId, 60, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);

        var document = GameMapper.ToDocument(game);

        Assert.False(document.ContainsKey("ttl"));
    }

    [Fact]
    public void ToDocument変換時_nullゲームの場合例外が発生する()
    {
        Game? nullGame = null;

        var exception = Assert.Throws<ArgumentNullException>(() => GameMapper.ToDocument(nullGame!));
        Assert.Equal("game", exception.ParamName);
    }

    [Fact]
    public void ToEntity変換時_nullDocumentの場合例外が発生する()
    {
        Document? nullDocument = null;

        var exception = Assert.Throws<ArgumentNullException>(() => GameMapper.ToEntity(nullDocument!));
        Assert.Equal("document", exception.ParamName);
    }
}