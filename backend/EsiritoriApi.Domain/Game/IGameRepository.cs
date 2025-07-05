namespace EsiritoriApi.Domain.Game;

using EsiritoriApi.Domain.Game.ValueObjects;

public interface IGameRepository
{
    Task SaveAsync(Game game, CancellationToken cancellationToken = default);
    Task<Game?> FindByIdAsync(GameId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Game>> FindAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(GameId id, CancellationToken cancellationToken = default);
}