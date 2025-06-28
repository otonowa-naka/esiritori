namespace EsiritoriApi.Tests.Api.Controllers;

using EsiritoriApi.Api.Controllers;
using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public sealed class GamesControllerSimpleTests
{
    [Fact]
    public async Task 有効なリクエストでゲーム作成が成功する()
    {
        var mockCreateGameUseCase = new Mock<ICreateGameUseCase>();
        var mockStartGameUseCase = new Mock<IStartGameUseCase>();
        var controller = new GamesController(mockCreateGameUseCase.Object, mockStartGameUseCase.Object);

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

        var expectedResponse = new CreateGameResponse
        {
            Game = new GameDto
            {
                Id = "game123",
                Status = "Waiting",
                Settings = new GameSettingsDto
                {
                    TimeLimit = 60,
                    RoundCount = 3,
                    PlayerCount = 4
                }
            },
            Player = new PlayerDto
            {
                Id = "player123",
                Name = "テストプレイヤー",
                IsReady = false,
                IsDrawer = false
            }
        };

        mockCreateGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedResponse);

        var result = await controller.CreateGame(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<CreateGameResponse>(createdResult.Value);
        Assert.Equal("game123", response.Game.Id);
        Assert.Equal("Waiting", response.Game.Status);
    }

    [Fact]
    public async Task ArgumentExceptionの場合BadRequestを返す()
    {
        var mockCreateGameUseCase = new Mock<ICreateGameUseCase>();
        var mockStartGameUseCase = new Mock<IStartGameUseCase>();
        var controller = new GamesController(mockCreateGameUseCase.Object, mockStartGameUseCase.Object);

        var request = new CreateGameRequest
        {
            CreatorName = "テストプレイヤー",
            Settings = new GameSettingsDto()
        };

        mockCreateGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new ArgumentException("無効な引数"));

        var result = await controller.CreateGame(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = badRequestResult.Value;
        Assert.NotNull(errorResponse);
    }

    [Fact]
    public async Task 一般的な例外の場合InternalServerErrorを返す()
    {
        var mockCreateGameUseCase = new Mock<ICreateGameUseCase>();
        var mockStartGameUseCase = new Mock<IStartGameUseCase>();
        var controller = new GamesController(mockCreateGameUseCase.Object, mockStartGameUseCase.Object);

        var request = new CreateGameRequest
        {
            CreatorName = "テストプレイヤー",
            Settings = new GameSettingsDto()
        };

        mockCreateGameUseCase.Setup(u => u.ExecuteAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("内部エラー"));

        var result = await controller.CreateGame(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }
}
