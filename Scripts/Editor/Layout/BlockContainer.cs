// ============================================================
// File:    BlockContainer.cs
// Purpose: A container block that holds and arranges child blocks.
// Author:  Ahmad Albahar
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Specifies the type of admonition (callout) for a block.
    /// </summary>
    public enum AdmonitionKind
    {
        None,
        Note,
        Tip,
        Warning,
        Important,
        Caution
    }

    /// <summary>
    /// A layout block that acts as a container for other blocks.
    /// Handles various layout modes like vertical stacks, horizontal rows, and specialized styling for tables and quotes.
    /// </summary>
    public class BlockContainer : Block
    {
        /// <summary>
        /// Gets or sets a value indicating whether this container is part of a blockquote.
        /// </summary>
        public bool Quoted = false;

        /// <summary>
        /// Gets or sets a value indicating whether this container should be highlighted (e.g., code blocks).
        /// </summary>
        public bool Highlight = false;

        /// <summary>
        /// Gets or sets a value indicating whether children should be arranged horizontally.
        /// </summary>
        public bool Horizontal = false;

        /// <summary>
        /// Gets or sets a value indicating whether this block represents a table row.
        /// </summary>
        public bool IsTableRow = false;

        /// <summary>
        /// Gets or sets a value indicating whether this block represents a table header row.
        /// </summary>
        public bool IsTableHeader = false;

        /// <summary>
        /// Gets or sets the type of admonition styling applied to this container.
        /// </summary>
        public AdmonitionKind Admonition = AdmonitionKind.None;

        private List<Block> mBlocks = new List<Block>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockContainer"/> class.
        /// </summary>
        /// <param name="indent">The indentation level.</param>
        public BlockContainer(float indent) : base(indent) { }

        /// <summary>
        /// Adds a child block to this container.
        /// </summary>
        /// <param name="block">The block to add.</param>
        /// <returns>The added block.</returns>
        public Block Add(Block block)
        {
            block.Parent = this;
            mBlocks.Add(block);
            return block;
        }

        /// <summary>
        /// Recursively searches for a block with the specified ID.
        /// </summary>
        /// <param name="id">The ID to search for.</param>
        /// <returns>The matching block if found; otherwise, null.</returns>
        public override Block Find(string id)
        {
            if (id.Equals(ID, StringComparison.Ordinal))
            {
                return this;
            }

            foreach (var block in mBlocks)
            {
                var match = block.Find(id);

                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Arranges the child blocks within the container.
        /// </summary>
        public override void Arrange(Context context, Vector2 pos, float maxWidth)
        {
            Rect.position = new Vector2(pos.x + Indent, pos.y);
            Rect.width = maxWidth - Indent - context.IndentSize;

            var paddingBottom = 0.0f;
            var paddingVertical = 0.0f;

            if (Highlight || IsTableHeader || IsTableRow)
            {
                GUIStyle style;

                if (Highlight)
                {
                    style = GUI.skin.GetStyle(Quoted ? "blockquote" : "blockcode");
                }
                else
                {
                    style = GUI.skin.GetStyle(IsTableHeader ? "th" : "tr");
                }

                pos.x += style.padding.left;
                pos.y += style.padding.top;

                maxWidth -= style.padding.horizontal;
                paddingBottom = style.padding.bottom;
                paddingVertical = style.padding.vertical;
            }

            if (Horizontal)
            {
                Rect.height = 0;
                float columnWidth = mBlocks.Count == 0 ? maxWidth : maxWidth / mBlocks.Count;

                foreach (var block in mBlocks)
                {
                    block.Arrange(context, pos, columnWidth);
                    pos.x += block.Rect.width;
                    Rect.height = Mathf.Max(Rect.height, block.Rect.height);
                }

                Rect.height += paddingVertical;
            }
            else
            {
                foreach (var block in mBlocks)
                {
                    block.Arrange(context, pos, maxWidth);
                    pos.y += block.Rect.height;
                }

                Rect.height = pos.y - Rect.position.y + paddingBottom;
            }
        }

        /// <summary>
        /// Draws the container and its children.
        /// </summary>
        public override void Draw(Context context)
        {
            var colors = MarkdownTheme.Instance.Active;

            // Backgrounds
            if (Highlight)
            {
                if (!Quoted)
                {
                    GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("blockcode"));

                    // Optional 1 px border around code blocks
                    var borderColor = colors.CodeBlockBorder;
                    UnityEditor.EditorGUI.DrawRect(new Rect(Rect.x, Rect.y, Rect.width, 1), borderColor);
                    UnityEditor.EditorGUI.DrawRect(new Rect(Rect.x, Rect.yMax - 1, Rect.width, 1), borderColor);
                    UnityEditor.EditorGUI.DrawRect(new Rect(Rect.x, Rect.y, 1, Rect.height), borderColor);
                    UnityEditor.EditorGUI.DrawRect(new Rect(Rect.xMax - 1, Rect.y, 1, Rect.height), borderColor);
                }
                else
                {
                    GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("blockquote"));

                    // Left border — colour depends on admonition type
                    var borderColor = GetAdmonitionBorderColor(colors);
                    var borderRect = new Rect(Rect.x, Rect.y, 4, Rect.height);
                    UnityEditor.EditorGUI.DrawRect(borderRect, borderColor);
                }
            }
            else if (IsTableHeader)
            {
                GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("th"));
            }
            else if (IsTableRow)
            {
                var parentBlock = Parent as BlockContainer;
                if (parentBlock == null)
                {
                    GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("tr"));
                }
                else
                {
                    var idx = parentBlock.mBlocks.IndexOf(this);
                    GUI.Box(Rect, string.Empty, GUI.skin.GetStyle(idx % 2 == 0 ? "tr" : "trl"));
                }
            }

            // Content
            mBlocks.ForEach(block => block.Draw(context));
        }

        /// <summary>
        /// Removes a trailing spacer block if present.
        /// </summary>
        public void RemoveTrailingSpace()
        {
            if (mBlocks.Count > 0 && mBlocks[mBlocks.Count - 1] is BlockSpace)
            {
                mBlocks.RemoveAt(mBlocks.Count - 1);
            }
        }

        private Color GetAdmonitionBorderColor(MarkdownTheme.ThemeColors colors)
        {
            switch (Admonition)
            {
                case AdmonitionKind.Note: return colors.AdmonitionNoteBorder;
                case AdmonitionKind.Tip: return colors.AdmonitionTipBorder;
                case AdmonitionKind.Warning: return colors.AdmonitionWarnBorder;
                case AdmonitionKind.Important: return colors.AdmonitionImportantBorder;
                case AdmonitionKind.Caution: return colors.AdmonitionCautionBorder;
                default: return colors.QuoteBorder;
            }
        }
    }
}
