// ============================================================
// File:    BlockContent.cs
// Purpose: A layout block that contains and arranges inline content.
// Author:  Ahmad Albahar
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Represents a block of inline content (text, images, etc.) that can wrap across multiple lines.
    /// Handles prefix rendering (e.g., bullet points) and line-based layout.
    /// </summary>
    public class BlockContent : Block
    {
        private Content mPrefix = null;
        private List<Content> mContent = new List<Content>();

        /// <summary>
        /// Gets a value indicating whether this block contains no content.
        /// </summary>
        public bool IsEmpty => mContent.Count == 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockContent"/> class.
        /// </summary>
        /// <param name="indent">The indentation level.</param>
        public BlockContent(float indent) : base(indent) { }

        /// <summary>
        /// Adds inline content to this block.
        /// </summary>
        /// <param name="content">The content to add.</param>
        public void Add(Content content)
        {
            mContent.Add(content);
        }

        /// <summary>
        /// Sets the prefix content (e.g., a bullet point) for this block.
        /// </summary>
        /// <param name="content">The prefix content.</param>
        public void Prefix(Content content)
        {
            mPrefix = content;
        }

        /// <summary>
        /// Arranges the inline content into rows, wrapping where necessary.
        /// </summary>
        public override void Arrange(Context context, Vector2 pos, float maxWidth)
        {
            var origin = pos;

            pos.x += Indent;
            maxWidth = Mathf.Max(maxWidth - Indent, context.MinWidth);

            Rect.position = pos;

            // prefix
            if (mPrefix != null)
            {
                mPrefix.Location.x = pos.x - context.IndentSize * 0.5f;
                mPrefix.Location.y = pos.y;
            }

            // content
            if (mContent.Count == 0)
            {
                Rect.width = 0.0f;
                Rect.height = 0.0f;
                return;
            }

            mContent.ForEach(c => c.Update(context));

            var rowWidth = mContent[0].Width;
            var rowHeight = mContent[0].Height;
            var startIndex = 0;

            for (var i = 1; i < mContent.Count; i++)
            {
                var content = mContent[i];

                if (rowWidth + content.Width > maxWidth)
                {
                    LayoutRow(pos, startIndex, i, rowHeight);
                    pos.y += rowHeight;

                    startIndex = i;
                    rowWidth = content.Width;
                    rowHeight = content.Height;
                }
                else
                {
                    rowWidth += content.Width;
                    rowHeight = Mathf.Max(rowHeight, content.Height);
                }
            }

            if (startIndex < mContent.Count)
            {
                LayoutRow(pos, startIndex, mContent.Count, rowHeight);
                pos.y += rowHeight;
            }

            Rect.width = maxWidth;
            Rect.height = pos.y - origin.y;
        }

        private void LayoutRow(Vector2 pos, int from, int until, float rowHeight)
        {
            for (var i = from; i < until; i++)
            {
                var content = mContent[i];

                content.Location.x = pos.x;
                content.Location.y = pos.y + rowHeight - content.Height;

                pos.x += content.Width;
            }
        }

        /// <summary>
        /// Draws the inline content and prefix.
        /// </summary>
        public override void Draw(Context context)
        {
            mContent.ForEach(c => c.Draw(context));

            if (mPrefix != null)
            {
                mPrefix.Draw(context);
            }
        }
    }
}
