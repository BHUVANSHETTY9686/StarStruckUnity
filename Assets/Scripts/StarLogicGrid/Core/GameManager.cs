// GameManager.cs - Main game controller for Star Logic Grid
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarLogicGrid.Data;
using StarLogicGrid.UI;

namespace StarLogicGrid.Core
{
    /// <summary>
    /// Main game controller that manages game state, user interactions,
    /// and coordinates between UI and game logic.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Managers")]
        [Tooltip("Reference to the GridManager component")]
        [SerializeField] private GridManager gridManager;
        
        [Header("UI Elements - Dropdowns")]
        [Tooltip("Dropdown for selecting grid size")]
        [SerializeField] private TMP_Dropdown gridSizeDropdown;
        
        [Tooltip("Dropdown for selecting difficulty")]
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        
        [Header("UI Elements - Text")]
        [Tooltip("Text element for status messages")]
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("UI Elements - Buttons")]
        [Tooltip("Button to generate new puzzle")]
        [SerializeField] private Button newPuzzleButton;
        
        [Tooltip("Button to reset current puzzle")]
        [SerializeField] private Button resetButton;
        
        [Tooltip("Button to get a hint")]
        [SerializeField] private Button hintButton;
        
        [Tooltip("Button to check solution")]
        [SerializeField] private Button checkButton;
        
        [Tooltip("Button to reveal answer")]
        [SerializeField] private Button revealButton;
        
        [Header("Status Colors")]
        [SerializeField] private Color normalStatusColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color successStatusColor = new Color(0.1f, 0.6f, 0.3f);
        [SerializeField] private Color errorStatusColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color warningStatusColor = new Color(0.8f, 0.6f, 0.1f);
        
        // Game state
        private PuzzleData currentPuzzle;
        private bool isGameActive;
        private int hintsUsed;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            SetupUI();
            SetupDropdowns();
            UpdateButtonStates();
            UpdateStatus("Select grid size and difficulty, then click 'New Puzzle'!", normalStatusColor);
        }
        
        #endregion
        
        #region UI Setup
        
        /// <summary>
        /// Sets up button click listeners.
        /// </summary>
        private void SetupUI()
        {
            if (newPuzzleButton != null)
                newPuzzleButton.onClick.AddListener(NewPuzzle);
            
            if (resetButton != null)
                resetButton.onClick.AddListener(ResetPuzzle);
            
            if (hintButton != null)
                hintButton.onClick.AddListener(GetHint);
            
            if (checkButton != null)
                checkButton.onClick.AddListener(CheckSolution);
            
            if (revealButton != null)
                revealButton.onClick.AddListener(RevealAnswer);
        }
        
        /// <summary>
        /// Sets up dropdown options.
        /// </summary>
        private void SetupDropdowns()
        {
            // Setup grid size dropdown
            if (gridSizeDropdown != null)
            {
                gridSizeDropdown.ClearOptions();
                gridSizeDropdown.AddOptions(new System.Collections.Generic.List<string> 
                { 
                    "5×5", 
                    "6×6", 
                    "8×8" 
                });
                gridSizeDropdown.value = 1; // Default to 6x6
            }
            
            // Setup difficulty dropdown
            if (difficultyDropdown != null)
            {
                difficultyDropdown.ClearOptions();
                difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Easy",
                    "Medium",
                    "Hard"
                });
                difficultyDropdown.value = 1; // Default to Medium
            }
        }
        
        /// <summary>
        /// Gets the selected grid size from dropdown.
        /// </summary>
        private int GetSelectedGridSize()
        {
            if (gridSizeDropdown == null) return 6;
            
            return gridSizeDropdown.value switch
            {
                0 => 5,
                1 => 6,
                2 => 8,
                _ => 6
            };
        }
        
        /// <summary>
        /// Gets the selected difficulty name.
        /// </summary>
        private string GetSelectedDifficulty()
        {
            if (difficultyDropdown == null) return "Medium";
            
            return difficultyDropdown.value switch
            {
                0 => "Easy",
                1 => "Medium",
                2 => "Hard",
                _ => "Medium"
            };
        }
        
        #endregion
        
        #region Game Actions
        
        /// <summary>
        /// Generates a new puzzle.
        /// </summary>
        public void NewPuzzle()
        {
            int gridSize = GetSelectedGridSize();
            string difficulty = GetSelectedDifficulty();
            
            UpdateStatus("Generating puzzle...", normalStatusColor);
            
            // Create new puzzle data
            currentPuzzle = new PuzzleData(gridSize);
            bool success = false;
            int attempts = 0;
            
            // Try to generate a valid, solvable puzzle
            while (!success && attempts < 100)
            {
                var regions = RegionGenerator.GenerateRegions(gridSize, gridSize);
                
                if (regions != null)
                {
                    currentPuzzle.regions = regions;
                    
                    if (PuzzleSolver.Solve(currentPuzzle))
                    {
                        success = true;
                    }
                }
                attempts++;
            }
            
            if (!success)
            {
                UpdateStatus("Failed to generate puzzle. Please try again!", errorStatusColor);
                return;
            }
            
            // Reset user grid and start game
            currentPuzzle.Reset();
            isGameActive = true;
            hintsUsed = 0;
            
            // Setup the visual grid
            gridManager.SetupGrid(currentPuzzle, OnCellClicked);
            
            UpdateButtonStates();
            UpdateStatus($"{gridSize}×{gridSize} {difficulty} puzzle ready. Good luck! ⭐", successStatusColor);
        }
        
        /// <summary>
        /// Resets the current puzzle to initial state.
        /// </summary>
        public void ResetPuzzle()
        {
            if (currentPuzzle == null)
            {
                UpdateStatus("No puzzle to reset!", warningStatusColor);
                return;
            }
            
            currentPuzzle.Reset();
            RegenerateAutoMarks();
            gridManager.UpdateAllCells();
            isGameActive = true;
            
            UpdateButtonStates();
            UpdateStatus("Puzzle reset! Try again.", warningStatusColor);
        }
        
        /// <summary>
        /// Provides a hint by placing a correct star.
        /// </summary>
        public void GetHint()
        {
            if (!isGameActive || currentPuzzle == null)
            {
                return;
            }
            
            int size = currentPuzzle.gridSize;
            
            // Find a cell where solution has a star but user doesn't
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (currentPuzzle.solutionGrid[r, c] == 1 && 
                        currentPuzzle.userGrid[r, c] != CellState.Star)
                    {
                        // Place the star
                        currentPuzzle.userGrid[r, c] = CellState.Star;
                        hintsUsed++;
                        
                        RegenerateAutoMarks();
                        gridManager.UpdateAllCells();
                        
                        // Play hint animation
                        var cellView = gridManager.GetCellView(r, c);
                        if (cellView != null)
                        {
                            cellView.PlayHintAnimation();
                        }
                        
                        UpdateStatus($"💡 Hint {hintsUsed}: Star placed at row {r + 1}, column {c + 1}", successStatusColor);
                        
                        // Check if puzzle is now complete
                        CheckSolutionInternal(false);
                        return;
                    }
                }
            }
            
            UpdateStatus("All stars already placed! 🎉", successStatusColor);
        }
        
        /// <summary>
        /// Checks if the current solution is correct.
        /// </summary>
        public void CheckSolution()
        {
            CheckSolutionInternal(true);
        }
        
        /// <summary>
        /// Internal solution check with optional error highlighting.
        /// </summary>
        private void CheckSolutionInternal(bool showErrors)
        {
            if (!isGameActive || currentPuzzle == null) return;
            
            int totalStars = PuzzleSolver.CountTotalStars(currentPuzzle);
            int requiredStars = currentPuzzle.gridSize * currentPuzzle.starsPerArea;
            bool rulesValid = PuzzleSolver.ValidateUserGrid(currentPuzzle);
            
            if (totalStars == requiredStars && rulesValid)
            {
                // Puzzle solved!
                isGameActive = false;
                UpdateButtonStates();
                UpdateStatus("🎉 CONGRATULATIONS! Puzzle solved! 🎉", successStatusColor);
            }
            else if (!rulesValid)
            {
                UpdateStatus("⚠️ Some stars conflict! Check the red stars.", errorStatusColor);
                if (showErrors)
                {
                    gridManager.HighlightErrors();
                }
            }
            else
            {
                UpdateStatus($"Progress: {totalStars}/{requiredStars} stars placed ⭐", normalStatusColor);
            }
        }
        
        /// <summary>
        /// Reveals the complete solution.
        /// </summary>
        public void RevealAnswer()
        {
            if (currentPuzzle == null)
            {
                UpdateStatus("No puzzle loaded!", errorStatusColor);
                return;
            }
            
            int size = currentPuzzle.gridSize;
            
            // Copy solution to user grid
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    currentPuzzle.userGrid[r, c] = (currentPuzzle.solutionGrid[r, c] == 1) 
                        ? CellState.Star 
                        : CellState.Empty;
                }
            }
            
            isGameActive = false;
            gridManager.UpdateAllCells();
            UpdateButtonStates();
            UpdateStatus("👁️ Answer revealed. Click 'New Puzzle' to play again.", warningStatusColor);
        }
        
        #endregion
        
        #region Cell Interaction
        
        /// <summary>
        /// Handles cell click events.
        /// </summary>
        private void OnCellClicked(int row, int col)
        {
            if (!isGameActive || currentPuzzle == null) return;
            
            CellState currentState = currentPuzzle.userGrid[row, col];
            
            // Treat auto-marked X as empty for cycling purposes
            CellState effectiveState = (currentState == CellState.MarkX) 
                ? CellState.Empty 
                : currentState;
            
            // Cycle through states: Empty -> Plus -> Star -> Empty
            CellState newState = effectiveState switch
            {
                CellState.Empty => CellState.MarkPlus,
                CellState.MarkPlus => CellState.Star,
                CellState.Star => CellState.Empty,
                _ => CellState.Empty
            };
            
            currentPuzzle.userGrid[row, col] = newState;
            
            // Regenerate conflict markers
            RegenerateAutoMarks();
            
            // Update display
            gridManager.UpdateAllCells();
            
            // Check solution status
            CheckSolutionInternal(false);
        }
        
        #endregion
        
        #region Auto-Marking
        
        /// <summary>
        /// Regenerates all automatic X markers based on placed stars.
        /// </summary>
        private void RegenerateAutoMarks()
        {
            int size = currentPuzzle.gridSize;
            
            // First, clear all auto-marks (MarkX cells)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (currentPuzzle.userGrid[r, c] == CellState.MarkX)
                    {
                        currentPuzzle.userGrid[r, c] = CellState.Empty;
                    }
                }
            }
            
            // Then, apply conflict marks for each star
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (currentPuzzle.userGrid[r, c] == CellState.Star)
                    {
                        ApplyStarConflictMarks(r, c);
                    }
                }
            }
        }
        
        /// <summary>
        /// Marks cells that conflict with a star at the given position.
        /// </summary>
        private void ApplyStarConflictMarks(int starRow, int starCol)
        {
            int size = currentPuzzle.gridSize;
            
            // Mark entire row
            for (int c = 0; c < size; c++)
            {
                if (c != starCol && currentPuzzle.userGrid[starRow, c] == CellState.Empty)
                {
                    currentPuzzle.userGrid[starRow, c] = CellState.MarkX;
                }
            }
            
            // Mark entire column
            for (int r = 0; r < size; r++)
            {
                if (r != starRow && currentPuzzle.userGrid[r, starCol] == CellState.Empty)
                {
                    currentPuzzle.userGrid[r, starCol] = CellState.MarkX;
                }
            }
            
            // Mark adjacent cells (including diagonals)
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    
                    int nr = starRow + dr;
                    int nc = starCol + dc;
                    
                    if (nr >= 0 && nr < size && nc >= 0 && nc < size)
                    {
                        if (currentPuzzle.userGrid[nr, nc] == CellState.Empty)
                        {
                            currentPuzzle.userGrid[nr, nc] = CellState.MarkX;
                        }
                    }
                }
            }
        }
        
        #endregion
        
        #region UI Updates
        
        /// <summary>
        /// Updates button interactability based on game state.
        /// </summary>
        private void UpdateButtonStates()
        {
            bool hasPuzzle = currentPuzzle != null;
            
            if (resetButton != null)
                resetButton.interactable = hasPuzzle;
            
            if (hintButton != null)
                hintButton.interactable = isGameActive;
            
            if (checkButton != null)
                checkButton.interactable = isGameActive;
            
            if (revealButton != null)
                revealButton.interactable = hasPuzzle;
        }
        
        /// <summary>
        /// Updates the status text with message and color.
        /// </summary>
        private void UpdateStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }
        
        #endregion
    }
}
