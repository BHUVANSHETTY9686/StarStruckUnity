// PuzzleSolver.cs - Backtracking solver for Star Logic Grid puzzles
using StarLogicGrid.Data;

namespace StarLogicGrid.Core
{
    /// <summary>
    /// Static utility class that solves Star Logic Grid puzzles using backtracking.
    /// Finds a valid star placement that satisfies all constraints.
    /// </summary>
    public static class PuzzleSolver
    {
        /// <summary>
        /// Attempts to solve the puzzle and populate the solution grid.
        /// </summary>
        /// <param name="puzzle">The puzzle data containing regions</param>
        /// <returns>True if a solution was found, false otherwise</returns>
        public static bool Solve(PuzzleData puzzle)
        {
            // Clear the solution grid
            for (int r = 0; r < puzzle.gridSize; r++)
            {
                for (int c = 0; c < puzzle.gridSize; c++)
                {
                    puzzle.solutionGrid[r, c] = 0;
                }
            }
            
            return SolveRecursive(puzzle, 0);
        }
        
        /// <summary>
        /// Recursive backtracking solver.
        /// </summary>
        /// <param name="puzzle">Current puzzle state</param>
        /// <param name="starIndex">Number of stars placed so far</param>
        /// <returns>True if solution found from this state</returns>
        private static bool SolveRecursive(PuzzleData puzzle, int starIndex)
        {
            int totalStarsNeeded = puzzle.gridSize * puzzle.starsPerArea;
            
            // Base case: all stars placed successfully
            if (starIndex == totalStarsNeeded)
            {
                return true;
            }
            
            // Try placing a star in each empty cell
            for (int r = 0; r < puzzle.gridSize; r++)
            {
                for (int c = 0; c < puzzle.gridSize; c++)
                {
                    if (puzzle.solutionGrid[r, c] == 0 && 
                        IsSafePlacement(puzzle, r, c))
                    {
                        // Place star
                        puzzle.solutionGrid[r, c] = 1;
                        
                        // Recurse
                        if (SolveRecursive(puzzle, starIndex + 1))
                        {
                            return true;
                        }
                        
                        // Backtrack
                        puzzle.solutionGrid[r, c] = 0;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if placing a star at the given position is valid.
        /// Validates row, column, region, and adjacency constraints.
        /// </summary>
        private static bool IsSafePlacement(PuzzleData puzzle, int r, int c)
        {
            int size = puzzle.gridSize;
            
            // Check row constraint
            int rowStars = 0;
            for (int col = 0; col < size; col++)
            {
                if (puzzle.solutionGrid[r, col] == 1)
                {
                    rowStars++;
                }
            }
            if (rowStars >= puzzle.starsPerArea)
            {
                return false;
            }
            
            // Check column constraint
            int colStars = 0;
            for (int row = 0; row < size; row++)
            {
                if (puzzle.solutionGrid[row, c] == 1)
                {
                    colStars++;
                }
            }
            if (colStars >= puzzle.starsPerArea)
            {
                return false;
            }
            
            // Check region constraint
            int regionId = puzzle.regions[r, c];
            int regionStars = 0;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (puzzle.regions[row, col] == regionId && 
                        puzzle.solutionGrid[row, col] == 1)
                    {
                        regionStars++;
                    }
                }
            }
            if (regionStars >= puzzle.starsPerArea)
            {
                return false;
            }
            
            // Check adjacency constraint (no touching, including diagonals)
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    
                    int nr = r + dr;
                    int nc = c + dc;
                    
                    if (nr >= 0 && nr < size && nc >= 0 && nc < size)
                    {
                        if (puzzle.solutionGrid[nr, nc] == 1)
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Validates if the current user grid state is valid (no rule violations).
        /// </summary>
        public static bool ValidateUserGrid(PuzzleData puzzle)
        {
            int size = puzzle.gridSize;
            
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (puzzle.userGrid[r, c] == CellState.Star)
                    {
                        if (!IsValidUserPlacement(puzzle, r, c))
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if a user-placed star at the given position is valid.
        /// </summary>
        public static bool IsValidUserPlacement(PuzzleData puzzle, int r, int c)
        {
            int size = puzzle.gridSize;
            
            // Check adjacency (no touching stars)
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    
                    int nr = r + dr;
                    int nc = c + dc;
                    
                    if (nr >= 0 && nr < size && nc >= 0 && nc < size)
                    {
                        if (puzzle.userGrid[nr, nc] == CellState.Star)
                        {
                            return false;
                        }
                    }
                }
            }
            
            // Check row count
            int rowStars = CountStarsInRow(puzzle, r);
            if (rowStars > puzzle.starsPerArea) return false;
            
            // Check column count
            int colStars = CountStarsInColumn(puzzle, c);
            if (colStars > puzzle.starsPerArea) return false;
            
            // Check region count
            int regionStars = CountStarsInRegion(puzzle, puzzle.regions[r, c]);
            if (regionStars > puzzle.starsPerArea) return false;
            
            return true;
        }
        
        /// <summary>
        /// Counts stars in a specific row.
        /// </summary>
        public static int CountStarsInRow(PuzzleData puzzle, int row)
        {
            int count = 0;
            for (int c = 0; c < puzzle.gridSize; c++)
            {
                if (puzzle.userGrid[row, c] == CellState.Star)
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// Counts stars in a specific column.
        /// </summary>
        public static int CountStarsInColumn(PuzzleData puzzle, int col)
        {
            int count = 0;
            for (int r = 0; r < puzzle.gridSize; r++)
            {
                if (puzzle.userGrid[r, col] == CellState.Star)
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// Counts stars in a specific region.
        /// </summary>
        public static int CountStarsInRegion(PuzzleData puzzle, int regionId)
        {
            int count = 0;
            int size = puzzle.gridSize;
            
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (puzzle.regions[r, c] == regionId && 
                        puzzle.userGrid[r, c] == CellState.Star)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        
        /// <summary>
        /// Counts total stars placed by the user.
        /// </summary>
        public static int CountTotalStars(PuzzleData puzzle)
        {
            int count = 0;
            int size = puzzle.gridSize;
            
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (puzzle.userGrid[r, c] == CellState.Star)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
