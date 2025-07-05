namespace EsiritoriApi.Application.DTOs;

public sealed class CreateGameRequest
{
    public string CreatorName { get; set; } = string.Empty;
    public GameSettingsDto Settings { get; set; } = new();
}

public sealed class GameSettingsDto
{
    public int TimeLimit { get; set; } = 60;
    public int RoundCount { get; set; } = 3;
    public int PlayerCount { get; set; } = 4;
}
