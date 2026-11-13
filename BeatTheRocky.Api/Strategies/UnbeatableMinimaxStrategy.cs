using BeatTheRocky.Api.Core.Enums;
using BeatTheRocky.Api.Core.Models;

namespace BeatTheRocky.Api.Strategies;

public class UnbeatableMinimaxStrategy : IAiStrategy
{
    private static readonly Random Rnd = new();

    public int CalculateBestMove(Board board, PlayerSymbol aiSymbol, PlayerSymbol humanSymbol)
    {
        var availableMoves = board.GetAvailableMoves();
        if (availableMoves.Count == 0) return -1;

        // If the whole board is empty (Rocky plays first after winning coin toss),
        // optimal moves are Center (4) or any of the 4 corners (0, 2, 6, 8).
        if (availableMoves.Count == 9)
        {
            var bestOpenings = new[] { 0, 2, 4, 6, 8 };
            return bestOpenings[Rnd.Next(bestOpenings.Length)];
        }

        // PRIORITY 1: Can Rocky win immediately this turn? Take the knockout punch!
        foreach (var move in availableMoves)
        {
            board.SetCell(move, aiSymbol);
            if (board.CheckWinner() == aiSymbol)
            {
                board.ClearCell(move);
                return move;
            }
            board.ClearCell(move);
        }

        // PRIORITY 2: Can Human win next turn? Block it without mercy!
        foreach (var move in availableMoves)
        {
            board.SetCell(move, humanSymbol);
            if (board.CheckWinner() == humanSymbol)
            {
                board.ClearCell(move);
                return move;
            }
            board.ClearCell(move);
        }

        // PRIORITY 3: Full Minimax search with Alpha-Beta pruning
        int bestScore = int.MinValue;
        var topMoves = new List<int>();

        foreach (var move in availableMoves)
        {
            board.SetCell(move, aiSymbol);
            int score = Minimax(board, 0, false, aiSymbol, humanSymbol, int.MinValue, int.MaxValue);
            board.ClearCell(move);

            if (score > bestScore)
            {
                bestScore = score;
                topMoves.Clear();
                topMoves.Add(move);
            }
            else if (score == bestScore)
            {
                topMoves.Add(move);
            }
        }

        return topMoves[Rnd.Next(topMoves.Count)];
    }

    private int Minimax(Board board, int depth, bool isMaximizing, PlayerSymbol aiSymbol, PlayerSymbol humanSymbol, int alpha, int beta)
    {
        var winner = board.CheckWinner();
        if (winner == aiSymbol)
        {
            return 10 - depth; // Rocky wins (sooner is better)
        }
        if (winner == humanSymbol)
        {
            return depth - 10; // Human wins (later is better, though unreachable)
        }
        if (board.IsFull())
        {
            return 0; // Draw
        }

        var availableMoves = board.GetAvailableMoves();

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            foreach (var move in availableMoves)
            {
                board.SetCell(move, aiSymbol);
                int evaluation = Minimax(board, depth + 1, false, aiSymbol, humanSymbol, alpha, beta);
                board.ClearCell(move);

                maxEval = Math.Max(maxEval, evaluation);
                alpha = Math.Max(alpha, evaluation);
                if (beta <= alpha) break; // Alpha-beta cutoff
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in availableMoves)
            {
                board.SetCell(move, humanSymbol);
                int evaluation = Minimax(board, depth + 1, true, aiSymbol, humanSymbol, alpha, beta);
                board.ClearCell(move);

                minEval = Math.Min(minEval, evaluation);
                beta = Math.Min(beta, evaluation);
                if (beta <= alpha) break; // Alpha-beta cutoff
            }
            return minEval;
        }
    }
}
