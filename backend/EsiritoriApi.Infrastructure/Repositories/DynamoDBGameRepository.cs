namespace EsiritoriApi.Infrastructure.Repositories;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Infrastructure.DynamoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// DynamoDB を使用した Game リポジトリの実装
/// </summary>
public sealed class DynamoDBGameRepository : IGameRepository
{
    private readonly Table _table;
    private readonly ILogger<DynamoDBGameRepository> _logger;
    private const string SortKey = "META";

    public DynamoDBGameRepository(
        IAmazonDynamoDB dynamoDBClient,
        IConfiguration configuration,
        ILogger<DynamoDBGameRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(dynamoDBClient);
        ArgumentNullException.ThrowIfNull(configuration);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var tableName = configuration["DynamoDB:TableName"] ?? "EsiritoriGame";
        _table = Table.LoadTable(dynamoDBClient, tableName);
    }

    /// <summary>
    /// ゲームを保存します
    /// </summary>
    public async Task SaveAsync(Game game, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);

        try
        {
            var document = GameMapper.ToDocument(game);
            
            _logger.LogDebug("Saving game {GameId} to DynamoDB", game.Id.Value);
            
            await _table.PutItemAsync(document, cancellationToken);
            
            _logger.LogInformation("Successfully saved game {GameId}", game.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save game {GameId}", game.Id.Value);
            throw;
        }
    }

    /// <summary>
    /// ID でゲームを取得します
    /// </summary>
    public async Task<Game?> FindByIdAsync(GameId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            var partitionKey = GameMapper.GetPartitionKey(id);
            
            _logger.LogDebug("Finding game {GameId} in DynamoDB", id.Value);
            
            var document = await _table.GetItemAsync(partitionKey, SortKey, cancellationToken);
            
            if (document == null)
            {
                _logger.LogDebug("Game {GameId} not found", id.Value);
                return null;
            }

            var game = GameMapper.ToEntity(document);
            
            _logger.LogDebug("Successfully retrieved game {GameId}", id.Value);
            
            return game;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find game {GameId}", id.Value);
            throw;
        }
    }

    /// <summary>
    /// 全てのゲームを取得します
    /// </summary>
    public async Task<IEnumerable<Game>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Finding all games in DynamoDB");

            var scanFilter = new ScanFilter();
            scanFilter.AddCondition("SK", ScanOperator.Equal, SortKey);

            var search = _table.Scan(scanFilter);
            var documents = new List<Document>();
            
            do
            {
                var documentBatch = await search.GetNextSetAsync(cancellationToken);
                documents.AddRange(documentBatch);
            }
            while (!search.IsDone);

            var games = documents.Select(GameMapper.ToEntity).ToList();
            
            _logger.LogInformation("Successfully retrieved {GameCount} games", games.Count);
            
            return games;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find all games");
            throw;
        }
    }

    /// <summary>
    /// ゲームを削除します
    /// </summary>
    public async Task DeleteAsync(GameId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            var partitionKey = GameMapper.GetPartitionKey(id);
            
            _logger.LogDebug("Deleting game {GameId} from DynamoDB", id.Value);
            
            await _table.DeleteItemAsync(partitionKey, SortKey, cancellationToken);
            
            _logger.LogInformation("Successfully deleted game {GameId}", id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete game {GameId}", id.Value);
            throw;
        }
    }

}