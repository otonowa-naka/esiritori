namespace EsiritoriApi.Tests.Integration;

using EsiritoriApi.Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public sealed class GameApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GameApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_api_games_有効なリクエストでゲームが作成される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "統合テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 90,
                RoundCount = 5,
                PlayerCount = 6
            }
        };

        var response = await _client.PostAsJsonAsync("/api/games", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var gameResponse = JsonSerializer.Deserialize<CreateGameResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(gameResponse);
        Assert.NotNull(gameResponse.Game);
        Assert.NotNull(gameResponse.Player);
        Assert.Equal("統合テスト作成者", gameResponse.Player.Name);
        Assert.Equal("Waiting", gameResponse.Game.Status);
        Assert.Equal(90, gameResponse.Game.Settings.TimeLimit);
        Assert.Equal(5, gameResponse.Game.Settings.RoundCount);
        Assert.Equal(6, gameResponse.Game.Settings.PlayerCount);
        Assert.Single(gameResponse.Game.Players);
        Assert.False(gameResponse.Player.IsReady);
        Assert.False(gameResponse.Player.IsDrawer);

        Assert.Matches(@"^\d{6}$", gameResponse.Game.Id);

        Assert.Matches(@"^[a-f0-9]{12}$", gameResponse.Player.Id);
    }

    [Fact]
    public async Task POST_api_games_空のプレイヤー名でBadRequestが返される()
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

        var response = await _client.PostAsJsonAsync("/api/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("プレイヤー名は必須です", responseContent);
    }

    [Fact]
    public async Task POST_api_games_nullプレイヤー名でBadRequestが返される()
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

        var response = await _client.PostAsJsonAsync("/api/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_api_games_空白のみのプレイヤー名でBadRequestが返される()
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

        var response = await _client.PostAsJsonAsync("/api/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_api_games_無効な制限時間でBadRequestが返される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 25, // 最小値未満
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var response = await _client.PostAsJsonAsync("/api/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("制限時間は30秒から300秒の間で設定してください", responseContent);
    }

    [Fact]
    public async Task POST_api_games_無効なラウンド数でBadRequestが返される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 15, // 最大値超過
                PlayerCount = 4
            }
        };

        var response = await _client.PostAsJsonAsync("/api/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("ラウンド数は1から10の間で設定してください", responseContent);
    }

    [Fact]
    public async Task POST_api_games_無効なプレイヤー数でBadRequestが返される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 1 // 最小値未満
            }
        };

        var response = await _client.PostAsJsonAsync("/api/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("プレイヤー数は2人から8人の間で設定してください", responseContent);
    }

    [Fact]
    public async Task POST_api_games_複数のゲームが独立して作成される()
    {
        var request1 = new CreateGameRequest
        {
            CreatorName = "作成者1",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var request2 = new CreateGameRequest
        {
            CreatorName = "作成者2",
            Settings = new GameSettingsDto
            {
                TimeLimit = 120,
                RoundCount = 5,
                PlayerCount = 6
            }
        };

        var response1 = await _client.PostAsJsonAsync("/api/games", request1);
        var response2 = await _client.PostAsJsonAsync("/api/games", request2);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);

        var game1Content = await response1.Content.ReadAsStringAsync();
        var game2Content = await response2.Content.ReadAsStringAsync();

        var game1Response = JsonSerializer.Deserialize<CreateGameResponse>(game1Content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var game2Response = JsonSerializer.Deserialize<CreateGameResponse>(game2Content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(game1Response);
        Assert.NotNull(game2Response);

        Assert.NotEqual(game1Response.Game.Id, game2Response.Game.Id);

        Assert.NotEqual(game1Response.Player.Id, game2Response.Player.Id);

        Assert.Equal("作成者1", game1Response.Player.Name);
        Assert.Equal("作成者2", game2Response.Player.Name);
        Assert.Equal(60, game1Response.Game.Settings.TimeLimit);
        Assert.Equal(120, game2Response.Game.Settings.TimeLimit);
    }

    [Fact]
    public async Task POST_api_games_CORSヘッダーが適切に設定される()
    {
        var request = new CreateGameRequest
        {
            CreatorName = "CORS テスト作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var response = await _client.PostAsJsonAsync("/api/games", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var corsHeader = response.Headers.GetValues("Access-Control-Allow-Origin").First();
            Assert.Equal("*", corsHeader);
        }
    }
}
