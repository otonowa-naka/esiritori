namespace EsiritoriApi.Application.DTOs;

using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game;

public sealed class StartGameResponse
{
    public GameDto Game { get; set; } = new();

    public static StartGameResponse FromGame(Game game)
    {
        return new StartGameResponse
        {
            Game = GameDto.FromGame(game)
        };
    }
} 