namespace EsiritoriApi.Tests.Domain.Entities;

using EsiritoriApi.Domain.Entities;
using EsiritoriApi.Domain.Errors;
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
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);

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
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var newPlayerId = new PlayerId("player456");
        var newPlayerName = new PlayerName("新しいプレイヤー");
        var newPlayer = new Player(newPlayerId, newPlayerName, PlayerStatus.NotReady, false, false);

        game.AddPlayer(newPlayer, now);
        Assert.Equal(2, game.Players.Count);
        Assert.Contains(game.Players, p => p.Id.Equals(newPlayerId));
        Assert.Contains(game.Players, p => p.Name.Equals(newPlayerName));
    }

    [Fact]
    public void プレイヤー追加時_ゲームが満員の場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 3); // 最大3人
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = new PlayerId("game_test_player1_max");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);

        var player2Id = new PlayerId("game_test_player2_max");
        var player2Name = new PlayerName("プレイヤー2");
        var player2 = new Player(player2Id, player2Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player2, now);

        // 3人目を追加しようとすると満員で例外発生
        var player3Id = new PlayerId("game_test_player3_max");
        var player3Name = new PlayerName("プレイヤー3");
        var player3 = new Player(player3Id, player3Name, PlayerStatus.NotReady, false, false);

        var exception = Assert.Throws<DomainErrorException>(() => game.AddPlayer(player3, now));
        Assert.Equal(DomainErrorCodes.Game.PlayerLimitExceeded, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤー追加時_既に参加しているプレイヤーの場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = new PlayerId("game_test_player1_duplicate");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        var exception = Assert.Throws<DomainErrorException>(() => game.AddPlayer(player1, now));
        Assert.Equal(DomainErrorCodes.Game.PlayerAlreadyJoined, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤー追加時_ゲームが開始済みの場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = new PlayerId("game_test_player1_started");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);
        game.StartGame(now);

        var newPlayerId = new PlayerId("game_test_newplayer_started");
        var newPlayerName = new PlayerName("新プレイヤー");
        var newPlayer = new Player(newPlayerId, newPlayerName, PlayerStatus.NotReady, false, false);

        var exception = Assert.Throws<DomainErrorException>(() => game.AddPlayer(newPlayer, now));
        Assert.Equal(DomainErrorCodes.Game.CannotAddPlayerAfterStart, exception.ErrorCode);
    }

    [Fact]
    public void ゲーム開始時_正常にゲームが開始される()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = new PlayerId("game_test_player1_start");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);
        
        // StartGame前のRoundを保存
        var originalRound = game.CurrentRound;
        
        game.StartGame(now);

        Assert.Equal(GameStatus.Playing, game.Status);
        Assert.Contains(game.Players, p => p.IsDrawer);
        Assert.Single(game.Players.Where(p => p.IsDrawer));
        
        // 新規Roundが作成されていることを確認
        Assert.Equal(1, game.CurrentRound.RoundNumber);
        Assert.Equal(1, game.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal(TurnStatus.SettingAnswer, game.CurrentRound.CurrentTurn.Status);
        Assert.Equal(creatorId, game.CurrentRound.CurrentTurn.DrawerId);
        Assert.Equal(settings.TimeLimit, game.CurrentRound.CurrentTurn.TimeLimit);
    }

    [Fact]
    public void ゲーム開始時_プレイヤーが2人未満の場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
        Assert.Equal(DomainErrorCodes.Game.InsufficientPlayers, exception.ErrorCode);
    }

    [Fact]
    public void ゲーム開始時_全プレイヤーが準備完了でない場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = new PlayerId("game_test_player1_notready");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        // player1は準備完了状態にしない

        var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
        Assert.Equal(DomainErrorCodes.Game.NotAllPlayersReady, exception.ErrorCode);
    }

    [Fact]
    public void ゲーム開始時_既に開始されている場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Playing, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
        Assert.Equal(DomainErrorCodes.Game.AlreadyStarted, exception.ErrorCode);
    }

    [Fact]
    public void ゲーム終了時_正常にゲームが終了される()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Playing, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        game.EndGame(now);

        Assert.Equal(GameStatus.Finished, game.Status);
    }

    [Fact]
    public void ゲーム終了時_既に終了している場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Finished, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var exception = Assert.Throws<DomainErrorException>(() => game.EndGame(now));
        Assert.Equal(DomainErrorCodes.Game.AlreadyEnded, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤー準備状態更新時_正常に更新される()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        game.UpdatePlayerReadyStatus(creatorId, true, now);

        var updatedPlayer = game.Players.First(p => p.Id.Equals(creatorId));
        Assert.Equal(PlayerStatus.Ready, updatedPlayer.Status);
    }

    [Fact]
    public void プレイヤー準備状態更新時_存在しないプレイヤーの場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var nonExistentPlayerId = new PlayerId("nonexistent");

        var exception = Assert.Throws<DomainErrorException>(() => game.UpdatePlayerReadyStatus(nonExistentPlayerId, true, now));
        Assert.Equal(DomainErrorCodes.Game.PlayerNotFound, exception.ErrorCode);
    }

    [Fact]
    public void スコア履歴追加時_正常に追加される()
    {
        var now = DateTime.UtcNow;
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Playing, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var scoreHistory = new ScoreHistory(creator.Id, 1, 1, 10, ScoreReason.CorrectAnswer, now);

        game.AddScoreHistory(scoreHistory, now);

        Assert.Single(game.ScoreHistories);
        Assert.Contains(scoreHistory, game.ScoreHistories);
    }
}
