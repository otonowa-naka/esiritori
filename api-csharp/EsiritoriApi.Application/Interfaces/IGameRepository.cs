namespace EsiritoriApi.Application.Interfaces;

using EsiritoriApi.Domain.Entities;
using EsiritoriApi.Domain.ValueObjects;

public interface IGameRepository
{
    Task SaveAsync(Game game, CancellationToken cancellationToken = default);
    Task<Game?> FindByIdAsync(GameId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Game>> FindAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(GameId id, CancellationToken cancellationToken = default);
}
