namespace EsiritoriApi.Tests.Api.Controllers;

using EsiritoriApi.Api.Controllers;
using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.Interfaces;
using EsiritoriApi.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public sealed class GamesControllerTests
{
    private readonly Mock<ICreateGameUseCase> _mockUseCase;
    private readonly GamesController _controller;

    public GamesControllerTests()
    {
        _mockUseCase = new Mock<ICreateGameUseCase>();
        _controller = new GamesController(_mockUseCase.Object);
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
                        Status = "NotStarted",
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

        _mockUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
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
        _mockUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
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
        _mockUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
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
        _mockUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
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
        _mockUseCase.Setup(u => u.ExecuteAsync(request, cancellationToken))
                   .ReturnsAsync(expectedResponse);

        await _controller.CreateGame(request, cancellationToken);

        _mockUseCase.Verify(u => u.ExecuteAsync(request, cancellationToken), Times.Once);
    }

    [Fact]
    public void コンストラクタ_nullUseCaseで例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() => new GamesController(null!));
    }
}
