// PuzzleData.cs - Data class holding all puzzle state information
using System;

namespace StarLogicGrid.Data
{
    /// <summary>
    /// Contains all data for a Star Logic Grid puzzle instance.
    /// Stores grid configuration, regions, current user state, and solution.
    /// </summary>
    [Serializable]
    public class PuzzleData
    {
        /// <summary>Size of the grid (e.g., 5 for 5x5, 6 for 6x6)</summary>
        public int gridSize;
        
        /// <summary>Number of stars required per row/column/region</summary>
        public int starsPerArea;
        
        /// <summary>Total number of distinct regions in the puzzle</summary>
        public int regionCount;
        
        /// <summary>2D array mapping each cell to its region ID</summary>
        public int[,] regions;
        
        /// <summary>Current state of each cell as set by the user</summary>
        public CellState[,] userGrid;
        
        /// <summary>Solution grid (1 = star, 0 = empty)</summary>
        public int[,] solutionGrid;
        
        /// <summary>
        /// Creates a new puzzle data instance with the specified grid size.
        /// </summary>
        /// <param name="size">The size of the grid (e.g., 5, 6, or 8)</param>
        public PuzzleData(int size)
        {
            gridSize = size;
            starsPerArea = 1; // Always 1 star per row/column/region in this variant
            regionCount = size; // Number of regions equals grid size
            
            regions = new int[size, size];
            userGrid = new CellState[size, size];
            solutionGrid = new int[size, size];
            
            // Initialize all cells to empty
            Reset();
        }
        
        /// <summary>
        /// Resets the user grid to all empty cells.
        /// Does not affect regions or solution.
        /// </summary>
        public void Reset()
        {
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    userGrid[r, c] = CellState.Empty;
                }
            }
        }
        
        /// <summary>
        /// Gets the region ID for a specific cell.
        /// </summary>
        public int GetRegion(int row, int col)
        {
            if (row >= 0 && row < gridSize && col >= 0 && col < gridSize)
            {
                return regions[row, col];
            }
            return -1;
        }
        
        /// <summary>
        /// Checks if coordinates are within grid bounds.
        /// </summary>
        public bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < gridSize && col >= 0 && col < gridSize;
        }
    }
}
