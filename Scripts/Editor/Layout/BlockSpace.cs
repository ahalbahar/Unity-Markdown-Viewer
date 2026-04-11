// ============================================================
// File:    BlockSpace.cs
// Purpose: A layout block that represents vertical whitespace.
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Represents vertical spacing between blocks in the layout.
    /// </summary>
    public class BlockSpace : Block
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockSpace"/> class.
        /// </summary>
        /// <param name="indent">The indentation level.</param>
        public BlockSpace(float indent) : base(indent) { }

        /// <summary>
        /// Spaces do not render anything.
        /// </summary>
        public override void Draw(Context context)
        {
        }

        /// <summary>
        /// Arranges the space block with a height proportional to the line height.
        /// </summary>
        public override void Arrange(Context context, Vector2 pos, float maxWidth)
        {
            Rect.position = pos;
            Rect.width = 1.0f;
            Rect.height = context.LineHeight * 0.75f;
        }
    }
}
