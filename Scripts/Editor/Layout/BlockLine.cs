// ============================================================
// File:    BlockLine.cs
// Purpose: A layout block that renders a horizontal separator (hr).
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Represents a horizontal rule (hr) separator line in the layout.
    /// </summary>
    public class BlockLine : Block
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockLine"/> class.
        /// </summary>
        /// <param name="indent">The indentation level.</param>
        public BlockLine( float indent ) : base( indent ) { }

        /// <summary>
        /// Draws the horizontal line using the "hr" GUI style.
        /// </summary>
        public override void Draw( Context context )
        {
            var rect = new Rect( Rect.position.x, Rect.center.y, Rect.width, 1.0f );
            GUI.Label( rect, string.Empty, GUI.skin.GetStyle( "hr" ) );
        }

        /// <summary>
        /// Arranges the line block with a fixed height.
        /// </summary>
        public override void Arrange( Context context, Vector2 pos, float maxWidth )
        {
            Rect.position = pos;
            Rect.width = maxWidth;
            Rect.height = 10.0f;
        }
    }
}
