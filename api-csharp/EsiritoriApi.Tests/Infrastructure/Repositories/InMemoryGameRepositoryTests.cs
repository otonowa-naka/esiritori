namespace EsiritoriApi.Tests.Infrastructure.Repositories;

using EsiritoriApi.Domain.Entities;
using EsiritoriApi.Domain.ValueObjects;
using EsiritoriApi.Infrastructure.Repositories;
using Xunit;

public sealed class InMemoryGameRepositoryTests
{
    private static Game CreateTestGame(string gameId, string creatorName, string? creatorId = null)
    {
        var id = new GameId(gameId);
        var settings = new GameSettings(60, 3, 4);
        var playerId = creatorId ?? $"player_{gameId}";
        var creator = new Player(new PlayerId(playerId), new PlayerName(creatorName), PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        return new Game(id, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);
    }

    [Fact]
    public async Task ゲーム保存時_正常に保存される()
    {
        var repository = new InMemoryGameRepository();
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テスト作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);

        await repository.SaveAsync(game);

        var savedGame = await repository.FindByIdAsync(gameId);
        Assert.NotNull(savedGame);
        Assert.Equal(game.Id, savedGame.Id);
        Assert.Equal(1, repository.Count);
    }

    [Fact]
    public async Task ゲーム検索時_存在するゲームが正常に取得される()
    {
        var repository = new InMemoryGameRepository();
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テスト作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);
        await repository.SaveAsync(game);

        var foundGame = await repository.FindByIdAsync(gameId);

