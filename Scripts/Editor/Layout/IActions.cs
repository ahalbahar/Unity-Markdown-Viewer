// ============================================================
// File:    IActions.cs
// Purpose: Interface for layout actions such as image fetching and navigation.
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Defines the actions available to the layout system for external interactions.
    /// </summary>
    public interface IActions
    {
        /// <summary>
        /// Requests an image texture from the specified URL.
        /// </summary>
        /// <param name="url">The image URL or path.</param>
        /// <returns>The texture if available; otherwise, a placeholder.</returns>
        Texture FetchImage(string url);

        /// <summary>
        /// Requests navigation to the specified page or anchor.
        /// </summary>
        /// <param name="url">The target URL or anchor link.</param>
        void SelectPage(string url);
    }
}
