namespace EsiritoriApi.Tests.Application.UseCases;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.Interfaces;
using EsiritoriApi.Application.UseCases;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.ValueObjects;
using Moq;
using Xunit;

public sealed class CreateGameUseCaseEdgeCaseTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly CreateGameUseCase _useCase;

    public CreateGameUseCaseEdgeCaseTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        _useCase = new CreateGameUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task 最小設定でゲームが正常に作成される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "プレイヤー",
            Settings = new GameSettingsDto
            {
                TimeLimit = 30,
                RoundCount = 1,
                PlayerCount = 2
            }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(request);

        Assert.NotNull(result);
        Assert.NotNull(result.Game.Id);
        Assert.Equal(30, result.Game.Settings.TimeLimit);
        Assert.Equal(1, result.Game.Settings.RoundCount);
        Assert.Equal(2, result.Game.Settings.PlayerCount);
        Assert.Single(result.Game.Players);
        Assert.Equal("プレイヤー", result.Game.Players.First().Name);
        Assert.Equal("Waiting", result.Game.Status);
        Assert.Equal("プレイヤー", result.Player.Name);
    }

    [Fact]
    public async Task 最大設定でゲームが正常に作成される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "プレイヤー最大",
            Settings = new GameSettingsDto
            {
                TimeLimit = 300,
                RoundCount = 10,
                PlayerCount = 8
            }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(request);

        Assert.NotNull(result);
        Assert.Equal(300, result.Game.Settings.TimeLimit);
        Assert.Equal(10, result.Game.Settings.RoundCount);
        Assert.Equal(8, result.Game.Settings.PlayerCount);
        Assert.Single(result.Game.Players);
        Assert.Equal("プレイヤー最大", result.Game.Players.First().Name);
    }

    [Fact]
    public async Task リポジトリ保存が呼び出される()
    {
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

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(request);

        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task リポジトリ例外が適切に伝播される()
    {
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

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("データベースエラー"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Equal("データベースエラー", exception.Message);
    }

    [Fact]
    public async Task 作成されたゲームのプレイヤーが準備完了状態でない()
    {
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

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(request);

        Assert.Single(result.Game.Players);
        Assert.False(result.Game.Players.First().IsReady);
        Assert.False(result.Game.Players.First().IsDrawer);
        Assert.False(result.Player.IsReady);
        Assert.False(result.Player.IsDrawer);
    }

    [Fact]
    public async Task 作成されたゲームIDが一意である()
    {
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

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var result1 = await _useCase.ExecuteAsync(request);
        var result2 = await _useCase.ExecuteAsync(request);

        Assert.NotEqual(result1.Game.Id, result2.Game.Id);
    }
}
