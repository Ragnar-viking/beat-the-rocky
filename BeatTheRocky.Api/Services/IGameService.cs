using BeatTheRocky.Api.Core.Enums;
using BeatTheRocky.Api.Core.Models;

namespace BeatTheRocky.Api.Services;

public interface IGameService
{
    GameSession CreateSession();
    GameSession ProcessToss(Guid sessionId, CoinChoice userChoice);
    GameSession MakeMove(Guid sessionId, int cellIndex);
    GameSession? GetSession(Guid sessionId);
    GameSession ResetSession(Guid sessionId);
}
