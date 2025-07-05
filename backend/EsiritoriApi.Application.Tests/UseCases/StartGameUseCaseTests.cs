namespace EsiritoriApi.Tests.Application.UseCases;

using EsiritoriApi.Application.DTOs;

using EsiritoriApi.Application.UseCases;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Errors;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;
using Moq;
using Xunit;

public sealed class StartGameUseCaseTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly StartGameUseCase _useCase;

    public StartGameUseCaseTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        _useCase = new StartGameUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task 有効なゲームIDでゲームが正常に開始される()
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

        var player1Id = new PlayerId("startgame_test_player1_valid");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);

        var request = new StartGameRequest { GameId = "123456" };

        _mockRepository.Setup(r => r.FindByIdAsync(It.IsAny<GameId>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(game);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var response = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Game);
        Assert.Equal("Playing", response.Game.Status);
        Assert.Equal(1, response.Game.CurrentRound.RoundNumber);
        Assert.Equal(1, response.Game.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal("SettingAnswer", response.Game.CurrentRound.CurrentTurn.Status);
        Assert.Equal(creatorId.Value, response.Game.CurrentRound.CurrentTurn.DrawerId);

        _mockRepository.Verify(r => r.FindByIdAsync(It.IsAny<GameId>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task nullリクエストの場合例外が発生する()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync(null!));
    }

    [Fact]
    public async Task 存在しないゲームIDの場合例外が発生する()
    {
        var request = new StartGameRequest { GameId = "nonexistent" };

        _mockRepository.Setup(r => r.FindByIdAsync(It.IsAny<GameId>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Game?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
        Assert.Equal("指定されたゲームが見つかりません", exception.Message);
    }

    [Fact]
    public async Task 既に開始されているゲームの場合例外が発生する()
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

        var request = new StartGameRequest { GameId = "123456" };

        _mockRepository.Setup(r => r.FindByIdAsync(It.IsAny<GameId>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(game);

        var exception = await Assert.ThrowsAsync<DomainErrorException>(() => _useCase.ExecuteAsync(request));
        Assert.Equal("ゲームは既に開始されています", exception.Message);
    }

    [Fact]
    public async Task プレイヤーが2人未満の場合例外が発生する()
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

        var request = new StartGameRequest { GameId = "123456" };

        _mockRepository.Setup(r => r.FindByIdAsync(It.IsAny<GameId>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(game);

        var exception = await Assert.ThrowsAsync<DomainErrorException>(() => _useCase.ExecuteAsync(request));
        Assert.Equal("ゲームを開始するには最低2人のプレイヤーが必要です", exception.Message);
    }

    [Fact]
    public async Task 全プレイヤーが準備完了でない場合例外が発生する()
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

        var player1Id = new PlayerId("startgame_test_player1_notready");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        // player1は準備完了状態にしない

        var request = new StartGameRequest { GameId = "123456" };

        _mockRepository.Setup(r => r.FindByIdAsync(It.IsAny<GameId>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(game);

        var exception = await Assert.ThrowsAsync<DomainErrorException>(() => _useCase.ExecuteAsync(request));
        Assert.Equal("全てのプレイヤーが準備完了状態である必要があります", exception.Message);
    }

    [Fact]
    public async Task リポジトリ保存時の例外が適切に伝播される()
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

        var player1Id = new PlayerId("startgame_test_player1_save_error");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);

        var request = new StartGameRequest { GameId = "123456" };

        _mockRepository.Setup(r => r.FindByIdAsync(It.IsAny<GameId>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(game);
        var expectedException = new InvalidOperationException("データベースエラー");
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(expectedException);

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
        Assert.Equal(expectedException.Message, actualException.Message);
    }

    [Fact]
    public async Task キャンセレーショントークンが適切に渡される()
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

        var player1Id = new PlayerId("startgame_test_player1_cancellation");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);

        var request = new StartGameRequest { GameId = "123456" };
        var cancellationToken = new CancellationToken();

        _mockRepository.Setup(r => r.FindByIdAsync(It.IsAny<GameId>(), cancellationToken))
                      .ReturnsAsync(game);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), cancellationToken))
                      .Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(request, cancellationToken);

        _mockRepository.Verify(r => r.FindByIdAsync(It.IsAny<GameId>(), cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Game>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ゲーム開始後に新規Roundが作成される()
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

        var player1Id = new PlayerId("startgame_test_player1_new_round");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);

        var request = new StartGameRequest { GameId = "123456" };

        _mockRepository.Setup(r => r.FindByIdAsync(It.IsAny<GameId>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(game);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Game);
        Assert.Equal("Playing", response.Game.Status);
        Assert.Equal(1, response.Game.CurrentRound.RoundNumber);
        Assert.Equal(1, response.Game.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal("SettingAnswer", response.Game.CurrentRound.CurrentTurn.Status);
        Assert.Equal(creatorId.Value, response.Game.CurrentRound.CurrentTurn.DrawerId);
    }

    [Fact]
    public void コンストラクタ_nullリポジトリで例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() => new StartGameUseCase(null!));
    }
} 