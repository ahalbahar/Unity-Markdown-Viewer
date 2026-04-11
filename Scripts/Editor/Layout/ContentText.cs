// ============================================================
// File:    ContentText.cs
// Purpose: Inline layout content for text.
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Represents a basic inline text element in the markdown document.
    /// </summary>
    public class ContentText : Content
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentText"/> class.
        /// </summary>
        /// <param name="payload">The text content.</param>
        /// <param name="style">The style to apply.</param>
        /// <param name="link">Optional URL or anchor link.</param>
        public ContentText(GUIContent payload, Style style, string link)
            : base(payload, style, link)
        {
        }
    }
}

