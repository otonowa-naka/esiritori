namespace EsiritoriApi.Tests.Infrastructure.Repositories;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;
using EsiritoriApi.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Amazon;
using Xunit;

[Trait("Category", "インフラストラクチャテスト")]
public sealed class DynamoDBGameRepositoryTests : IAsyncLifetime
{
    private IAmazonDynamoDB _dynamoDBClient = null!;
    private DynamoDBGameRepository _repository = null!;
    private const string TableName = "EsiritoriGame";
    private const string LocalStackEndpoint = "http://localhost:4566";

    public DynamoDBGameRepositoryTests()
    {
        // Docker Composeで起動したLocalStackを使用
    }

    public async Task InitializeAsync()
    {
        // LocalStack用の環境変数を明示的に設定
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "test");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "test");
        Environment.SetEnvironmentVariable("AWS_DEFAULT_REGION", "ap-northeast-1");
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", LocalStackEndpoint);
        
        // LocalStackが既に起動していることを前提とする
        // 環境変数を使用したクライアント作成
        Console.WriteLine($"Using environment variables approach:");
        Console.WriteLine($"AWS_ENDPOINT_URL: {Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL")}");
        Console.WriteLine($"AWS_ACCESS_KEY_ID: {Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")}");
        Console.WriteLine($"AWS_DEFAULT_REGION: {Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")}");
        
        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = LocalStackEndpoint,
            UseHttp = true,
            MaxErrorRetry = 3,
            Timeout = TimeSpan.FromSeconds(30),
            RegionEndpoint = RegionEndpoint.APNortheast1  // 明示的にap-northeast-1を設定
        };
        
        var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");
        _dynamoDBClient = new AmazonDynamoDBClient(credentials, config);

        // テーブルは既にDocker Composeで作成されているため、作成処理は不要
        // LocalStackとの接続をテスト（リトライ付き）
        await WaitForLocalStackAndTable();
        
        // データのクリーンアップのみ実行
        await ClearTable();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("DynamoDB:TableName", TableName)
            })
            .Build();

        var logger = new Mock<ILogger<DynamoDBGameRepository>>().Object;
        _repository = new DynamoDBGameRepository(_dynamoDBClient, configuration, logger);
    }

    public Task DisposeAsync()
    {
        _dynamoDBClient?.Dispose();
        return Task.CompletedTask;
    }

    private async Task WaitForLocalStackAndTable()
    {
        // LocalStackが起動していない場合はテストをスキップ
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            await httpClient.GetStringAsync($"{LocalStackEndpoint}/_localstack/health");
        }
        catch (Exception)
        {
            Assert.True(false, "LocalStack is not running. Please start Docker Compose: docker compose up -d");
        }

        // テーブルが存在するかチェック（詳細デバッグ付き）
        try
        {
            Console.WriteLine($"DynamoDB Client Config:");
            Console.WriteLine($"  ServiceURL: {_dynamoDBClient.Config.ServiceURL}");
            Console.WriteLine($"  RegionEndpoint: {_dynamoDBClient.Config.RegionEndpoint}");
            Console.WriteLine($"  UseHttp: {_dynamoDBClient.Config.UseHttp}");
            
            var request = new ListTablesRequest();
            Console.WriteLine($"Sending ListTables request to: {_dynamoDBClient.Config.ServiceURL}");
            
            var tables = await _dynamoDBClient.ListTablesAsync(request);
            Console.WriteLine($"Response received. Found {tables.TableNames.Count} tables: [{string.Join(", ", tables.TableNames)}]");
            
            if (tables.TableNames.Contains(TableName))
            {
                Console.WriteLine($"✓ Successfully found table {TableName}");
                return;
            }
            else
            {
                // LocalStackのテーブルが見つからない場合の実用的な対処
                Console.WriteLine("Table not found in default region, attempting to create for testing...");
                
                // テストのためにテーブルを作成する
                try
                {
                    await CreateTestTableIfNeeded();
                    
                    // 再度テーブル一覧を確認
                    var tablesRetry = await _dynamoDBClient.ListTablesAsync();
                    if (tablesRetry.TableNames.Contains(TableName))
                    {
                        Console.WriteLine($"✓ Successfully created and found table {TableName}");
                        return;
                    }
                }
                catch (Exception createEx)
                {
                    Console.WriteLine($"Failed to create test table: {createEx.Message}");
                }
                
                Assert.True(false, $"Table {TableName} not found and could not be created. Available tables: [{string.Join(", ", tables.TableNames)}]. Please ensure Docker Compose is running with table initialization.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception details: {ex}");
            Assert.True(false, $"Failed to connect to LocalStack: {ex.Message}. Please ensure Docker Compose is running.");
        }
    }

    private async Task CreateTestTableIfNeeded()
    {
        Console.WriteLine($"Creating test table {TableName}...");
        
        var createTableRequest = new CreateTableRequest
        {
            TableName = TableName,
            KeySchema = new List<KeySchemaElement>
            {
                new() { AttributeName = "PK", KeyType = KeyType.HASH },
                new() { AttributeName = "SK", KeyType = KeyType.RANGE }
            },
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new() { AttributeName = "PK", AttributeType = ScalarAttributeType.S },
                new() { AttributeName = "SK", AttributeType = ScalarAttributeType.S },
                new() { AttributeName = "GSI1PK", AttributeType = ScalarAttributeType.S },
                new() { AttributeName = "GSI1SK", AttributeType = ScalarAttributeType.S },
                new() { AttributeName = "GSI2PK", AttributeType = ScalarAttributeType.S },
                new() { AttributeName = "GSI2SK", AttributeType = ScalarAttributeType.S }
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new()
                {
                    IndexName = "GSI1-ActiveGameIndex",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new() { AttributeName = "GSI1PK", KeyType = KeyType.HASH },
                        new() { AttributeName = "GSI1SK", KeyType = KeyType.RANGE }
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.KEYS_ONLY },
                    ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 5, WriteCapacityUnits = 5 }
                },
                new()
                {
                    IndexName = "GSI2-PlayerIndex",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new() { AttributeName = "GSI2PK", KeyType = KeyType.HASH },
                        new() { AttributeName = "GSI2SK", KeyType = KeyType.RANGE }
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.ALL },
                    ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 5, WriteCapacityUnits = 5 }
                }
            },
            ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 5, WriteCapacityUnits = 5 }
        };

        await _dynamoDBClient.CreateTableAsync(createTableRequest);
        
        // テーブルがアクティブになるまで待機
        var maxWaitTime = TimeSpan.FromMinutes(1);
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var response = await _dynamoDBClient.DescribeTableAsync(new DescribeTableRequest { TableName = TableName });
            if (response.Table.TableStatus == TableStatus.ACTIVE)
                break;
            await Task.Delay(1000);
        }
        
        Console.WriteLine($"Table {TableName} created successfully");
    }

    private async Task ClearTable()
    {
        var scanRequest = new ScanRequest
        {
            TableName = TableName,
            ProjectionExpression = "PK, SK"
        };

        var scanResponse = await _dynamoDBClient.ScanAsync(scanRequest);
        
        if (scanResponse.Items.Count > 0)
        {
            var deleteRequests = scanResponse.Items.Select(item => new WriteRequest
            {
                DeleteRequest = new DeleteRequest
                {
                    Key = new Dictionary<string, AttributeValue>
                    {
                        ["PK"] = item["PK"],
                        ["SK"] = item["SK"]
                    }
                }
            }).ToList();

            // バッチで削除
            for (int i = 0; i < deleteRequests.Count; i += 25)
            {
                var batchRequest = new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>>
                    {
                        [TableName] = deleteRequests.Skip(i).Take(25).ToList()
                    }
                };
                await _dynamoDBClient.BatchWriteItemAsync(batchRequest);
            }
        }
    }

    private static Game CreateTestGame(string gameId, string creatorName, string? creatorId = null)
    {
        var now = DateTime.UtcNow;
        var id = new GameId(gameId);
        var settings = new GameSettings(60, 3, 4);
        var playerId = creatorId ?? $"player_{gameId}";
        var creator = new Player(new PlayerId(playerId), new PlayerName(creatorName), PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, now);
        var initialRound = Round.CreateInitial(initialTurn, now);
        return new Game(id, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);
    }

    [Fact]
    public async Task ゲーム保存時_正常に保存される()
    {
        await ClearTable();
        
        var gameId = new GameId("123456");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テスト作成者");
        var creatorId = new PlayerId("creator123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);

        await _repository.SaveAsync(game);

        var savedGame = await _repository.FindByIdAsync(gameId);
        Assert.NotNull(savedGame);
        Assert.Equal(game.Id, savedGame.Id);
        Assert.Equal(game.Status, savedGame.Status);
        Assert.Equal(game.Players.Count, savedGame.Players.Count);
    }

    [Fact]
    public async Task ゲーム検索時_存在するゲームが正常に取得される()
    {
        await ClearTable();
        
        var gameId = new GameId("123456");
        var game = CreateTestGame("123456", "テスト作成者", "creator123");
        await _repository.SaveAsync(game);

        var foundGame = await _repository.FindByIdAsync(gameId);

        Assert.NotNull(foundGame);
        Assert.Equal(gameId, foundGame.Id);
        Assert.Equal(game.Status, foundGame.Status);
        Assert.Equal("テスト作成者", foundGame.Players.First().Name.Value);
    }

    [Fact]
    public async Task ゲーム検索時_存在しないゲームの場合nullが返される()
    {
        await ClearTable();
        
        var nonExistentGameId = new GameId("999999");

        var foundGame = await _repository.FindByIdAsync(nonExistentGameId);

        Assert.Null(foundGame);
    }

    [Fact]
    public async Task 全ゲーム取得時_保存されている全ゲームが取得される()
    {
        await ClearTable();
        
        var game1 = CreateTestGame("123456", "プレイヤー1", "player1");
        var game2 = CreateTestGame("654321", "プレイヤー2", "player2");
        await _repository.SaveAsync(game1);
        await _repository.SaveAsync(game2);

        var allGames = await _repository.FindAllAsync();

        Assert.Equal(2, allGames.Count());
        Assert.Contains(allGames, g => g.Id.Equals(game1.Id));
        Assert.Contains(allGames, g => g.Id.Equals(game2.Id));
    }

    [Fact]
    public async Task 全ゲーム取得時_ゲームが存在しない場合空のコレクションが返される()
    {
        await ClearTable();

        var allGames = await _repository.FindAllAsync();

        Assert.Empty(allGames);
    }

    [Fact]
    public async Task ゲーム削除時_正常に削除される()
    {
        await ClearTable();
        
        var gameId = new GameId("123456");
        var game = CreateTestGame("123456", "テスト作成者", "playerX");
        await _repository.SaveAsync(game);

        await _repository.DeleteAsync(gameId);

        var deletedGame = await _repository.FindByIdAsync(gameId);
        Assert.Null(deletedGame);
    }

    [Fact]
    public async Task ゲーム削除時_存在しないゲームの場合例外が発生しない()
    {
        await ClearTable();
        
        var nonExistentGameId = new GameId("999999");

        await _repository.DeleteAsync(nonExistentGameId); // 例外が発生しないことを確認
    }

    [Fact]
    public async Task 同じIDのゲーム保存時_既存のゲームが更新される()
    {
        await ClearTable();
        
        var gameId = new GameId("123456");
        var originalGame = CreateTestGame("123456", "元の作成者", "playerA");
        await _repository.SaveAsync(originalGame);

        var updatedGame = CreateTestGame("123456", "更新された作成者", "playerB");

        await _repository.SaveAsync(updatedGame);

        var savedGame = await _repository.FindByIdAsync(gameId);
        Assert.NotNull(savedGame);
        Assert.Equal("更新された作成者", savedGame.Players.First().Name.Value);
    }


    [Fact]
    public async Task nullゲーム保存時_例外が発生する()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.SaveAsync(null!));
    }

    [Fact]
    public async Task nullゲームID検索時_例外が発生する()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.FindByIdAsync(null!));
    }

    [Fact]
    public async Task nullゲームID削除時_例外が発生する()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.DeleteAsync(null!));
    }

    [Fact]
    public async Task CancellationToken対応_SaveAsync()
    {
        await ClearTable();
        
        var game = CreateTestGame("123456", "テスト作成者");
        var cancellationTokenSource = new CancellationTokenSource();

        await _repository.SaveAsync(game, cancellationTokenSource.Token);

        var savedGame = await _repository.FindByIdAsync(game.Id);
        Assert.NotNull(savedGame);
        Assert.Equal(game.Id, savedGame.Id);
    }

    [Fact]
    public async Task CancellationToken対応_FindByIdAsync()
    {
        await ClearTable();
        
        var game = CreateTestGame("123456", "テスト作成者");
        await _repository.SaveAsync(game);
        var cancellationTokenSource = new CancellationTokenSource();

        var foundGame = await _repository.FindByIdAsync(game.Id, cancellationTokenSource.Token);

        Assert.NotNull(foundGame);
        Assert.Equal(game.Id, foundGame.Id);
    }

    [Fact]
    public async Task CancellationToken対応_FindAllAsync()
    {
        await ClearTable();
        
        var game1 = CreateTestGame("123456", "プレイヤー1");
        var game2 = CreateTestGame("654321", "プレイヤー2");
        await _repository.SaveAsync(game1);
        await _repository.SaveAsync(game2);
        var cancellationTokenSource = new CancellationTokenSource();

        var allGames = await _repository.FindAllAsync(cancellationTokenSource.Token);

        Assert.Equal(2, allGames.Count());
    }

    [Fact]
    public async Task CancellationToken対応_DeleteAsync()
    {
        await ClearTable();
        
        var game = CreateTestGame("123456", "テスト作成者");
        await _repository.SaveAsync(game);
        var cancellationTokenSource = new CancellationTokenSource();

        await _repository.DeleteAsync(game.Id, cancellationTokenSource.Token);

        var deletedGame = await _repository.FindByIdAsync(game.Id);
        Assert.Null(deletedGame);
    }
}