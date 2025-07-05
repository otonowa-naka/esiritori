namespace EsiritoriApi.Tests.Api.Controllers;

using EsiritoriApi.Api.Controllers;
using EsiritoriApi.Application.DTOs;

using EsiritoriApi.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public sealed class GamesControllerTests
{
    private readonly Mock<ICreateGameUseCase> _mockCreateGameUseCase;
    private readonly Mock<IStartGameUseCase> _mockStartGameUseCase;
    private readonly GamesController _controller;

    public GamesControllerTests()
    {
        _mockCreateGameUseCase = new Mock<ICreateGameUseCase>();
        _mockStartGameUseCase = new Mock<IStartGameUseCase>();
        _controller = new GamesController(_mockCreateGameUseCase.Object, _mockStartGameUseCase.Object);
    }

    [Fact]
    public async Task ゲーム作成時_有効なリクエストで正常にゲームが作成される()
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

        var expectedResponse = new CreateGameResponse
        {
            Game = new GameDto
            {
                Id = "123456",
                Status = "Waiting",
                Settings = new GameSettingsDto
                {
                    TimeLimit = 60,
                    RoundCount = 3,
                    PlayerCount = 4
                },
                CurrentRound = new RoundDto
                {
                    RoundNumber = 1,
                    CurrentTurn = new TurnDto
                    {
                        TurnNumber = 1,
                        Status = "SettingAnswer",
                        DrawerId = "creator123",
                        Answer = ""
                    }
                },
                Players = new List<PlayerDto>
                {
                    new PlayerDto
                    {
                        Id = "creator123",
                        Name = "テスト作成者",
                        IsReady = false,
                        IsDrawer = false
                    }
                },
                ScoreRecords = new List<ScoreRecordDto>()
            },
            Player = new PlayerDto
            {
                Id = "creator123",
                Name = "テスト作成者",
                IsReady = false,
                IsDrawer = false
            }
        };

        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedResponse);

        var result = await _controller.CreateGame(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<CreateGameResponse>(createdResult.Value);
        Assert.Equal(expectedResponse.Game.Id, response.Game.Id);
        Assert.Equal(expectedResponse.Player.Name, response.Player.Name);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task ゲーム作成時_空のプレイヤー名でBadRequestが返される()
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

        var result = await _controller.CreateGame(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);

        var errorResponse = badRequestResult.Value;
        Assert.NotNull(errorResponse);
    }

    [Fact]
    public async Task ゲーム作成時_nullプレイヤー名でBadRequestが返される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = null!,
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var result = await _controller.CreateGame(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ゲーム作成時_空白のみのプレイヤー名でBadRequestが返される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "   ",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var result = await _controller.CreateGame(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ゲーム作成時_ArgumentExceptionでBadRequestが返される()
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

        var expectedException = new ArgumentException("無効な引数です");
        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        var result = await _controller.CreateGame(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);

        var errorResponse = badRequestResult.Value;
        Assert.NotNull(errorResponse);
    }

    [Fact]
    public async Task ゲーム作成時_InvalidOperationExceptionでBadRequestが返される()
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

        var expectedException = new InvalidOperationException("無効な操作です");
        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        var result = await _controller.CreateGame(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ゲーム作成時_予期しない例外で500エラーが返される()
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

        var expectedException = new Exception("予期しないエラー");
        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        var result = await _controller.CreateGame(request);

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);

        var errorResponse = statusCodeResult.Value;
        Assert.NotNull(errorResponse);
    }

    [Fact]
    public async Task ゲーム作成時_キャンセレーショントークンが適切に渡される()
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

        var expectedResponse = new CreateGameResponse
        {
            Game = new GameDto { Id = "123456" },
            Player = new PlayerDto { Id = "creator123" }
        };

        var cancellationToken = new CancellationToken();
        _mockCreateGameUseCase.Setup(u => u.ExecuteAsync(request, cancellationToken))
                   .ReturnsAsync(expectedResponse);

        await _controller.CreateGame(request, cancellationToken);

        _mockCreateGameUseCase.Verify(u => u.ExecuteAsync(request, cancellationToken), Times.Once);
    }

    [Fact]
    public void コンストラクタ_nullUseCaseで例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() => new GamesController(null!, null!));
    }

    [Fact]
    public async Task StartGame_有効なゲームIDで正常にゲームが開始される()
    {
        var gameId = "123456";
        var expectedResponse = new StartGameResponse
        {
            Game = new GameDto
            {
                Id = gameId,
                Status = "Playing",
                Settings = new GameSettingsDto
                {
                    TimeLimit = 60,
                    RoundCount = 3,
                    PlayerCount = 4
                },
                CurrentRound = new RoundDto
                {
                    RoundNumber = 1,
                    CurrentTurn = new TurnDto
                    {
                        TurnNumber = 1,
                        Status = "SettingAnswer",
                        DrawerId = "creator123",
                        Answer = ""
                    }
                },
                Players = new List<PlayerDto>
                {
                    new PlayerDto
                    {
                        Id = "creator123",
                        Name = "作成者",
                        IsReady = true,
                        IsDrawer = true
                    }
                },
                ScoreRecords = new List<ScoreRecordDto>()
            }
        };

        _mockStartGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<StartGameRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedResponse);

        var result = await _controller.StartGame(gameId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<StartGameResponse>(okResult.Value);
        Assert.Equal(gameId, response.Game.Id);
        Assert.Equal("Playing", response.Game.Status);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task StartGame_空のゲームIDでBadRequestが返される()
    {
        var result = await _controller.StartGame("");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task StartGame_nullゲームIDでBadRequestが返される()
    {
        var result = await _controller.StartGame(null!);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task StartGame_ArgumentExceptionでBadRequestが返される()
    {
        var gameId = "123456";
        var expectedException = new ArgumentException("無効な引数です");
        _mockStartGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<StartGameRequest>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        var result = await _controller.StartGame(gameId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task StartGame_InvalidOperationExceptionでBadRequestが返される()
    {
        var gameId = "123456";
        var expectedException = new InvalidOperationException("無効な操作です");
        _mockStartGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<StartGameRequest>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        var result = await _controller.StartGame(gameId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task StartGame_予期しない例外で500エラーが返される()
    {
        var gameId = "123456";
        var expectedException = new Exception("予期しないエラー");
        _mockStartGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<StartGameRequest>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        var result = await _controller.StartGame(gameId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task StartGame_キャンセレーショントークンが適切に渡される()
    {
        var gameId = "123456";
        var expectedResponse = new StartGameResponse
        {
            Game = new GameDto { Id = gameId, Status = "Playing" }
        };

        var cancellationToken = new CancellationToken();
        _mockStartGameUseCase.Setup(u => u.ExecuteAsync(It.Is<StartGameRequest>(r => r.GameId == gameId), cancellationToken))
                   .ReturnsAsync(expectedResponse);

        await _controller.StartGame(gameId, cancellationToken);

        _mockStartGameUseCase.Verify(u => u.ExecuteAsync(It.Is<StartGameRequest>(r => r.GameId == gameId), cancellationToken), Times.Once);
    }
}
