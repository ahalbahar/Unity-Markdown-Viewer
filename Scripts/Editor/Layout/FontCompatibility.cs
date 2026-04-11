// ============================================================
// File:    FontCompatibility.cs
// Purpose: Provides font fallback handling for Unity editor versions with weak emoji support.
// Author:  Ahmad Albahar
// Notes:   Older Unity IMGUI builds often need an OS font stack to display emoji glyphs correctly.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Resolves editor fonts that can display both normal text and emoji content across Unity versions.
    /// </summary>
    internal static class FontCompatibility
    {
#if !UNITY_6000_0_OR_NEWER
        private static readonly Dictionary<int, Font> VariableFontCache = new Dictionary<int, Font>();
        private static readonly string[] VariableFontCandidates =
        {
            "Segoe UI",
            "Segoe UI Emoji",
            "Apple Color Emoji",
            "Helvetica Neue",
            "Arial Unicode MS",
            "Noto Sans",
            "Noto Color Emoji",
            "Noto Emoji",
            "Liberation Sans",
            "DejaVu Sans",
            "Arial"
        };
#endif

        /// <summary>
        /// Returns a font suitable for the current editor version and style.
        /// </summary>
        /// <param name="referenceFont">The font defined by the active GUI skin.</param>
        /// <param name="style">The markdown style being rendered.</param>
        /// <param name="fontSize">The effective font size.</param>
        /// <returns>The font that should be assigned to the GUI style.</returns>
        internal static Font ResolveFont(Font referenceFont, Style style, int fontSize)
        {
#if UNITY_6000_0_OR_NEWER
            return referenceFont;
#else
            if (style.Fixed)
            {
                return referenceFont;
            }

            int safeSize = Mathf.Max(fontSize, 12);
            if (VariableFontCache.TryGetValue(safeSize, out Font cachedFont) && cachedFont != null)
            {
                return cachedFont;
            }

            Font fallbackFont = Font.CreateDynamicFontFromOSFont(VariableFontCandidates, safeSize);
            if (fallbackFont == null)
            {
                return referenceFont;
            }

            // CHANGED: Older Unity IMGUI versions need an OS-driven dynamic font stack to pick up emoji glyphs reliably.
            VariableFontCache[safeSize] = fallbackFont;
            return fallbackFont;
#endif
        }
    }
}
