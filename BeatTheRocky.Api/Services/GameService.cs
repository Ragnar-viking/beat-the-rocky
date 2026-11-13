using BeatTheRocky.Api.Core.Enums;
using BeatTheRocky.Api.Core.Models;
using BeatTheRocky.Api.Factories;
using BeatTheRocky.Api.Repositories;
using BeatTheRocky.Api.Strategies;

namespace BeatTheRocky.Api.Services;

public class GameService : IGameService
{
    private readonly IGameSessionRepository _repository;
    private readonly IAiStrategy _aiStrategy;
    private readonly IGameFactory _factory;
    private readonly ILogger<GameService> _logger;

    private static readonly string[] RockyTaunts = new[]
    {
        "Nice try, kid! But I run this ring.",
        "You thought I didn't see that coming?",
        "Float like a butterfly, sting like a 3-in-a-row!",
        "Every move you make is already in my game plan.",
        "You're stepping into the danger zone!",
        "Keep your guard up, rookie!"
    };

    private static readonly string[] RockyWinQuotes = new[]
    {
        "KNOCKOUT! 🥊 Rocky remains undefeated champion!",
        "Down goes Frazier! Down goes the challenger!",
        "Ain't nobody beating Rocky in this ring! Better luck next time!",
        "And the referee waves it off! A clean knockout!",
        "Champion of the world! Did you really think you could beat me?"
    };

    private static readonly string[] RockyDrawQuotes = new[]
    {
        "DING DING! You survived all rounds, but you still didn't beat Rocky!",
        "A draw! You fought with heart, kid, but the belt stays right here.",
        "Good defense! But the undefeated streak of Rocky remains unbroken!"
    };

    public GameService(
        IGameSessionRepository repository,
        IAiStrategy aiStrategy,
        IGameFactory factory,
        ILogger<GameService> logger)
    {
        _repository = repository;
        _aiStrategy = aiStrategy;
        _factory = factory;
        _logger = logger;
    }

    public GameSession CreateSession()
    {
        var session = _factory.CreateNewSession();
        _repository.Save(session);
        _logger.LogInformation("Created new game session {SessionId}", session.Id);
        return session;
    }

    public GameSession ProcessToss(Guid sessionId, CoinChoice userChoice)
    {
        var session = _repository.Get(sessionId);
        if (session == null)
        {
            throw new KeyNotFoundException($"Session {sessionId} was not found.");
        }

        // Determine coin flip result (0 = Heads, 1 = Tails)
        var actualToss = (CoinResult)Random.Shared.Next(0, 2);
        bool userWonToss = (userChoice == CoinChoice.Heads && actualToss == CoinResult.Heads) ||
                           (userChoice == CoinChoice.Tails && actualToss == CoinResult.Tails);

        session.UserCoinBet = userChoice;
        session.TossResult = actualToss;
        session.UserWonToss = userWonToss;
        session.Status = GameStatus.InProgress;
        session.Board = new Board();
        session.WinningCombination = null;

        if (userWonToss)
        {
            // User won toss -> User plays 'X' and takes the first turn
            session.HumanSymbol = PlayerSymbol.X;
            session.RockySymbol = PlayerSymbol.O;
            session.CurrentTurn = PlayerSymbol.X;
            session.RockyComment = $"Coin landed on {actualToss}! You won the toss, kid. Step up and make your first punch!";
        }
        else
        {
            // Rocky won toss -> Rocky plays 'X' and takes the first turn
            session.RockySymbol = PlayerSymbol.X;
            session.HumanSymbol = PlayerSymbol.O;
            session.CurrentTurn = PlayerSymbol.X;

            // Rocky makes his opening move immediately
            int rockyOpening = _aiStrategy.CalculateBestMove(session.Board, session.RockySymbol, session.HumanSymbol);
            session.Board.SetCell(rockyOpening, session.RockySymbol);

            // Turn shifts to user
            session.CurrentTurn = session.HumanSymbol;
            session.RockyComment = $"Coin landed on {actualToss}! Rocky won the toss! I've placed my first punch. Your turn, challenger!";
        }

        _repository.Save(session);
        return session;
    }

