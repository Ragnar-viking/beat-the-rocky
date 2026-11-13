using BeatTheRocky.Api.Core.Enums;
using BeatTheRocky.Api.Core.Models;

namespace BeatTheRocky.Api.Factories;

public class GameFactory : IGameFactory
{
    public GameSession CreateNewSession()
    {
        return new GameSession
        {
            Id = Guid.NewGuid(),
            Board = new Board(),
            Status = GameStatus.AwaitingCoinToss,
            HumanSymbol = PlayerSymbol.Empty,
            RockySymbol = PlayerSymbol.Empty,
            CurrentTurn = PlayerSymbol.Empty,
            RockyComment = "Welcome to the ring! Call Heads or Tails to see who gets the opening punch."
        };
    }
}
