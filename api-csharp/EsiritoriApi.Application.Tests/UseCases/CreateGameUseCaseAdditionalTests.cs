namespace EsiritoriApi.Application.Tests.UseCases;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.Interfaces;
using EsiritoriApi.Application.UseCases;
using EsiritoriApi.Domain.Entities;
using Moq;
using Xunit;

[Trait("Category", "ユースケース")]
public sealed class CreateGameUseCaseAdditionalTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly CreateGameUseCase _useCase;

    public CreateGameUseCaseAdditionalTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        _useCase = new CreateGameUseCase(_mockRepository.Object);
    }

    [Fact]
    public void コンストラクタ_nullリポジトリで例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() => new CreateGameUseCase(null!));
    }

    [Fact]
    public async Task キャンセレーショントークンが適切に渡される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テストプレイヤー",
            Settings = new GameSettingsDto { TimeLimit = 60, RoundCount = 3, PlayerCount = 4 }
        };
        var cancellationToken = new CancellationToken();

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), cancellationToken))
                      .Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(request, cancellationToken);

        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Game>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task リポジトリ保存時の例外が適切に伝播される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テストプレイヤー",
            Settings = new GameSettingsDto { TimeLimit = 60, RoundCount = 3, PlayerCount = 4 }
        };
        var expectedException = new InvalidOperationException("データベースエラー");

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(expectedException);

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
        Assert.Equal(expectedException.Message, actualException.Message);
    }

    [Theory]
    [InlineData(30, 2, 2)]
    [InlineData(120, 5, 8)]
    [InlineData(90, 4, 6)]
    public async Task 様々なゲーム設定でのゲーム作成(int timeLimit, int roundCount, int playerCount)
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テストプレイヤー",
            Settings = new GameSettingsDto { TimeLimit = timeLimit, RoundCount = roundCount, PlayerCount = playerCount }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(timeLimit, response.Game.Settings.TimeLimit);
        Assert.Equal(roundCount, response.Game.Settings.RoundCount);
        Assert.Equal(playerCount, response.Game.Settings.PlayerCount);
    }

    [Theory]
    [InlineData("日本語プレイヤー")]
    [InlineData("Player123")]
    [InlineData("プレイヤー_あいうえお")]
    [InlineData("English Player")]
    [InlineData("🎮ゲーマー🎯")]
    public async Task 様々なプレイヤー名でのゲーム作成(string creatorName)
    {
        var request = new CreateGameRequest
        {
            CreatorName = creatorName,
            Settings = new GameSettingsDto { TimeLimit = 60, RoundCount = 3, PlayerCount = 4 }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.Equal(creatorName, response.Player.Name);
        Assert.Equal(creatorName, response.Game.Players.First().Name);
    }

    [Fact]
    public async Task レスポンスマッピングの完全性検証()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "完全テストプレイヤー",
            Settings = new GameSettingsDto { TimeLimit = 75, RoundCount = 7, PlayerCount = 5 }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var response = await _useCase.ExecuteAsync(request);

        // Game情報の検証
        Assert.NotNull(response.Game);
        Assert.NotEmpty(response.Game.Id);
        Assert.Equal("Waiting", response.Game.Status);
        
        // Settings情報の検証
        Assert.NotNull(response.Game.Settings);
        Assert.Equal(75, response.Game.Settings.TimeLimit);
        Assert.Equal(7, response.Game.Settings.RoundCount);
        Assert.Equal(5, response.Game.Settings.PlayerCount);
        
        // CurrentRound情報の検証
        Assert.NotNull(response.Game.CurrentRound);
        Assert.Equal(1, response.Game.CurrentRound.RoundNumber);
        
        // CurrentTurn情報の検証
        Assert.NotNull(response.Game.CurrentRound.CurrentTurn);
        Assert.Equal(1, response.Game.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal("SettingAnswer", response.Game.CurrentRound.CurrentTurn.Status);
        Assert.NotEmpty(response.Game.CurrentRound.CurrentTurn.DrawerId);
        Assert.Equal(string.Empty, response.Game.CurrentRound.CurrentTurn.Answer);
        
        // Players情報の検証
        Assert.NotNull(response.Game.Players);
        Assert.Single(response.Game.Players);
        Assert.Equal("完全テストプレイヤー", response.Game.Players.First().Name);
        Assert.False(response.Game.Players.First().IsReady);
        Assert.False(response.Game.Players.First().IsDrawer);
        
        // ScoreRecords情報の検証
        Assert.NotNull(response.Game.ScoreRecords);
        Assert.Empty(response.Game.ScoreRecords);
        
        // Player情報の検証
        Assert.NotNull(response.Player);
        Assert.Equal("完全テストプレイヤー", response.Player.Name);
        Assert.NotEmpty(response.Player.Id);
        Assert.False(response.Player.IsReady);
        Assert.False(response.Player.IsDrawer);
        
        // GameとPlayerのIDが一致することを確認
        Assert.Equal(response.Player.Id, response.Game.Players.First().Id);
        Assert.Equal(response.Player.Id, response.Game.CurrentRound.CurrentTurn.DrawerId);
    }

    [Fact]
    public async Task 複数回実行時にユニークなIDが生成される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テストプレイヤー",
            Settings = new GameSettingsDto { TimeLimit = 60, RoundCount = 3, PlayerCount = 4 }
        };

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var response1 = await _useCase.ExecuteAsync(request);
        var response2 = await _useCase.ExecuteAsync(request);

        Assert.NotEqual(response1.Game.Id, response2.Game.Id);
        Assert.NotEqual(response1.Player.Id, response2.Player.Id);
    }
}