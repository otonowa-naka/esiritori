namespace EsiritoriApi.Infrastructure.DynamoDB;

using Amazon.DynamoDBv2.DocumentModel;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;
using EsiritoriApi.Domain.Shared.ValueObjects;
using System.Text.Json;

/// <summary>
/// Game エンティティと DynamoDB Document の変換を行うマッパー
/// </summary>
public static class GameMapper
{
    private const string PartitionKeyPrefix = "GAME#";
    private const string SortKey = "META";
    private const string StatusPrefix = "STATUS#";
    private const string CreatedAtPrefix = "CREATED#";

    /// <summary>
    /// Game エンティティを DynamoDB Document に変換
    /// </summary>
    public static Document ToDocument(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var document = new Document
        {
            ["PK"] = $"{PartitionKeyPrefix}{game.Id.Value}",
            ["SK"] = SortKey,
            ["gameId"] = game.Id.Value,
            ["status"] = game.Status.ToString().ToLowerInvariant(),
            ["createdAt"] = ((DateTimeOffset)game.CreatedAt).ToUnixTimeSeconds(),
            ["updatedAt"] = ((DateTimeOffset)game.UpdatedAt).ToUnixTimeSeconds(),
            
            // ゲーム設定
            ["settings"] = Document.FromJson(JsonSerializer.Serialize(new 
            {
                timeLimit = game.Settings.TimeLimit,
                roundCount = game.Settings.RoundCount,
                playerCount = game.Settings.PlayerCount
            })),
            
            // 現在のラウンド情報
            ["currentRound"] = Document.FromJson(JsonSerializer.Serialize(new 
            {
                roundNumber = game.CurrentRound.RoundNumber,
                currentTurn = new 
                {
                    turnNumber = game.CurrentRound.CurrentTurn.TurnNumber,
                    drawerId = game.CurrentRound.CurrentTurn.DrawerId.Value,
                    answer = game.CurrentRound.CurrentTurn.Answer.HasValue ? game.CurrentRound.CurrentTurn.Answer.Value.Value : "",
                    status = game.CurrentRound.CurrentTurn.Status.ToString().ToLowerInvariant(),
                    timeLimit = game.CurrentRound.CurrentTurn.TimeLimit,
                    startedAt = ((DateTimeOffset)game.CurrentRound.CurrentTurn.StartedAt).ToUnixTimeSeconds(),
                    endedAt = game.CurrentRound.CurrentTurn.EndedAt.HasValue 
                        ? ((DateTimeOffset)game.CurrentRound.CurrentTurn.EndedAt.Value).ToUnixTimeSeconds() 
                        : (long?)null,
                    correctPlayerIds = game.CurrentRound.CurrentTurn.CorrectPlayerIds.Select(id => id.Value).ToArray()
                }
            })),
            
            // プレイヤー情報
            ["players"] = new DynamoDBList(game.Players.Select(player => Document.FromJson(JsonSerializer.Serialize(new 
            {
                id = player.Id.Value,
                name = player.Name.Value,
                status = player.Status.ToString().ToLowerInvariant(),
                isReady = player.IsReady,
                isDrawer = player.IsDrawer
            })))),
            
            // スコア履歴
            ["scoreHistories"] = new DynamoDBList(game.ScoreHistories.Select(score => Document.FromJson(JsonSerializer.Serialize(new 
            {
                playerId = score.PlayerId.Value,
                roundNumber = score.RoundNumber,
                turnNumber = score.TurnNumber,
                points = score.Points,
                reason = ConvertToSnakeCase(score.Reason.ToString()),
                timestamp = ((DateTimeOffset)score.Timestamp).ToUnixTimeSeconds()
            })))),
            
            // GSI用フィールド
            ["GSI1PK"] = $"{StatusPrefix}{game.Status.ToString().ToLowerInvariant()}",
            ["GSI1SK"] = $"{CreatedAtPrefix}{((DateTimeOffset)game.CreatedAt).ToUnixTimeSeconds()}"
        };

        // TTL設定（完了したゲームは7日後に削除）
        if (game.Status == GameStatus.Finished)
        {
            document["ttl"] = ((DateTimeOffset)DateTime.UtcNow.AddDays(7)).ToUnixTimeSeconds();
        }

        return document;
    }

