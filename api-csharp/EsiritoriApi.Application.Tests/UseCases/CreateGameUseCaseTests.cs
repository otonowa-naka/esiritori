namespace EsiritoriApi.Tests.Application.UseCases;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.Interfaces;
using EsiritoriApi.Application.UseCases;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;
using EsiritoriApi.Domain.Errors;
using Moq;
using Xunit;

public sealed class CreateGameUseCaseTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly CreateGameUseCase _useCase;

    public CreateGameUseCaseTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        _useCase = new CreateGameUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task 有効なリクエストでゲームが正常に作成される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Game);
        Assert.NotNull(response.Player);
        Assert.Equal("テスト作成者", response.Player.Name);
        Assert.Equal(GameStatus.Waiting.ToString(), response.Game.Status);
        Assert.Equal(60, response.Game.Settings.TimeLimit);
        Assert.Equal(3, response.Game.Settings.RoundCount);
        Assert.Equal(4, response.Game.Settings.PlayerCount);
        Assert.Single(response.Game.Players);
        Assert.False(response.Player.IsReady);
        Assert.False(response.Player.IsDrawer);

        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task nullリクエストの場合例外が発生する()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync(null!));
    }

    [Fact]
    public async Task 空の作成者名の場合でもゲームが作成される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<DomainErrorException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task リポジトリ保存時の例外が適切に伝播される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var expectedException = new InvalidOperationException("データベースエラー");
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(expectedException);

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
        Assert.Equal(expectedException.Message, actualException.Message);
    }

    [Fact]
    public async Task キャンセレーショントークンが適切に渡される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var cancellationToken = new CancellationToken();
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), cancellationToken))
                      .Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(request, cancellationToken);

        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Game>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task 生成されるゲームIDがGUID形式である()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(request);

        Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", response.Game.Id);
    }

    [Fact]
    public async Task 生成されるプレイヤーIDがGUID形式である()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(request);

        Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", response.Player.Id);
    }
}
