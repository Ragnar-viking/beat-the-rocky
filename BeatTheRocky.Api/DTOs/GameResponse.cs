using BeatTheRocky.Api.Core.Enums;
using BeatTheRocky.Api.Core.Models;

namespace BeatTheRocky.Api.DTOs;

public record GameResponse(
    Guid Id,
    string[] Board,
    string HumanSymbol,
    string RockySymbol,
    string CurrentTurn,
    string Status,
    string? UserCoinBet,
    string? TossResult,
    bool? UserWonToss,
    int[]? WinningCombination,
    string RockyComment,
    bool IsGameOver
)
{
    public static GameResponse FromSession(GameSession session)
    {
        var boardArray = session.Board.GetCells().Select(c => c switch
        {
            PlayerSymbol.X => "X",
            PlayerSymbol.O => "O",
            _ => ""
        }).ToArray();

        var isGameOver = session.Status is GameStatus.RockyWon or GameStatus.PlayerWon or GameStatus.Draw;

        return new GameResponse(
            Id: session.Id,
            Board: boardArray,
            HumanSymbol: session.HumanSymbol == PlayerSymbol.Empty ? "" : session.HumanSymbol.ToString(),
            RockySymbol: session.RockySymbol == PlayerSymbol.Empty ? "" : session.RockySymbol.ToString(),
            CurrentTurn: session.CurrentTurn == PlayerSymbol.Empty ? "" : session.CurrentTurn.ToString(),
            Status: session.Status.ToString(),
            UserCoinBet: session.UserCoinBet?.ToString(),
            TossResult: session.TossResult?.ToString(),
            UserWonToss: session.UserWonToss,
            WinningCombination: session.WinningCombination,
            RockyComment: session.RockyComment,
            IsGameOver: isGameOver
        );
    }
}
