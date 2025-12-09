// CellView.cs - Individual cell UI behavior and rendering
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using StarLogicGrid.Data;

namespace StarLogicGrid.UI
{
    /// <summary>
    /// UI component for a single cell in the Star Logic Grid.
    /// Handles rendering, click detection, and animations.
    /// </summary>
    public class CellView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [Tooltip("Background image showing region color")]
        [SerializeField] private Image backgroundImage;
        
        [Tooltip("Text displaying the cell symbol (star, +, or X)")]
        [SerializeField] private TextMeshProUGUI symbolText;
        
        [Tooltip("Border images: Top, Right, Bottom, Left")]
        [SerializeField] private Image[] borderImages;
        
        [Header("Visual Settings")]
        [Tooltip("Color for star symbols")]
        [SerializeField] private Color starColor = new Color(0.96f, 0.62f, 0.04f);
        
        [Tooltip("Color for stars in error state")]
        [SerializeField] private Color starErrorColor = new Color(0.94f, 0.27f, 0.27f);
        
        [Tooltip("Color for plus mark")]
        [SerializeField] private Color markPlusColor = new Color(0.23f, 0.51f, 0.95f);
        
        [Tooltip("Color for X mark")]
        [SerializeField] private Color markXColor = new Color(0.42f, 0.45f, 0.51f);
        
        [Tooltip("Border color")]
        [SerializeField] private Color borderColor = new Color(0.12f, 0.16f, 0.22f);
        
        [Header("Border Settings")]
        [Tooltip("Width of thick borders (region boundaries)")]
        [SerializeField] private float thickBorderWidth = 4f;
        
        [Tooltip("Width of normal borders")]
        [SerializeField] private float normalBorderWidth = 1f;
        
        [Header("Animation Settings")]
        [Tooltip("Duration of hover scale animation")]
        [SerializeField] private float hoverScaleDuration = 0.1f;
        
        [Tooltip("Scale when hovering")]
        [SerializeField] private float hoverScale = 1.05f;
        
        // Cell position in the grid
        public int Row { get; private set; }
        public int Col { get; private set; }
        
        // Current state
        private CellState currentState;
        private bool isError;
        private Color originalBackgroundColor;
        
        // Callback when cell is clicked
        private System.Action<int, int> onCellClicked;
        
        // Animation coroutine reference
        private Coroutine hoverCoroutine;
        private Coroutine hintCoroutine;
        
        /// <summary>
        /// Initializes the cell with position, color, and click callback.
        /// </summary>
        public void Initialize(int row, int col, Color regionColor, 
            System.Action<int, int> clickCallback)
        {
            Row = row;
            Col = col;
            originalBackgroundColor = regionColor;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = regionColor;
            }
            
            onCellClicked = clickCallback;
            SetState(CellState.Empty, false);
            
