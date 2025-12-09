// CellState.cs - Enum representing the state of each cell in the puzzle
namespace StarLogicGrid.Data
{
    /// <summary>
    /// Represents the possible states of a cell in the Star Logic Grid puzzle.
    /// </summary>
    public enum CellState
    {
        /// <summary>Empty cell with no marking</summary>
        Empty = 0,
        
        /// <summary>Cell contains a star</summary>
        Star = 1,
        
        /// <summary>Cell marked with plus by player (potential star location)</summary>
        MarkPlus = 2,
        
        /// <summary>Cell marked with X (auto-generated conflict marker)</summary>
        MarkX = 3
    }
}
