using BeatTheRocky.Api.Core.Enums;
using BeatTheRocky.Api.Core.Models;
using BeatTheRocky.Api.Factories;
using BeatTheRocky.Api.Repositories;
using BeatTheRocky.Api.Services;
using BeatTheRocky.Api.Strategies;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BeatTheRocky.Tests;

public class UnbeatableAiTests
{
    private readonly UnbeatableMinimaxStrategy _ai = new();

    [Fact]
    public void Rocky_Takes_Immediate_Winning_Move()
    {
        // Arrange
        // Rocky has [0] and [1], [2] is empty
        var board = new Board();
        board.SetCell(0, PlayerSymbol.X);
        board.SetCell(1, PlayerSymbol.X);
        board.SetCell(3, PlayerSymbol.O);
        board.SetCell(4, PlayerSymbol.O);

        // Act
        int bestMove = _ai.CalculateBestMove(board, PlayerSymbol.X, PlayerSymbol.O);

        // Assert - Rocky must take cell 2 for instant win
        Assert.Equal(2, bestMove);
    }

    [Fact]
    public void Rocky_Blocks_Human_Immediate_Winning_Threat()
    {
        // Arrange
        // Human has [0] and [3], [6] is empty (vertical win threat)
        var board = new Board();
        board.SetCell(0, PlayerSymbol.O);
        board.SetCell(3, PlayerSymbol.O);
        board.SetCell(4, PlayerSymbol.X); // Rocky in center

        // Act
        int bestMove = _ai.CalculateBestMove(board, PlayerSymbol.X, PlayerSymbol.O);

        // Assert - Rocky must block at cell 6
        Assert.Equal(6, bestMove);
    }

    [Fact]
    public void Rocky_Chooses_Optimal_Opening_On_Empty_Board()
    {
        // Arrange
        var board = new Board();
        var optimalMoves = new HashSet<int> { 0, 2, 4, 6, 8 };

        // Act
        for (int i = 0; i < 20; i++)
        {
            int move = _ai.CalculateBestMove(board, PlayerSymbol.X, PlayerSymbol.O);
            // Assert
            Assert.Contains(move, optimalMoves);
        }
    }

    [Fact]
    public void Rocky_Never_Loses_Against_Simulated_Opponents()
    {
        // Simulate 200 matches against randomized moves
        var random = new Random(42);

        for (int match = 0; match < 200; match++)
        {
            var board = new Board();
            // Alternate who goes first
            bool rockyTurn = (match % 2 == 0);
            var rockySymbol = rockyTurn ? PlayerSymbol.X : PlayerSymbol.O;
            var humanSymbol = rockyTurn ? PlayerSymbol.O : PlayerSymbol.X;

            while (!board.IsFull() && board.CheckWinner() == PlayerSymbol.Empty)
            {
                if (rockyTurn)
                {
                    int move = _ai.CalculateBestMove(board, rockySymbol, humanSymbol);
                    Assert.True(board.SetCell(move, rockySymbol));
                }
                else
                {
                    // Opponent chooses random available move
                    var available = board.GetAvailableMoves();
                    int move = available[random.Next(available.Count)];
                    Assert.True(board.SetCell(move, humanSymbol));
                }

                rockyTurn = !rockyTurn;
            }

            var winner = board.CheckWinner();
            // Rocky must NEVER lose!
            Assert.NotEqual(humanSymbol, winner);
        }
    }

    [Fact]
    public void GameService_Toss_Assigns_Turns_Correctly()
    {
        var repo = new InMemoryGameSessionRepository();
        var factory = new GameFactory();
        var service = new GameService(repo, _ai, factory, NullLogger<GameService>.Instance);

        var session = service.CreateSession();
        Assert.Equal(GameStatus.AwaitingCoinToss, session.Status);

        // Perform toss
        var updated = service.ProcessToss(session.Id, CoinChoice.Heads);

        Assert.Equal(GameStatus.InProgress, updated.Status);
        Assert.NotNull(updated.TossResult);
        Assert.NotNull(updated.UserWonToss);

        if (updated.UserWonToss.Value)
        {
            Assert.Equal(PlayerSymbol.X, updated.HumanSymbol);
            Assert.Equal(PlayerSymbol.O, updated.RockySymbol);
            Assert.Equal(PlayerSymbol.X, updated.CurrentTurn);
        }
        else
        {
            Assert.Equal(PlayerSymbol.X, updated.RockySymbol);
            Assert.Equal(PlayerSymbol.O, updated.HumanSymbol);
            // Rocky should have already made his first move
            Assert.Equal(PlayerSymbol.O, updated.CurrentTurn);
            Assert.Contains(PlayerSymbol.X, updated.Board.GetCells());
        }
    }
}
