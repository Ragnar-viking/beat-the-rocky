using BeatTheRocky.Api.Core.Enums;

namespace BeatTheRocky.Api.Core.Models;

public class Board
{
    private readonly PlayerSymbol[] _cells;

    public static readonly int[][] WinningCombinations = new int[][]
    {
        new[] { 0, 1, 2 }, // Row 0
        new[] { 3, 4, 5 }, // Row 1
        new[] { 6, 7, 8 }, // Row 2
        new[] { 0, 3, 6 }, // Col 0
        new[] { 1, 4, 7 }, // Col 1
        new[] { 2, 5, 8 }, // Col 2
        new[] { 0, 4, 8 }, // Diag Main
        new[] { 2, 4, 6 }  // Diag Anti
    };

    public Board()
    {
        _cells = new PlayerSymbol[9];
        Array.Fill(_cells, PlayerSymbol.Empty);
    }

    private Board(PlayerSymbol[] cells)
    {
        _cells = (PlayerSymbol[])cells.Clone();
    }

    public PlayerSymbol GetCell(int index)
    {
        if (index < 0 || index > 8) throw new ArgumentOutOfRangeException(nameof(index));
        return _cells[index];
    }

    public bool SetCell(int index, PlayerSymbol symbol)
    {
        if (index < 0 || index > 8) return false;
        if (_cells[index] != PlayerSymbol.Empty && symbol != PlayerSymbol.Empty) return false;
        _cells[index] = symbol;
        return true;
    }

    public void ClearCell(int index)
    {
        if (index >= 0 && index <= 8)
        {
            _cells[index] = PlayerSymbol.Empty;
        }
    }

    public List<int> GetAvailableMoves()
    {
        var moves = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            if (_cells[i] == PlayerSymbol.Empty)
            {
                moves.Add(i);
            }
        }
        return moves;
    }

    public bool IsFull()
    {
        return _cells.All(c => c != PlayerSymbol.Empty);
    }

    public PlayerSymbol CheckWinner()
    {
        foreach (var combo in WinningCombinations)
        {
            var first = _cells[combo[0]];
            if (first != PlayerSymbol.Empty &&
                first == _cells[combo[1]] &&
                first == _cells[combo[2]])
            {
                return first;
            }
        }
        return PlayerSymbol.Empty;
    }

    public int[]? GetWinningCombination()
    {
        foreach (var combo in WinningCombinations)
        {
            var first = _cells[combo[0]];
            if (first != PlayerSymbol.Empty &&
                first == _cells[combo[1]] &&
                first == _cells[combo[2]])
            {
                return combo;
            }
        }
        return null;
    }

    public PlayerSymbol[] GetCells() => (PlayerSymbol[])_cells.Clone();

    public Board Clone()
    {
        return new Board(_cells);
    }
}