    public GameSession MakeMove(Guid sessionId, int cellIndex)
    {
        var session = _repository.Get(sessionId);
        if (session == null)
        {
            throw new KeyNotFoundException($"Session {sessionId} was not found.");
        }

        if (session.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException($"Cannot make move: Game is currently {session.Status}.");
        }

        if (session.CurrentTurn != session.HumanSymbol)
        {
            throw new InvalidOperationException("It is not your turn!");
        }

        if (cellIndex < 0 || cellIndex > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(cellIndex), "Cell index must be between 0 and 8.");
        }

        if (session.Board.GetCell(cellIndex) != PlayerSymbol.Empty)
        {
            throw new InvalidOperationException("Cell is already occupied.");
        }

        // Apply Player's Move
        session.Board.SetCell(cellIndex, session.HumanSymbol);

        // Check if Human Won (theoretically impossible with Minimax, but verified correctly)
        var winner = session.Board.CheckWinner();
        if (winner == session.HumanSymbol)
        {
            session.Status = GameStatus.PlayerWon;
            session.WinningCombination = session.Board.GetWinningCombination();
            session.RockyComment = "WHAT?! Impossible! You must've cracked the code!";
            _repository.Save(session);
            return session;
        }

        // Check if board full after Human's move (Draw)
        if (session.Board.IsFull())
        {
            session.Status = GameStatus.Draw;
            session.RockyComment = RockyDrawQuotes[Random.Shared.Next(RockyDrawQuotes.Length)];
            _repository.Save(session);
            return session;
        }

        // Rocky's Turn
        session.CurrentTurn = session.RockySymbol;
        int rockyMove = _aiStrategy.CalculateBestMove(session.Board, session.RockySymbol, session.HumanSymbol);

        if (rockyMove >= 0)
        {
            session.Board.SetCell(rockyMove, session.RockySymbol);
        }

        // Check if Rocky Won
        winner = session.Board.CheckWinner();
        if (winner == session.RockySymbol)
        {
            session.Status = GameStatus.RockyWon;
            session.WinningCombination = session.Board.GetWinningCombination();
            session.RockyComment = RockyWinQuotes[Random.Shared.Next(RockyWinQuotes.Length)];
            _repository.Save(session);
            return session;
        }

        // Check if board is full after Rocky's move (Draw)
        if (session.Board.IsFull())
        {
            session.Status = GameStatus.Draw;
            session.RockyComment = RockyDrawQuotes[Random.Shared.Next(RockyDrawQuotes.Length)];
            _repository.Save(session);
            return session;
        }

        // Game continues
        session.CurrentTurn = session.HumanSymbol;
        session.RockyComment = RockyTaunts[Random.Shared.Next(RockyTaunts.Length)];

        _repository.Save(session);
        return session;
    }

    public GameSession? GetSession(Guid sessionId)
    {
        return _repository.Get(sessionId);
    }

    public GameSession ResetSession(Guid sessionId)
    {
        var session = _repository.Get(sessionId);
        if (session == null)
        {
            session = _factory.CreateNewSession();
        }
        else
        {
            session.Board = new Board();
            session.Status = GameStatus.AwaitingCoinToss;
            session.HumanSymbol = PlayerSymbol.Empty;
            session.RockySymbol = PlayerSymbol.Empty;
            session.CurrentTurn = PlayerSymbol.Empty;
            session.UserCoinBet = null;
            session.TossResult = null;
            session.UserWonToss = null;
            session.WinningCombination = null;
            session.RockyComment = "Rematch time! Call Heads or Tails again to see who gets the opening punch.";
        }

        _repository.Save(session);
        return session;
    }
}