    /// <summary>
    /// DynamoDB Document を Game エンティティに変換
    /// </summary>
    public static Game ToEntity(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var gameId = new GameId(document["gameId"].AsString());
        var status = Enum.Parse<GameStatus>(document["status"].AsString(), true);
        var createdAt = DateTimeOffset.FromUnixTimeSeconds(document["createdAt"].AsLong()).DateTime;
        var updatedAt = DateTimeOffset.FromUnixTimeSeconds(document["updatedAt"].AsLong()).DateTime;

        // ゲーム設定の復元
        var settingsDoc = document["settings"].AsDocument();
        var settings = new GameSettings(
            settingsDoc["timeLimit"].AsInt(),
            settingsDoc["roundCount"].AsInt(),
            settingsDoc["playerCount"].AsInt()
        );

        // プレイヤー情報の復元
        var playersList = document["players"].AsDynamoDBList();
        var players = new List<Player>();
        foreach (var playerEntry in playersList.Entries)
        {
            var playerDoc = playerEntry.AsDocument();
            players.Add(new Player(
                new PlayerId(playerDoc["id"].AsString()),
                new PlayerName(playerDoc["name"].AsString()),
                Enum.Parse<PlayerStatus>(playerDoc["status"].AsString(), true),
                playerDoc["isReady"].AsBoolean(),
                playerDoc["isDrawer"].AsBoolean()
            ));
        }

        // スコア履歴の復元
        var scoreHistoriesList = document["scoreHistories"].AsDynamoDBList();
        var scoreHistories = new List<ScoreHistory>();
        foreach (var scoreEntry in scoreHistoriesList.Entries)
        {
            var scoreDoc = scoreEntry.AsDocument();
            scoreHistories.Add(new ScoreHistory(
                new PlayerId(scoreDoc["playerId"].AsString()),
                scoreDoc["roundNumber"].AsInt(),
                scoreDoc["turnNumber"].AsInt(),
                scoreDoc["points"].AsInt(),
                Enum.Parse<ScoreReason>(ConvertToPascalCase(scoreDoc["reason"].AsString()), true),
                DateTimeOffset.FromUnixTimeSeconds(scoreDoc["timestamp"].AsLong()).DateTime
            ));
        }

        // 現在のラウンド情報の復元
        var currentRoundDoc = document["currentRound"].AsDocument();
        var currentTurnDoc = currentRoundDoc["currentTurn"].AsDocument();
        
        var drawerId = new PlayerId(currentTurnDoc["drawerId"].AsString());
        var answerValue = currentTurnDoc.ContainsKey("answer") ? currentTurnDoc["answer"].AsString() : "";
        var answer = string.IsNullOrEmpty(answerValue) ? Option<Answer>.None() : Option<Answer>.Some(new Answer(answerValue));
        var turnStatus = Enum.Parse<TurnStatus>(currentTurnDoc["status"].AsString(), true);
        var timeLimit = currentTurnDoc["timeLimit"].AsInt();
        
        var startedAt = DateTimeOffset.FromUnixTimeSeconds(currentTurnDoc["startedAt"].AsLong()).DateTime;
        
        var endedAt = Option<DateTime>.None();
        if (currentTurnDoc.ContainsKey("endedAt") && currentTurnDoc["endedAt"] != null)
        {
            try
            {
                var endedAtValue = currentTurnDoc["endedAt"].AsLong();
                endedAt = Option<DateTime>.Some(DateTimeOffset.FromUnixTimeSeconds(endedAtValue).DateTime);
            }
            catch
            {
                // If the value is null or invalid, keep as None
            }
        }
        
        var correctPlayerIds = currentTurnDoc.ContainsKey("correctPlayerIds") 
            ? currentTurnDoc["correctPlayerIds"].AsListOfString().Select(id => new PlayerId(id)).ToList()
            : new List<PlayerId>();

        var currentTurn = new Turn(
            currentTurnDoc["turnNumber"].AsInt(),
            drawerId,
            answer,
            turnStatus,
            timeLimit,
            startedAt,
            endedAt,
            correctPlayerIds
        );

        var currentRound = new Round(
            currentRoundDoc["roundNumber"].AsInt(),
            currentTurn,
            startedAt, // Roundの開始時刻はTurnの開始時刻と同じとする
            Option<DateTime>.None() // 現在進行中のラウンドなので終了時刻はNone
        );

        return new Game(gameId, settings, status, currentRound, players, scoreHistories, createdAt, updatedAt);
    }

    /// <summary>
    /// Game ID から DynamoDB のパーティションキーを生成
    /// </summary>
    public static string GetPartitionKey(GameId gameId)
    {
        return $"{PartitionKeyPrefix}{gameId.Value}";
    }

    /// <summary>
    /// DynamoDB のパーティションキーから Game ID を抽出
    /// </summary>
    public static GameId ExtractGameId(string partitionKey)
    {
        if (!partitionKey.StartsWith(PartitionKeyPrefix))
            throw new ArgumentException($"Invalid partition key format: {partitionKey}");
        
        var gameIdValue = partitionKey.Substring(PartitionKeyPrefix.Length);
        return new GameId(gameIdValue);
    }

    /// <summary>
    /// 文字列をスネークケースに変換する
    /// </summary>
    private static string ConvertToSnakeCase(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1_$2").ToLowerInvariant();
    }

    /// <summary>
    /// スネークケースの文字列をパスカルケースに変換する
    /// </summary>
    private static string ConvertToPascalCase(string input)
    {
        return string.Join("", input.Split('_')
            .Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
    }
}