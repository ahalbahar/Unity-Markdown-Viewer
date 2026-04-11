// ============================================================
// File:    ContentImage.cs
// Purpose: Inline layout content for images.
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;
using UnityEditor;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Represents an inline image in the markdown document.
    /// Handles image fetching, scaling, and placeholder rendering.
    /// </summary>
    public class ContentImage : Content
    {
        /// <summary>
        /// Gets or sets the source URL or asset path of the image.
        /// </summary>
        public string URL;

        /// <summary>
        /// Gets or sets the fallback alt text shown when the image is unavailable.
        /// </summary>
        public string Alt;

        /// <summary>
        /// Gets or sets the optional width override applied to the rendered image.
        /// </summary>
        public int OverrideWidth;

        /// <summary>
        /// Gets or sets the optional height override applied to the rendered image.
        /// </summary>
        public int OverrideHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentImage"/> class.
        /// </summary>
        /// <param name="payload">The content payload (texture/text).</param>
        /// <param name="style">The style to apply.</param>
        /// <param name="link">Optional URL or anchor link.</param>
        public ContentImage(GUIContent payload, Style style, string link)
            : base(payload, style, link)
        {
        }

        /// <summary>
        /// Draws the image or alternative text if the image is not available.
        /// </summary>
        public override void Draw(Context context)
        {
            if (Payload.image != null)
            {
                if (!string.IsNullOrEmpty(Link))
                {
                    EditorGUIUtility.AddCursorRect(Location, MouseCursor.Link);
                    if (GUI.Button(Location, GUIContent.none, GUIStyle.none))
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(Link, @"^\w+:", System.Text.RegularExpressions.RegexOptions.Singleline))
                        {
                            Application.OpenURL(Link);
                        }
                        else
                        {
                            context.SelectPage(Link);
                        }
                    }
                }

                GUI.DrawTexture(Location, Payload.image, ScaleMode.ScaleToFit);
            }
            else if (!string.IsNullOrEmpty(Payload.text))
            {
                base.Draw(context);
            }
        }

        /// <summary>
        /// Updates the image content by fetching it from the context and calculating its display size.
        /// </summary>
        public override void Update(Context context)
        {
            Payload.image = context.FetchImage(URL);
            Payload.text = null;

            if (Payload.image == null)
            {
                context.Apply(Style);
                var text = !string.IsNullOrEmpty(Alt) ? Alt : URL;
                Payload.text = string.Format("[{0}]", text);
                Location.size = context.CalcSize(Payload);
            }
            else
            {
                var w = OverrideWidth > 0 ? (float)OverrideWidth : Payload.image.width;
                var h = OverrideHeight > 0 ? (float)OverrideHeight : Payload.image.height;

                // If only one dimension is overridden, scale the other to preserve aspect ratio
                if (OverrideWidth > 0 && OverrideHeight <= 0 && Payload.image.width > 0)
                {
                    h = Payload.image.height * (w / Payload.image.width);
                }
                else if (OverrideHeight > 0 && OverrideWidth <= 0 && Payload.image.height > 0)
                {
                    w = Payload.image.width * (h / Payload.image.height);
                }

                Location.size = new Vector2(w, h);
            }
        }
    }
}
