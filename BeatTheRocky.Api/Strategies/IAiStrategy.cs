using BeatTheRocky.Api.Core.Enums;
using BeatTheRocky.Api.Core.Models;

namespace BeatTheRocky.Api.Strategies;

public interface IAiStrategy
{
    int CalculateBestMove(Board board, PlayerSymbol aiSymbol, PlayerSymbol humanSymbol);
}
