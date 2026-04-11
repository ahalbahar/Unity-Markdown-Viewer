// ============================================================
// File:    Block.cs
// Purpose: Base class for all layout blocks in the Markdown renderer.
// Author:  Ahmad Albahar
// ============================================================

using System;
using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Base class for all renderable layout blocks.
    /// Handles positioning, indentation, and basic identification.
    /// </summary>
    public abstract class Block
    {
        /// <summary>
        /// Optional unique identifier for the block (e.g., for anchor navigation).
        /// </summary>
        public string ID = null;

        /// <summary>
        /// The bounding rectangle of the block in document coordinates.
        /// </summary>
        public Rect Rect = new Rect();

        /// <summary>
        /// The parent block containing this block.
        /// </summary>
        public Block Parent = null;

        /// <summary>
        /// Horizontal indentation level for this block.
        /// </summary>
        public float Indent = 0.0f;

        /// <summary>
        /// Arranges the block and its children within the specified horizontal space.
        /// </summary>
        /// <param name="context">The layout context.</param>
        /// <param name="anchor">The starting position for layout.</param>
        /// <param name="maxWidth">The maximum available width.</param>
        public abstract void Arrange(Context context, Vector2 anchor, float maxWidth);

        /// <summary>
        /// Draws the block and its children into the current GUI.
        /// </summary>
        /// <param name="context">The layout context containing rendering state.</param>
        public abstract void Draw(Context context);

        /// <summary>
        /// Initializes a new instance of the <see cref="Block"/> class.
        /// </summary>
        /// <param name="indent">The indentation level for this block.</param>
        protected Block(float indent)
        {
            Indent = indent;
        }

        /// <summary>
        /// Recursively searches for a block with the specified ID.
        /// </summary>
        /// <param name="id">The ID to search for.</param>
        /// <returns>The block if found; otherwise, null.</returns>
        public virtual Block Find(string id)
        {
            return id.Equals(ID, StringComparison.Ordinal) ? this : null;
        }
    }
}
