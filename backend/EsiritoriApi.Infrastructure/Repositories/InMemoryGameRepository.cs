namespace EsiritoriApi.Infrastructure.Repositories;

using EsiritoriApi.Application.Interfaces;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.ValueObjects;
using System.Collections.Concurrent;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<string, Game> _games = new();

    public Task SaveAsync(Game game, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);

        _games.AddOrUpdate(game.Id.Value, game, (_, _) => game);
        return Task.CompletedTask;
    }

    public Task<Game?> FindByIdAsync(GameId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        _games.TryGetValue(id.Value, out var game);
        return Task.FromResult(game);
    }

    public Task<IEnumerable<Game>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        var games = _games.Values.AsEnumerable();
        return Task.FromResult(games);
    }

    public Task DeleteAsync(GameId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        _games.TryRemove(id.Value, out _);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _games.Clear();
    }

    public int Count => _games.Count;
}
