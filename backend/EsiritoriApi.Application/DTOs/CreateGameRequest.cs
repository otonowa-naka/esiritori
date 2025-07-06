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

    public static GameSettingsDto FromGameSettings(EsiritoriApi.Domain.Game.ValueObjects.GameSettings settings)
    {
        return new GameSettingsDto
        {
            TimeLimit = settings.TimeLimit,
            RoundCount = settings.RoundCount,
            PlayerCount = settings.PlayerCount
        };
    }
}
