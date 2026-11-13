using BeatTheRocky.Api.Core.Models;

namespace BeatTheRocky.Api.Repositories;

public interface IGameSessionRepository
{
    void Save(GameSession session);
    GameSession? Get(Guid id);
    bool Delete(Guid id);
}
