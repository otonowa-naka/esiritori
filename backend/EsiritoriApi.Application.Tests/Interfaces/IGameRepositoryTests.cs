namespace EsiritoriApi.Application.Tests.Interfaces;


using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;
using Moq;
using Xunit;

[Trait("Category", "ユースケース")]
public sealed class IGameRepositoryTests
{
    [Fact]
    public void IGameRepository_SaveAsyncメソッドが定義されている()
    {
        var mockRepository = new Mock<IGameRepository>();
        var game = CreateTestGame();

        var task = mockRepository.Object.SaveAsync(game);

        Assert.NotNull(task);
        mockRepository.Verify(r => r.SaveAsync(game, default), Times.Once);
    }

    [Fact]
    public void IGameRepository_FindByIdAsyncメソッドが定義されている()
    {
        var mockRepository = new Mock<IGameRepository>();
        var gameId = new GameId("123456");
        var game = CreateTestGame();
        mockRepository.Setup(r => r.FindByIdAsync(gameId, default)).ReturnsAsync(game);

        var task = mockRepository.Object.FindByIdAsync(gameId);

        Assert.NotNull(task);
        mockRepository.Verify(r => r.FindByIdAsync(gameId, default), Times.Once);
    }

    [Fact]
    public void IGameRepository_FindAllAsyncメソッドが定義されている()
    {
        var mockRepository = new Mock<IGameRepository>();
        var games = new List<Game> { CreateTestGame() };
        mockRepository.Setup(r => r.FindAllAsync(default)).ReturnsAsync(games);

        var task = mockRepository.Object.FindAllAsync();

        Assert.NotNull(task);
        mockRepository.Verify(r => r.FindAllAsync(default), Times.Once);
    }

    [Fact]
    public void IGameRepository_DeleteAsyncメソッドが定義されている()
    {
        var mockRepository = new Mock<IGameRepository>();
        var gameId = new GameId("123456");

        var task = mockRepository.Object.DeleteAsync(gameId);

        Assert.NotNull(task);
        mockRepository.Verify(r => r.DeleteAsync(gameId, default), Times.Once);
    }

    [Fact]
    public async Task IGameRepository_CancellationTokenサポート確認()
    {
        var mockRepository = new Mock<IGameRepository>();
        var game = CreateTestGame();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        await mockRepository.Object.SaveAsync(game, cancellationToken);
        await mockRepository.Object.FindByIdAsync(game.Id, cancellationToken);
        await mockRepository.Object.FindAllAsync(cancellationToken);
        await mockRepository.Object.DeleteAsync(game.Id, cancellationToken);

        mockRepository.Verify(r => r.SaveAsync(game, cancellationToken), Times.Once);
        mockRepository.Verify(r => r.FindByIdAsync(game.Id, cancellationToken), Times.Once);
        mockRepository.Verify(r => r.FindAllAsync(cancellationToken), Times.Once);
        mockRepository.Verify(r => r.DeleteAsync(game.Id, cancellationToken), Times.Once);
    }

    private static Game CreateTestGame()
    {
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var playerId = new PlayerId("player1");
        var playerName = new PlayerName("テストプレイヤー");
        var creator = new Player(playerId, playerName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        return new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);
    }
}