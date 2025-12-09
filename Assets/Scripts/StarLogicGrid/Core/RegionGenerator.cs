// RegionGenerator.cs - Generates random contiguous regions for the puzzle
using System.Collections.Generic;
using UnityEngine;

namespace StarLogicGrid.Core
{
    /// <summary>
    /// Static utility class for generating random contiguous regions on the grid.
    /// Uses a flood-fill growth algorithm to create balanced, connected regions.
    /// </summary>
    public static class RegionGenerator
    {
        // Direction arrays for 4-way adjacency (up, down, left, right)
        private static readonly int[] DirR = { 0, 0, 1, -1 };
        private static readonly int[] DirC = { 1, -1, 0, 0 };
        
        /// <summary>
        /// Generates a grid of regions with the specified size and count.
        /// Each region will have an equal number of cells.
        /// </summary>
        /// <param name="gridSize">Size of the grid (e.g., 6 for 6x6)</param>
        /// <param name="regionCount">Number of distinct regions to create</param>
        /// <returns>2D array mapping each cell to its region ID, or null if generation fails</returns>
        public static int[,] GenerateRegions(int gridSize, int regionCount)
        {
            int[,] grid = null;
            int attempts = 0;
            
            // Try multiple times to generate valid regions
            while (grid == null && attempts < 200)
            {
                grid = TryGenerateRegions(gridSize, regionCount);
                attempts++;
            }
            
            if (grid == null)
            {
                Debug.LogWarning($"Failed to generate regions after {attempts} attempts");
            }
            
            return grid;
        }
        
        /// <summary>
        /// Single attempt to generate regions using flood-fill growth.
        /// </summary>
        private static int[,] TryGenerateRegions(int size, int nRegions)
        {
            int cellsPerRegion = (size * size) / nRegions;
            int[,] grid = new int[size, size];
            
            // Initialize all cells to -1 (unassigned)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    grid[r, c] = -1;
                }
            }
            
            int[] regionSizes = new int[nRegions];
            List<Vector2Int> availableCells = new List<Vector2Int>();
            
            // Create list of all cells
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    availableCells.Add(new Vector2Int(r, c));
                }
            }
            
            // Shuffle the cells for random seed placement
            ShuffleList(availableCells);
            
            // Place seed cells for each region
            List<Vector2Int> seeds = new List<Vector2Int>();
            for (int i = 0; i < nRegions; i++)
            {
                Vector2Int seed = availableCells[i];
                grid[seed.x, seed.y] = i;
                regionSizes[i]++;
                seeds.Add(seed);
            }
            
            // Candidates for region expansion: (row, col, regionId)
            List<(int r, int c, int regionId)> candidates = new List<(int, int, int)>();
            
            // Initialize candidates from all seeds
            for (int i = 0; i < seeds.Count; i++)
            {
                AddCandidates(grid, candidates, seeds[i].x, seeds[i].y, i, size);
            }
            
            // Grow regions by adding adjacent cells
            while (candidates.Count > 0)
            {
                // Filter to only candidates for regions that aren't full yet
                var validCandidates = candidates.FindAll(c => 
                    regionSizes[c.regionId] < cellsPerRegion);
                
                if (validCandidates.Count == 0) break;
                
                // Pick a random valid candidate
                int idx = Random.Range(0, validCandidates.Count);
                var choice = validCandidates[idx];
                
                // Assign cell to region if not already assigned
                if (grid[choice.r, choice.c] == -1)
                {
                    grid[choice.r, choice.c] = choice.regionId;
                    regionSizes[choice.regionId]++;
                    
                    // Add new candidates from this cell
                    AddCandidates(grid, candidates, choice.r, choice.c, 
                        choice.regionId, size);
                }
                
                // Remove this cell from all candidate lists
                candidates.RemoveAll(c => c.r == choice.r && c.c == choice.c);
            }
            
            // Verify all regions have the correct size
            foreach (int regionSize in regionSizes)
            {
                if (regionSize != cellsPerRegion)
                {
                    return null; // Failed - try again
                }
            }
            
            return grid;
        }
        
        /// <summary>
        /// Adds adjacent unassigned cells as candidates for a region.
        /// </summary>
        private static void AddCandidates(int[,] grid, 
            List<(int r, int c, int regionId)> candidates,
            int r, int c, int regionId, int size)
        {
            for (int d = 0; d < 4; d++)
            {
                int nr = r + DirR[d];
                int nc = c + DirC[d];
                
                // Check bounds and if cell is unassigned
                if (nr >= 0 && nr < size && nc >= 0 && nc < size && 
                    grid[nr, nc] == -1)
                {
                    // Check if this candidate already exists for this region
                    bool exists = candidates.Exists(cand => 
                        cand.r == nr && cand.c == nc && cand.regionId == regionId);
                    
                    if (!exists)
                    {
                        candidates.Add((nr, nc, regionId));
                    }
                }
            }
        }
        
        /// <summary>
        /// Fisher-Yates shuffle for randomizing a list.
        /// </summary>
        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
