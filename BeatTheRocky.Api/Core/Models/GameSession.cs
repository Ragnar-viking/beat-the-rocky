using BeatTheRocky.Api.Core.Enums;

namespace BeatTheRocky.Api.Core.Models;

public class GameSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Board Board { get; set; } = new();
    public PlayerSymbol HumanSymbol { get; set; } = PlayerSymbol.Empty;
    public PlayerSymbol RockySymbol { get; set; } = PlayerSymbol.Empty;
    public PlayerSymbol CurrentTurn { get; set; } = PlayerSymbol.Empty;
    public GameStatus Status { get; set; } = GameStatus.AwaitingCoinToss;
    public CoinChoice? UserCoinBet { get; set; }
    public CoinResult? TossResult { get; set; }
    public bool? UserWonToss { get; set; }
    public int[]? WinningCombination { get; set; }
    public string RockyComment { get; set; } = "Step into the ring, kid! Let's toss the coin to see who gets the first punch.";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
