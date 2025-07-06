namespace EsiritoriApi.Tests.Integration;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Infrastructure.Repositories;
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
    private static readonly InMemoryGameRepository _sharedRepository = new();

    public GameApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing IGameRepository registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGameRepository));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                // Add shared singleton repository for testing
                services.AddSingleton<IGameRepository>(_sharedRepository);
            });
        });
        _client = _factory.CreateClient();
    }

    private IGameRepository GetRepository()
    {
        return _sharedRepository;
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

        Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", gameResponse.Game.Id);

        Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", gameResponse.Player.Id);
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

    [Fact]
    public async Task StartGame_有効なゲームIDでゲームが正常に開始される()
    {
        var now = DateTime.UtcNow;
        // Arrange - ゲームを作成
        var createRequest = new CreateGameRequest
        {
            CreatorName = "作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdGame = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // プレイヤーを追加して準備完了状態にする
        var repository = GetRepository();
        var game = await repository.FindByIdAsync(new GameId(createdGame!.Game.Id), CancellationToken.None);
        Assert.NotNull(game);
        var player1Id = new PlayerId("integration_test_player1_start");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(new PlayerId(createdGame.Player.Id), true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);
        await repository.SaveAsync(game, CancellationToken.None);

        // Act - ゲームを開始
        var startResponse = await _client.PostAsync($"/api/games/{createdGame.Game.Id}/start", null);

        // Assert
        startResponse.EnsureSuccessStatusCode();
        var startGameResponse = await startResponse.Content.ReadFromJsonAsync<StartGameResponse>();
        Assert.NotNull(startGameResponse);
        Assert.Equal("Playing", startGameResponse.Game.Status);
        Assert.Equal(1, startGameResponse.Game.CurrentRound.RoundNumber);
        Assert.Equal(1, startGameResponse.Game.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal("SettingAnswer", startGameResponse.Game.CurrentRound.CurrentTurn.Status);
    }

    [Fact]
    public async Task StartGame_存在しないゲームIDで400エラーが返される()
    {
        // Act
        var response = await _client.PostAsync("/api/games/nonexistent/start", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StartGame_空のゲームIDで404エラーが返される()
    {
        // Act
        var response = await _client.PostAsync("/api/games//start", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StartGame_プレイヤーが2人未満で400エラーが返される()
    {
        // Arrange - ゲームを作成（プレイヤー1人のみ）
        var createRequest = new CreateGameRequest
        {
            CreatorName = "作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdGame = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // Act - ゲームを開始
        var startResponse = await _client.PostAsync($"/api/games/{createdGame!.Game.Id}/start", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, startResponse.StatusCode);
    }

    [Fact]
    public async Task StartGame_全プレイヤーが準備完了でない場合400エラーが返される()
    {
        var now = DateTime.UtcNow;
        // Arrange - ゲームを作成
        var createRequest = new CreateGameRequest
        {
            CreatorName = "作成者",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdGame = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // プレイヤーを追加するが準備完了状態にしない
        var repository = GetRepository();
        var game = await repository.FindByIdAsync(new GameId(createdGame!.Game.Id), CancellationToken.None);
        Assert.NotNull(game);
        var player1Id = new PlayerId("integration_test_player1_notready");
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        // 準備完了状態にしない
        await repository.SaveAsync(game, CancellationToken.None);

        // Act - ゲームを開始
        var startResponse = await _client.PostAsync($"/api/games/{createdGame.Game.Id}/start", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, startResponse.StatusCode);
    }
}
