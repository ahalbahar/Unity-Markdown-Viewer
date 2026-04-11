// ============================================================
// File:    MarkdownLayout.cs
// Purpose: Container for the laid-out Markdown document and its context.
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Represents the final laid-out state of a Markdown document.
    /// Manages the top-level document block and provides methods for arrangement and drawing.
    /// </summary>
    public class MarkdownLayout
    {
        private Context mContext;
        private BlockContainer mDocument;

        /// <summary>
        /// Gets the total height of the laid-out document.
        /// </summary>
        public float Height => mDocument.Rect.height;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownLayout"/> class.
        /// </summary>
        /// <param name="context">The layout context.</param>
        /// <param name="doc">The root document block container.</param>
        public MarkdownLayout(Context context, BlockContainer doc)
        {
            mContext = context;
            mDocument = doc;
        }

        /// <summary>
        /// Searches for a block with the specified ID in the document.
        /// </summary>
        /// <param name="id">The ID to search for.</param>
        /// <returns>The block if found; otherwise, null.</returns>
        public Block Find(string id)
        {
            return mDocument.Find(id);
        }

        /// <summary>
        /// Arranges the document blocks within the specified horizontal space.
        /// </summary>
        /// <param name="pos">The starting position.</param>
        /// <param name="maxWidth">The maximum available width.</param>
        public void Arrange(Vector2 pos, float maxWidth)
        {
            mContext.Reset();
            mDocument.Arrange(mContext, pos, maxWidth);
        }

        /// <summary>
        /// Draws the document at the specified position.
        /// </summary>
        /// <param name="pos">The drawing position.</param>
        public void Draw(Vector2 pos)
        {
            mContext.Reset();
            mDocument.Draw(mContext);
        }
    }
}
