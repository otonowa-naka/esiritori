namespace EsiritoriApi.Tests.Domain.Entities;

using EsiritoriApi.Domain.Entities;
using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class GameTests
{
    [Fact]
    public void ゲーム作成時_正常な値でゲームが作成される()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テストプレイヤー");
        var creatorId = new PlayerId("player123");

        var game = new Game(gameId, settings, creatorName, creatorId);

        Assert.Equal(gameId, game.Id);
        Assert.Equal(GameStatus.Waiting, game.Status);
        Assert.Equal(settings, game.Settings);
        Assert.Single(game.Players);
        Assert.Equal(creatorName, game.Players.First().Name);
        Assert.Equal(creatorId, game.Players.First().Id);
        Assert.False(game.Players.First().IsReady);
        Assert.False(game.Players.First().IsDrawer);
    }

    [Fact]
    public void プレイヤー追加時_正常にプレイヤーが追加される()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var newPlayerId = new PlayerId("player456");
        var newPlayerName = new PlayerName("新しいプレイヤー");

        var updatedGame = game.AddPlayer(newPlayerId, newPlayerName);

        Assert.Equal(2, updatedGame.Players.Count);
        Assert.Contains(updatedGame.Players, p => p.Id.Equals(newPlayerId));
        Assert.Contains(updatedGame.Players, p => p.Name.Equals(newPlayerName));
    }

    [Fact]
    public void プレイヤー追加時_ゲームが満員の場合例外が発生する()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 2); // 最大2人
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var player1Id = new PlayerId("player1");
        var player1Name = new PlayerName("プレイヤー1");
        var gameWith2Players = game.AddPlayer(player1Id, player1Name);

        var player2Id = new PlayerId("player2");
        var player2Name = new PlayerName("プレイヤー2");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            gameWith2Players.AddPlayer(player2Id, player2Name));
        Assert.Equal("ゲームが満員です", exception.Message);
    }

    [Fact]
    public void プレイヤー追加時_既に参加しているプレイヤーの場合例外が発生する()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            game.AddPlayer(creatorId, creatorName));
        Assert.Equal("このプレイヤーは既に参加しています", exception.Message);
    }

    [Fact]
    public void プレイヤー追加時_ゲームが開始済みの場合例外が発生する()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var player1Id = new PlayerId("player1");
        var player1Name = new PlayerName("プレイヤー1");
        var gameWithPlayer = game.AddPlayer(player1Id, player1Name)
            .UpdatePlayerReadyStatus(creatorId, true)
            .UpdatePlayerReadyStatus(player1Id, true);

        var startedGame = gameWithPlayer.StartGame();

        var newPlayerId = new PlayerId("newplayer");
        var newPlayerName = new PlayerName("新プレイヤー");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            startedGame.AddPlayer(newPlayerId, newPlayerName));
        Assert.Equal("ゲームが開始されているため、プレイヤーを追加できません", exception.Message);
    }

    [Fact]
    public void ゲーム開始時_正常にゲームが開始される()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var player1Id = new PlayerId("player1");
        var player1Name = new PlayerName("プレイヤー1");
        var gameWithPlayers = game.AddPlayer(player1Id, player1Name)
            .UpdatePlayerReadyStatus(creatorId, true)
            .UpdatePlayerReadyStatus(player1Id, true);

        var startedGame = gameWithPlayers.StartGame();

        Assert.Equal(GameStatus.Playing, startedGame.Status);
        Assert.Contains(startedGame.Players, p => p.IsDrawer);
        Assert.Single(startedGame.Players.Where(p => p.IsDrawer));
    }

    [Fact]
    public void ゲーム開始時_プレイヤーが2人未満の場合例外が発生する()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId)
            .UpdatePlayerReadyStatus(creatorId, true);

        var exception = Assert.Throws<InvalidOperationException>(() => game.StartGame());
        Assert.Equal("ゲームを開始するには最低2人のプレイヤーが必要です", exception.Message);
    }

    [Fact]
    public void ゲーム開始時_全プレイヤーが準備完了でない場合例外が発生する()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var player1Id = new PlayerId("player1");
        var player1Name = new PlayerName("プレイヤー1");
        var gameWithPlayers = game.AddPlayer(player1Id, player1Name)
            .UpdatePlayerReadyStatus(creatorId, true);

        var exception = Assert.Throws<InvalidOperationException>(() => gameWithPlayers.StartGame());
        Assert.Equal("全てのプレイヤーが準備完了状態である必要があります", exception.Message);
    }

    [Fact]
    public void ゲーム終了時_正常にゲームが終了される()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var player1Id = new PlayerId("player1");
        var player1Name = new PlayerName("プレイヤー1");
        var startedGame = game.AddPlayer(player1Id, player1Name)
            .UpdatePlayerReadyStatus(creatorId, true)
            .UpdatePlayerReadyStatus(player1Id, true)
            .StartGame();

        var endedGame = startedGame.EndGame();

        Assert.Equal(GameStatus.Finished, endedGame.Status);
    }

    [Fact]
    public void プレイヤー準備状態更新時_正常に更新される()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var updatedGame = game.UpdatePlayerReadyStatus(creatorId, true);

        var creator = updatedGame.Players.First(p => p.Id.Equals(creatorId));
        Assert.True(creator.IsReady);
    }

    [Fact]
    public void プレイヤー準備状態更新時_存在しないプレイヤーの場合例外が発生する()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var nonExistentPlayerId = new PlayerId("nonexistent");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            game.UpdatePlayerReadyStatus(nonExistentPlayerId, true));
        Assert.Equal("プレイヤーが見つかりません", exception.Message);
    }

    [Fact]
    public void スコア履歴追加時_正常に追加される()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var game = new Game(gameId, settings, creatorName, creatorId);

        var scoreHistory = new ScoreHistory(
            creatorId,
            1,
            1,
            100,
            ScoreReason.CorrectAnswer,
            DateTime.UtcNow
        );

        var updatedGame = game.AddScoreHistory(scoreHistory);

        Assert.Single(updatedGame.ScoreHistories);
        Assert.Contains(scoreHistory, updatedGame.ScoreHistories);
    }
}
