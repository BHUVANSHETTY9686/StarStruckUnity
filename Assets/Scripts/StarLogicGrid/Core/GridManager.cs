// GridManager.cs - Manages the puzzle grid UI creation and updates
using UnityEngine;
using UnityEngine.UI;
using StarLogicGrid.Data;
using StarLogicGrid.UI;

namespace StarLogicGrid.Core
{
    /// <summary>
    /// Manages the visual grid of cells for the Star Logic Grid puzzle.
    /// Handles cell creation, updates, and border configuration.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Prefab for individual cells")]
        [SerializeField] private GameObject cellPrefab;
        
        [Tooltip("GridLayoutGroup component for cell arrangement")]
        [SerializeField] private GridLayoutGroup gridLayout;
        
        [Tooltip("Container RectTransform for the grid")]
        [SerializeField] private RectTransform gridContainer;
        
        [Header("Region Colors")]
        [Tooltip("Color palette for regions - should have at least 8 colors")]
        [SerializeField] private Color[] regionColors = new Color[]
        {
            new Color(0.99f, 0.80f, 0.80f), // Light Red
            new Color(0.99f, 0.93f, 0.80f), // Light Yellow
            new Color(0.80f, 0.99f, 0.80f), // Light Green
            new Color(0.80f, 0.89f, 0.99f), // Light Blue
            new Color(0.89f, 0.80f, 0.99f), // Light Indigo
            new Color(0.96f, 0.80f, 0.99f), // Light Purple
            new Color(0.99f, 0.80f, 0.92f), // Light Pink
            new Color(0.99f, 0.87f, 0.80f)  // Light Orange
        };
        
        // Cell view references
        private CellView[,] cellViews;
        
        // Current puzzle data
        private PuzzleData currentPuzzle;
        
        // Click callback
        private System.Action<int, int> onCellClicked;
        
        // Shuffled colors for current puzzle
        private Color[] currentColors;
        
        /// <summary>
        /// Sets up the grid with a new puzzle.
        /// </summary>
        public void SetupGrid(PuzzleData puzzle, System.Action<int, int> clickCallback)
        {
            currentPuzzle = puzzle;
            onCellClicked = clickCallback;
            
            ClearGrid();
            ShuffleColors();
            CreateCells();
            UpdateAllCells();
        }
        
        /// <summary>
        /// Clears all cells from the grid.
        /// </summary>
        private void ClearGrid()
        {
            if (gridContainer == null) return;
            
            // Destroy all child objects
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gridContainer.GetChild(i).gameObject);
            }
        }
        
        /// <summary>
        /// Shuffles the color palette for variety.
        /// </summary>
        private void ShuffleColors()
        {
            currentColors = (Color[])regionColors.Clone();
            
            for (int i = currentColors.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (currentColors[i], currentColors[j]) = (currentColors[j], currentColors[i]);
            }
        }
        
        /// <summary>
        /// Creates all cell GameObjects for the grid.
        /// </summary>
        private void CreateCells()
        {
            if (cellPrefab == null || gridContainer == null || gridLayout == null)
            {
                Debug.LogError("GridManager: Missing required references!");
                return;
            }
            
            int size = currentPuzzle.gridSize;
            cellViews = new CellView[size, size];
            
            // Calculate cell size based on container
            float containerSize = Mathf.Min(gridContainer.rect.width, gridContainer.rect.height);
            
            // If container size is 0 (not laid out yet), use a default
            if (containerSize <= 0)
            {
                containerSize = 500f;
            }
            
            float cellSize = (containerSize - (gridLayout.spacing.x * (size - 1))) / size;
            
            // Configure grid layout
            gridLayout.constraintCount = size;
            gridLayout.cellSize = new Vector2(cellSize, cellSize);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            
            // Create cells row by row
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    CreateCell(r, c);
                }
            }
        }
        
        /// <summary>
        /// Creates a single cell at the specified position.
        /// </summary>
        private void CreateCell(int row, int col)
        {
            GameObject cellObj = Instantiate(cellPrefab, gridContainer);
            cellObj.name = $"Cell_{row}_{col}";
            
            CellView cellView = cellObj.GetComponent<CellView>();
            
            if (cellView == null)
            {
                Debug.LogError($"Cell prefab missing CellView component at ({row}, {col})");
                return;
            }
            
            // Get region color
            int regionId = currentPuzzle.regions[row, col];
            Color regionColor = currentColors[regionId % currentColors.Length];
            
            // Initialize the cell
            cellView.Initialize(row, col, regionColor, onCellClicked);
            
            // Configure borders
            SetupCellBorders(cellView, row, col);
            
            cellViews[row, col] = cellView;
        }
        
        /// <summary>
        /// Sets up borders for a cell based on region boundaries.
        /// </summary>
        private void SetupCellBorders(CellView cell, int r, int c)
        {
            int size = currentPuzzle.gridSize;
            int regionId = currentPuzzle.regions[r, c];
            
            // Determine which borders should be thick
            bool thickTop = (r == 0) || 
                (r > 0 && currentPuzzle.regions[r - 1, c] != regionId);
            
            bool thickBottom = (r == size - 1) || 
                (r < size - 1 && currentPuzzle.regions[r + 1, c] != regionId);
            
            bool thickLeft = (c == 0) || 
                (c > 0 && currentPuzzle.regions[r, c - 1] != regionId);
            
            bool thickRight = (c == size - 1) || 
                (c < size - 1 && currentPuzzle.regions[r, c + 1] != regionId);
            
            cell.SetBorders(thickTop, thickRight, thickBottom, thickLeft);
        }
        
        /// <summary>
        /// Updates all cells to reflect current puzzle state.
        /// </summary>
        public void UpdateAllCells()
        {
            if (cellViews == null || currentPuzzle == null) return;
            
            int size = currentPuzzle.gridSize;
            
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    UpdateCell(r, c);
                }
            }
        }
        
        /// <summary>
        /// Updates a single cell to reflect current state.
        /// </summary>
        public void UpdateCell(int row, int col)
        {
            if (cellViews == null || cellViews[row, col] == null) return;
            
            bool hasError = CheckCellError(row, col);
            cellViews[row, col].SetState(currentPuzzle.userGrid[row, col], hasError);
        }
        
        /// <summary>
        /// Checks if a cell has an error (invalid star placement).
        /// </summary>
        private bool CheckCellError(int r, int c)
        {
            if (currentPuzzle.userGrid[r, c] != CellState.Star)
            {
                return false;
            }
            
            return !PuzzleSolver.IsValidUserPlacement(currentPuzzle, r, c);
        }
        
        /// <summary>
        /// Gets the CellView at the specified position.
        /// </summary>
        public CellView GetCellView(int row, int col)
        {
            if (cellViews != null && row >= 0 && row < cellViews.GetLength(0) &&
                col >= 0 && col < cellViews.GetLength(1))
            {
                return cellViews[row, col];
            }
            return null;
        }
        
        /// <summary>
        /// Plays error animation on all cells with errors.
        /// </summary>
        public void HighlightErrors()
        {
            if (cellViews == null || currentPuzzle == null) return;
            
            int size = currentPuzzle.gridSize;
            
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (currentPuzzle.userGrid[r, c] == CellState.Star && 
                        CheckCellError(r, c))
                    {
                        cellViews[r, c].PlayErrorAnimation();
                    }
                }
            }
        }
    }
}
