// ============================================================
// File:    Content.cs
// Purpose: Base class for all inline content (text, images) in the layout.
// Author:  Ahmad Albahar
// ============================================================

using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Base class for all inline content elements.
    /// Manages positioning, styling, and interaction (links).
    /// </summary>
    public abstract class Content
    {
        /// <summary>
        /// Gets or sets the document-space location occupied by this content element.
        /// </summary>
        public Rect Location;

        /// <summary>
        /// Gets or sets the style flags applied while rendering this content.
        /// </summary>
        public Style Style;

        /// <summary>
        /// Gets or sets the GUI payload used to render text or images.
        /// </summary>
        public GUIContent Payload;

        /// <summary>
        /// Gets or sets the optional hyperlink or anchor target associated with this content.
        /// </summary>
        public string Link;

        /// <summary>
        /// Gets the current rendered width of the content.
        /// </summary>
        public float Width => Location.width;

        /// <summary>
        /// Gets the current rendered height of the content.
        /// </summary>
        public float Height => Location.height;

        /// <summary>
        /// Gets a value indicating whether the content requires periodic updates.
        /// </summary>
        public virtual bool CanUpdate => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Content"/> class.
        /// </summary>
        /// <param name="payload">The content payload (text/image).</param>
        /// <param name="style">The style to apply.</param>
        /// <param name="link">Optional URL or anchor link.</param>
        protected Content(GUIContent payload, Style style, string link)
        {
            Payload = payload;
            Style = style;
            Link = link;
        }

        /// <summary>
        /// Calculates the size of the content based on the current context.
        /// </summary>
        /// <param name="context">The layout context.</param>
        public void CalcSize(Context context)
        {
            Location.size = context.CalcSize(Payload);
        }

        /// <summary>
        /// Draws the content into the current GUI.
        /// </summary>
        /// <param name="context">The layout context.</param>
        public virtual void Draw(Context context)
        {
            if (string.IsNullOrEmpty(Link))
            {
                // Draw highlight background before label
                if (Style.Highlight)
                {
                    EditorGUI.DrawRect(Location, context.HighlightColor);
                }

                GUI.Label(Location, Payload, context.Apply(Style));

                // Draw underline after label
                if (Style.Underline)
                {
                    var underlineRect = new Rect(Location.x, Location.yMax - 1f, Location.width, 1f);
                    EditorGUI.DrawRect(underlineRect, context.UnderlineColor);
                }

                return;
            }

            EditorGUIUtility.AddCursorRect(Location, MouseCursor.Link);

            if (GUI.Button(Location, Payload, context.Apply(Style)))
            {
                if (Regex.IsMatch(Link, @"^\w+:", RegexOptions.Singleline))
                {
                    Application.OpenURL(Link);
                }
                else
                {
                    context.SelectPage(Link);
                }
            }
        }

        /// <summary>
        /// Optional update loop for dynamic content.
        /// </summary>
        /// <param name="context">The layout context.</param>
        public virtual void Update(Context context)
        {
        }
    }
}
