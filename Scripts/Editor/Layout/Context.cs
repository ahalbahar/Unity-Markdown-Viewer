// ============================================================
// File:    Context.cs
// Purpose: Main layout and rendering context for the Markdown viewer.
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Holds the state and services required for laying out and rendering Markdown.
    /// Manages style conversion, image fetching, and navigation.
    /// </summary>
    public class Context
    {
        private StyleConverter mStyleConverter;
        private GUIStyle mStyleGUI;
        private MarkdownHandleImages mImages;
        private MarkdownHandleNavigate mNavigate;

        /// <summary>
        /// Gets the current line height based on the active GUI style.
        /// </summary>
        public float LineHeight => mStyleGUI.lineHeight;

        /// <summary>
        /// Gets the minimum width for layout elements.
        /// </summary>
        public float MinWidth => LineHeight * 2.0f;

        /// <summary>
        /// Gets the standard horizontal indentation size.
        /// </summary>
        public float IndentSize => LineHeight * 1.5f;

        /// <summary>
        /// Gets the active highlight color from the current theme.
        /// </summary>
        public Color HighlightColor => MarkdownTheme.Instance.Active.Highlight;

        /// <summary>
        /// Gets the active underline color from the current theme.
        /// </summary>
        public Color UnderlineColor => MarkdownTheme.Instance.Active.UnderlineColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="skin">The GUISkin to use for rendering.</param>
        /// <param name="images">The image handler service.</param>
        /// <param name="navigate">The navigation handler service.</param>
        public Context(GUISkin skin, MarkdownHandleImages images, MarkdownHandleNavigate navigate)
        {
            mStyleConverter = new StyleConverter(skin);
            mImages = images;
            mNavigate = navigate;

            Apply(Style.Default);
        }

        /// <summary>
        /// Requests navigation to the specified path or anchor.
        /// </summary>
        /// <param name="path">The target path or anchor link.</param>
        public void SelectPage(string path) => mNavigate.SelectPage(path);

        /// <summary>
        /// Fetches an image texture from the specified URL.
        /// </summary>
        /// <param name="url">The image URL or path.</param>
        /// <returns>The texture if available; otherwise, a placeholder.</returns>
        public Texture FetchImage(string url) => mImages.FetchImage(url);

        /// <summary>
        /// Resets the context to the default style.
        /// </summary>
        public void Reset() => Apply(Style.Default);

        /// <summary>
        /// Applies the specified <see cref="Style"/> and returns the corresponding <see cref="GUIStyle"/>.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The applied GUIStyle.</returns>
        public GUIStyle Apply(Style style)
        {
            mStyleGUI = mStyleConverter.Apply(style);
            return mStyleGUI;
        }

        /// <summary>
        /// Calculates the GUI size of the specified content using the current style.
        /// </summary>
        /// <param name="content">The content to measure.</param>
        /// <returns>The calculated size.</returns>
        public Vector2 CalcSize(GUIContent content) => mStyleGUI.CalcSize(content);

    }
}