        Assert.NotNull(foundGame);
        Assert.Equal(gameId, foundGame.Id);
        Assert.Equal(game.Status, foundGame.Status);
    }

    [Fact]
    public async Task ゲーム検索時_存在しないゲームの場合nullが返される()
    {
        var repository = new InMemoryGameRepository();
        var nonExistentGameId = new GameId("999999");

        var foundGame = await repository.FindByIdAsync(nonExistentGameId);

        Assert.Null(foundGame);
    }

    [Fact]
    public async Task 全ゲーム取得時_保存されている全ゲームが取得される()
    {
        var repository = new InMemoryGameRepository();
        var game1 = CreateTestGame("123456", "プレイヤー1", "player1");
        var game2 = CreateTestGame("654321", "プレイヤー2", "player2");
        await repository.SaveAsync(game1);
        await repository.SaveAsync(game2);

        var allGames = await repository.FindAllAsync();

        Assert.Equal(2, allGames.Count());
        Assert.Contains(allGames, g => g.Id.Equals(game1.Id));
        Assert.Contains(allGames, g => g.Id.Equals(game2.Id));
    }

    [Fact]
    public async Task 全ゲーム取得時_ゲームが存在しない場合空のコレクションが返される()
    {
        var repository = new InMemoryGameRepository();

        var allGames = await repository.FindAllAsync();

        Assert.Empty(allGames);
    }

    [Fact]
    public async Task ゲーム削除時_正常に削除される()
    {
        var repository = new InMemoryGameRepository();
        var gameId = new GameId("123456");
        var game = CreateTestGame("123456", "テスト作成者", "playerX");
        await repository.SaveAsync(game);

        await repository.DeleteAsync(gameId);

        var deletedGame = await repository.FindByIdAsync(gameId);
        Assert.Null(deletedGame);
        Assert.Equal(0, repository.Count);
    }

    [Fact]
    public async Task ゲーム削除時_存在しないゲームの場合例外が発生しない()
    {
        var repository = new InMemoryGameRepository();
        var nonExistentGameId = new GameId("999999");

        await repository.DeleteAsync(nonExistentGameId); // 例外が発生しないことを確認
    }

    [Fact]
    public async Task 同じIDのゲーム保存時_既存のゲームが更新される()
    {
        var repository = new InMemoryGameRepository();
        var gameId = new GameId("123456");
        var originalGame = CreateTestGame("123456", "元の作成者", "playerA");
        await repository.SaveAsync(originalGame);

        var updatedGame = CreateTestGame("123456", "更新された作成者", "playerB");

        await repository.SaveAsync(updatedGame);

        var savedGame = await repository.FindByIdAsync(gameId);
        Assert.NotNull(savedGame);
        Assert.Equal("更新された作成者", savedGame.Players.First().Name.Value);
        Assert.Equal(1, repository.Count); // カウントは変わらない
    }

    [Fact]
    public async Task リポジトリクリア時_全てのゲームが削除される()
    {
        var repository = new InMemoryGameRepository();
        var game1 = CreateTestGame("123456", "プレイヤー1", "player1");
        var game2 = CreateTestGame("654321", "プレイヤー2", "player2");
        await repository.SaveAsync(game1);
        await repository.SaveAsync(game2);

        repository.Clear();

        Assert.Equal(0, repository.Count);
    }

    [Fact]
    public async Task nullゲーム保存時_例外が発生する()
    {
        var repository = new InMemoryGameRepository();

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.SaveAsync(null!));
    }

    [Fact]
    public async Task nullゲームID検索時_例外が発生する()
    {
        var repository = new InMemoryGameRepository();

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.FindByIdAsync(null!));
    }

    [Fact]
    public async Task nullゲームID削除時_例外が発生する()
    {
        var repository = new InMemoryGameRepository();

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.DeleteAsync(null!));
    }

    [Fact]
    public async Task ゲームにプレイヤー追加時_正常に追加される()
    {
        var now = DateTime.UtcNow;
        var repository = new InMemoryGameRepository();
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テスト作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);
        await repository.SaveAsync(game);

        var newPlayerName = "新しいプレイヤー";
        var newPlayerId = new PlayerId("newPlayer123");
        var newPlayer = new Player(newPlayerId, new PlayerName(newPlayerName), PlayerStatus.NotReady, false, false);

        game.AddPlayer(newPlayer, now);
        Assert.Equal(2, game.Players.Count);
        Assert.Equal("newPlayer123", game.Players.Last().Id.Value);
        Assert.Equal("新しいプレイヤー", game.Players.Last().Name.Value);
    }

    [Fact]
    public async Task ゲームに既存のプレイヤー追加時_既存のゲームが更新される()
    {
        var now = DateTime.UtcNow;
        var repository = new InMemoryGameRepository();
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テスト作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);
        await repository.SaveAsync(game);

        var newPlayerName = "新しいプレイヤー";
        var newPlayerId = new PlayerId("newPlayer123");
        var newPlayer = new Player(newPlayerId, new PlayerName(newPlayerName), PlayerStatus.NotReady, false, false);

        game.AddPlayer(newPlayer, now);
        Assert.Equal(2, game.Players.Count);
        Assert.Contains(newPlayerId.Value, game.Players.Select(p => p.Id.Value));
        Assert.Contains(newPlayerName, game.Players.Select(p => p.Name.Value));
    }

    [Fact]
    public async Task ゲームに既存のプレイヤー追加時_既存のゲームが更新される_既存のプレイヤーが既に存在する場合()
    {
        var now = DateTime.UtcNow;
        var repository = new InMemoryGameRepository();
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テスト作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);
        await repository.SaveAsync(game);

        var newPlayerName = "既存のプレイヤー";
        var newPlayerId = new PlayerId("existingPlayer123");
        var newPlayer = new Player(newPlayerId, new PlayerName(newPlayerName), PlayerStatus.NotReady, false, false);

        game.AddPlayer(newPlayer, now);
        Assert.Equal(2, game.Players.Count);
        Assert.Contains(game.Players, p => p.Id.Value == newPlayerId.Value);
        Assert.Contains(game.Players, p => p.Name.Value == newPlayerName);
    }

    [Fact]
    public async Task ゲームに既存のプレイヤー追加時_既存のゲームが更新される_既存のプレイヤーが既に存在する場合_既存のプレイヤーが既に存在する場合_既存のプレイヤーが既に存在する場合()
    {
        var now = DateTime.UtcNow;
        var repository = new InMemoryGameRepository();
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テスト作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);
        await repository.SaveAsync(game);

        var newPlayerName = "既存のプレイヤー";
        var newPlayerId = new PlayerId("existingPlayer123");
        var newPlayer = new Player(newPlayerId, new PlayerName(newPlayerName), PlayerStatus.NotReady, false, false);

        game.AddPlayer(newPlayer, now);
        Assert.Equal(2, game.Players.Count);
        Assert.Contains(game.Players, p => p.Id.Value == newPlayerId.Value);
        Assert.Contains(game.Players, p => p.Name.Value == newPlayerName);
    }
}