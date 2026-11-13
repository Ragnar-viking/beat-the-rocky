using System.Collections.Concurrent;
using BeatTheRocky.Api.Core.Models;

namespace BeatTheRocky.Api.Repositories;

public class InMemoryGameSessionRepository : IGameSessionRepository
{
    private readonly ConcurrentDictionary<Guid, GameSession> _sessions = new();

    public void Save(GameSession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        _sessions[session.Id] = session;
    }

    public GameSession? Get(Guid id)
    {
        _sessions.TryGetValue(id, out var session);
        return session;
    }

    public bool Delete(Guid id)
    {
        return _sessions.TryRemove(id, out _);
    }
}