            // Set border colors
            if (borderImages != null)
            {
                foreach (var border in borderImages)
                {
                    if (border != null)
                    {
                        border.color = borderColor;
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the cell's visual state.
        /// </summary>
        public void SetState(CellState state, bool hasError)
        {
            currentState = state;
            isError = hasError;
            UpdateVisual();
        }
        
        /// <summary>
        /// Configures border thickness based on region boundaries.
        /// </summary>
        public void SetBorders(bool thickTop, bool thickRight, 
            bool thickBottom, bool thickLeft)
        {
            if (borderImages == null || borderImages.Length < 4) return;
            
            SetBorderThickness(borderImages[0], thickTop, true);     // Top
            SetBorderThickness(borderImages[1], thickRight, false);   // Right
            SetBorderThickness(borderImages[2], thickBottom, true);   // Bottom
            SetBorderThickness(borderImages[3], thickLeft, false);    // Left
        }
        
        /// <summary>
        /// Sets the thickness of a single border.
        /// </summary>
        private void SetBorderThickness(Image border, bool isThick, bool isHorizontal)
        {
            if (border == null) return;
            
            float thickness = isThick ? thickBorderWidth : normalBorderWidth;
            RectTransform rect = border.rectTransform;
            
            if (isHorizontal)
            {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, thickness);
            }
            else
            {
                rect.sizeDelta = new Vector2(thickness, rect.sizeDelta.y);
            }
            
            // Make thick borders more visible
            if (isThick)
            {
                border.color = borderColor;
            }
            else
            {
                border.color = new Color(borderColor.r, borderColor.g, borderColor.b, 0.5f);
            }
        }
        
        /// <summary>
        /// Updates the visual appearance based on current state.
        /// </summary>
        private void UpdateVisual()
        {
            if (symbolText == null) return;
            
            switch (currentState)
            {
                case CellState.Empty:
                    symbolText.text = "";
                    break;
                    
                case CellState.Star:
                    symbolText.text = "★";
                    symbolText.color = isError ? starErrorColor : starColor;
                    symbolText.fontSize = 48;
                    symbolText.fontStyle = FontStyles.Normal;
                    
                    // Add glow effect by enabling shadow
                    AddStarGlow();
                    break;
                    
                case CellState.MarkPlus:
                    symbolText.text = "+";
                    symbolText.color = markPlusColor;
                    symbolText.fontSize = 40;
                    symbolText.fontStyle = FontStyles.Bold;
                    RemoveStarGlow();
                    break;
                    
                case CellState.MarkX:
                    symbolText.text = "✕";
                    symbolText.color = markXColor;
                    symbolText.fontSize = 32;
                    symbolText.fontStyle = FontStyles.Bold;
                    RemoveStarGlow();
                    break;
            }
        }
        
        /// <summary>
        /// Adds a glow effect to the star symbol.
        /// </summary>
        private void AddStarGlow()
        {
            // You can add a material with glow shader here
            // For now, we just ensure the color is bright
            if (symbolText != null && !isError)
            {
                symbolText.outlineWidth = 0.1f;
                symbolText.outlineColor = new Color(1f, 0.8f, 0.2f, 0.5f);
            }
        }
        
        /// <summary>
        /// Removes the glow effect.
        /// </summary>
        private void RemoveStarGlow()
        {
            if (symbolText != null)
            {
                symbolText.outlineWidth = 0f;
            }
        }
        
        #region Input Handling
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onCellClicked?.Invoke(Row, Col);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(AnimateScale(hoverScale));
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(AnimateScale(1f));
        }
        
        #endregion
        
        #region Animations
        
        /// <summary>
        /// Animates the cell scale for hover effect.
        /// </summary>
        private IEnumerator AnimateScale(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            float elapsed = 0f;
            
            while (elapsed < hoverScaleDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / hoverScaleDuration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }
            
            transform.localScale = endScale;
        }
        
        /// <summary>
        /// Plays the hint animation (pulsing green highlight).
        /// </summary>
        public void PlayHintAnimation()
        {
            if (hintCoroutine != null)
            {
                StopCoroutine(hintCoroutine);
            }
            hintCoroutine = StartCoroutine(PulseAnimation());
        }
        
        /// <summary>
        /// Pulsing background animation for hints.
        /// </summary>
        private IEnumerator PulseAnimation()
        {
            if (backgroundImage == null) yield break;
            
            Color highlightColor = new Color(0.13f, 0.77f, 0.37f, 0.6f);
            
            for (int i = 0; i < 3; i++)
            {
                // Fade to highlight
                float elapsed = 0f;
                while (elapsed < 0.15f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / 0.15f;
                    backgroundImage.color = Color.Lerp(originalBackgroundColor, 
                        highlightColor, t);
                    yield return null;
                }
                
                // Fade back
                elapsed = 0f;
                while (elapsed < 0.15f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / 0.15f;
                    backgroundImage.color = Color.Lerp(highlightColor, 
                        originalBackgroundColor, t);
                    yield return null;
                }
            }
            
            backgroundImage.color = originalBackgroundColor;
        }
        
        /// <summary>
        /// Plays an error shake animation.
        /// </summary>
        public void PlayErrorAnimation()
        {
            StartCoroutine(ShakeAnimation());
        }
        
        private IEnumerator ShakeAnimation()
        {
            Vector3 originalPos = transform.localPosition;
            float shakeMagnitude = 5f;
            float shakeDuration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float x = originalPos.x + Random.Range(-shakeMagnitude, shakeMagnitude) * 
                    (1 - elapsed / shakeDuration);
                transform.localPosition = new Vector3(x, originalPos.y, originalPos.z);
                yield return null;
            }
            
            transform.localPosition = originalPos;
        }
        
        #endregion
    }
}
