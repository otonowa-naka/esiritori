namespace EsiritoriApi.Application.UseCases;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;

public interface ICreateGameUseCase
{
    Task<CreateGameResponse> ExecuteAsync(CreateGameRequest request, CancellationToken cancellationToken = default);
}

public class CreateGameUseCase : ICreateGameUseCase
{
    private readonly IGameRepository _gameRepository;

    public CreateGameUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<CreateGameResponse> ExecuteAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var creatorName = new PlayerName(request.CreatorName);
        var settings = new GameSettings(
            request.Settings.TimeLimit,
            request.Settings.RoundCount,
            request.Settings.PlayerCount
        );

        var creator = Player.CreateInitial(creatorName);
        var game = Game.NewGame(settings, creator, DateTime.UtcNow);

        await _gameRepository.SaveAsync(game, cancellationToken);

        return CreateGameResponse.FromGame(game);
    }

}
