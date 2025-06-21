namespace EsiritoriApi.Application.DTOs;

public sealed class CreateGameResponse
{
    public GameDto Game { get; set; } = new();
    public PlayerDto Player { get; set; } = new();
}

public sealed class GameDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public GameSettingsDto Settings { get; set; } = new();
    public RoundDto CurrentRound { get; set; } = new();
    public List<PlayerDto> Players { get; set; } = new();
    public List<ScoreRecordDto> ScoreRecords { get; set; } = new();
}

public sealed class RoundDto
{
    public int RoundNumber { get; set; }
    public TurnDto CurrentTurn { get; set; } = new();
}

public sealed class TurnDto
{
    public int TurnNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DrawerId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public sealed class PlayerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    public bool IsDrawer { get; set; }
}

public sealed class ScoreRecordDto
{
    public string PlayerId { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public int TurnNumber { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
