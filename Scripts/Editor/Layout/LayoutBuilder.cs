// ============================================================
// File:    LayoutBuilder.cs
// Purpose: Concrete implementation of IBuilder for creating MarkdownLayout.
// Author:  Ahmad Albahar
// ============================================================

using System.Text;
using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Builder class that constructs a <see cref="MarkdownLayout"/> from Markdown tokens.
    /// Manages the hierarchical structure of blocks and inline content.
    /// </summary>
    public class LayoutBuilder : IBuilder
    {
        private Context mContext;
        private Style mStyle;
        private string mLink;
        private string mTooltip;
        private StringBuilder mWord;
        private float mIndent;

        private BlockContainer mDocument;
        private BlockContainer mCurrentContainer;
        private Block mCurrentBlock;
        private BlockContent mCurrentContent;

        private Block CurrentBlock
        {
            get => mCurrentBlock;
            set
            {
                mCurrentBlock = value;
                mCurrentContent = mCurrentBlock as BlockContent;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutBuilder"/> class.
        /// </summary>
        /// <param name="context">The layout context.</param>
        public LayoutBuilder(Context context)
        {
            mContext = context;

            mStyle = new Style();
            mLink = null;
            mTooltip = null;
            mWord = new StringBuilder(1024);

            mIndent = 0.0f;

            mDocument = new BlockContainer(mIndent);
            mCurrentContainer = mDocument;
            mCurrentBlock = null;
            mCurrentContent = null;
        }

        /// <summary>
        /// Finalizes the build process and returns the constructed layout.
        /// </summary>
        /// <returns>The constructed MarkdownLayout.</returns>
        public MarkdownLayout GetLayout()
        {
            return new MarkdownLayout(mContext, mDocument);
        }

        #region IBuilder Implementation

        /// <inheritdoc/>
        public void Text(string text, Style style, string link, string tooltip)
        {
            if (mCurrentContent == null)
            {
                NewContentBlock();
            }

            mContext.Apply(style);

            if (style.Size > 0)
            {
                if (mCurrentContent.ID == null)
                {
                    mCurrentContent.ID = "#";
                }
                else
                {
                    mCurrentContent.ID += "-";
                }

                mCurrentContent.ID += text.Trim().Replace(' ', '-').ToLower();
            }

            mStyle = style;
            mLink = link;
            mTooltip = tooltip;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (ch == '\n')
                {
                    AddWord();
                    NewLine();
                }
                else if (char.IsWhiteSpace(ch))
                {
                    if (style.Block || style.RichText)
                    {
                        // Code blocks and rich-text strings: preserve spaces so colour/bold
                        // tags are never split across word boundaries.
                        mWord.Append(ch);
                    }
                    else
                    {
                        mWord.Append(' ');
                        AddWord();
                    }
                }
                else if (!style.RichText && (ch == '.' || ch == '/' || ch == '_' || ch == ':' || ch == '-'))
                {
                    mWord.Append(ch);
                    AddWord();
                }
                // Break at camelCase / PascalCase transitions if the word is getting long
                else if (!style.RichText && char.IsLower(ch) && i + 1 < text.Length && char.IsUpper(text[i + 1]) && mWord.Length > 8)
                {
                    mWord.Append(ch);
                    AddWord();
                }
                else
                {
                    mWord.Append(ch);

                    // Force break if extremely long (emergency wrap)
                    if (!style.RichText && mWord.Length > 25)
                    {
                        AddWord();
                    }
                }
            }

            AddWord();
        }

        /// <inheritdoc/>
        public void Image(string url, string alt, string title, int width = 0, int height = 0)
        {
            var payload = new GUIContent();
            var content = new ContentImage(payload, mStyle, mLink);

            content.URL = url;
            content.Alt = alt;
            content.OverrideWidth = width;
            content.OverrideHeight = height;
            payload.tooltip = !string.IsNullOrEmpty(title) ? title : alt;

            AddContent(content);
        }

        /// <inheritdoc/>
        public void Diagram(MarkdownImageRequest imageRequest, string source, string title)
        {
            if (imageRequest == null)
            {
                throw new System.ArgumentNullException(nameof(imageRequest));
            }

            // CHANGED: Mermaid blocks now use a dedicated block so wide charts can scroll horizontally and pop out.
            Space();
            AddBlock(new BlockDiagram(mIndent, imageRequest, source, title));
            Space();
        }

        /// <inheritdoc/>
        public void NewLine()
        {
            if (mCurrentContent != null && mCurrentContent.IsEmpty)
            {
                return;
            }

            NewContentBlock();
        }

        /// <inheritdoc/>
        public void Space()
        {
            if (CurrentBlock is BlockSpace || CurrentBlock is BlockContainer)
            {
                return;
            }

            AddBlock(new BlockSpace(mIndent));
        }

        /// <inheritdoc/>
        public void HorizontalLine()
        {
            if (CurrentBlock is BlockLine)
            {
                return;
            }

            AddBlock(new BlockLine(mIndent));
        }

        /// <inheritdoc/>
        public void Indent()
        {
            NewLine();

            mIndent += mContext.IndentSize;

            if (mCurrentContent != null)
            {
                mCurrentContent.Indent = mIndent;
            }
        }

        /// <inheritdoc/>
        public void Outdent()
        {
            NewLine();

            mIndent = Mathf.Max(mIndent - mContext.IndentSize, 0.0f);

            if (mCurrentContent != null)
            {
                mCurrentContent.Indent = mIndent;
            }
        }

        /// <inheritdoc/>
        public void Prefix(string text, Style style)
        {
            mContext.Apply(style);

            if (mCurrentContent == null)
            {
                return;
            }

            var payload = new GUIContent(text);
            var content = new ContentText(payload, style, null);
            content.Location.size = mContext.CalcSize(payload);

            mCurrentContent.Prefix(content);
        }

        /// <inheritdoc/>
        public void StartBlock(bool quoted)
        {
            Space();
            mCurrentContainer = AddBlock(new BlockContainer(mIndent) { Highlight = true, Quoted = quoted });
            CurrentBlock = null;
        }

        /// <inheritdoc/>
        public void EndBlock()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as BlockContainer ?? mDocument;
            CurrentBlock = null;

            Space();
        }

        /// <summary>
        /// Sets the admonition type for the current container.
        /// </summary>
        /// <param name="kind">The admonition kind.</param>
        public void SetAdmonition(AdmonitionKind kind)
        {
            mCurrentContainer.Admonition = kind;
        }

        /// <summary>
        /// Overrides the identifier of the current content block.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        public void OverrideCurrentID(string id)
        {
            if (mCurrentContent != null)
            {
                mCurrentContent.ID = id;
            }
        }

        /// <inheritdoc/>
        public void StartTable()
        {
            Space();
            mCurrentContainer = AddBlock(new BlockContainer(mIndent) { Quoted = false, Highlight = false });
            CurrentBlock = null;
        }

        /// <inheritdoc/>
        public void EndTable()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as BlockContainer ?? mDocument;
            CurrentBlock = null;

            Space();
        }

        /// <inheritdoc/>
        public void StartTableRow(bool isHeader)
        {
            mCurrentContainer = AddBlock(new BlockContainer(mIndent) { Quoted = false, Highlight = false, Horizontal = true, IsTableHeader = isHeader, IsTableRow = !isHeader });
            CurrentBlock = null;
        }

        /// <inheritdoc/>
        public void EndTableRow()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as BlockContainer ?? mDocument;
            CurrentBlock = null;
        }

        #endregion

        #region Private Helpers

        private void AddContent(Content content)
        {
            if (mCurrentContent == null)
            {
                NewContentBlock();
            }

            mCurrentContent.Add(content);
        }

        private T AddBlock<T>(T block) where T : Block
        {
            CurrentBlock = mCurrentContainer.Add(block);
            return block;
        }

        private void NewContentBlock()
        {
            AddBlock(new BlockContent(mIndent));

            mStyle.Clear();
            mContext.Apply(mStyle);
        }

        private void AddWord()
        {
            if (mWord.Length == 0)
            {
                return;
            }

            var text = mWord.ToString();
            if (mStyle.Strikethrough)
            {
                text = $"<s>{text}</s>";
                mStyle.RichText = true;
                mContext.Apply(mStyle);
            }

            AddInlineSegments(text);

            mWord.Length = 0;
        }

        private void AddInlineSegments(string text)
        {
            if (!EmojiImageSupport.Enabled || mStyle.RichText)
            {
                AddTextContent(text);
                return;
            }

            var segments = EmojiImageSupport.Split(text);
            if (segments.Count == 0)
            {
                AddTextContent(text);
                return;
            }

            foreach (var segment in segments)
            {
                if (segment.IsEmojiImage)
                {
                    AddEmojiContent(segment);
                }
                else
                {
                    AddTextContent(segment.Text);
                }
            }
        }

        private void AddTextContent(string text)
        {
            var payload = new GUIContent(text, mTooltip);
            var content = new ContentText(payload, mStyle, mLink);
            content.CalcSize(mContext);
            AddContent(content);
        }

        private void AddEmojiContent(EmojiImageSupport.EmojiSegment segment)
        {
            mContext.Apply(mStyle);
            int size = Mathf.Max(1, Mathf.RoundToInt(mContext.LineHeight));

            var payload = new GUIContent
            {
                text = segment.Text,
                tooltip = mTooltip,
            };

            var content = new ContentImage(payload, mStyle, mLink)
            {
                URL = segment.ImageUrls[0],
                Alt = segment.Text,
                OverrideWidth = size,
                OverrideHeight = size,
            };

            // CHANGED: Emoji image requests must be deferred to the normal OnGUI layout/update pass.
            content.Location.size = new Vector2(size, size);
            AddContent(content);
        }

        #endregion
    }
}
