// ============================================================
// File:    RendererMarkdown.cs
// Purpose: Multi-purpose Markdig renderer for building MarkdownLayout.
// Author:  Ahmad Albahar
// ============================================================

using AB.MDV.Layout;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// The primary renderer for transforming a Markdig <see cref="MarkdownDocument"/> into a <see cref="MarkdownLayout"/>.
    /// Manages a collection of specific block and inline renderers.
    /// </summary>
    public class RendererMarkdown : RendererBase
    {
        internal LayoutBuilder Layout;
        internal Style Style = new Style();
        internal string ToolTip = null;

        private string mLink = null;

        /// <summary>
        /// Gets or sets the current hyperlink URL. Automatically updates the Style.Link flag.
        /// </summary>
        internal string Link
        {
            get => mLink;
            set
            {
                mLink = value;
                Style.Link = !string.IsNullOrEmpty(mLink);
            }
        }

        /// <summary>
        /// If true, the next requested block space will be ignored.
        /// </summary>
        public bool ConsumeSpace = false;

        /// <summary>
        /// If true, the next requested newline will be ignored.
        /// </summary>
        public bool ConsumeNewLine = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RendererMarkdown"/> class.
        /// </summary>
        /// <param name="doc">The layout builder to use for output.</param>
        public RendererMarkdown(LayoutBuilder doc)
        {
            Layout = doc;

            // Block renderers
            ObjectRenderers.Add(new RendererBlockCode());
            ObjectRenderers.Add(new RendererBlockList());
            ObjectRenderers.Add(new RendererBlockHeading());
            ObjectRenderers.Add(new RendererBlockHtml());
            ObjectRenderers.Add(new RendererBlockParagraph());
            ObjectRenderers.Add(new RendererBlockQuote());
            ObjectRenderers.Add(new RendererBlockThematicBreak());
            ObjectRenderers.Add(new RendererTable());
            ObjectRenderers.Add(new RendererBlockDefinitionList());
            ObjectRenderers.Add(new RendererBlockDefinitionItem());
            ObjectRenderers.Add(new RendererBlockDefinitionTerm());

            // Inline renderers
            ObjectRenderers.Add(new RendererInlineLink());
            ObjectRenderers.Add(new RendererInlineAutoLink());
            ObjectRenderers.Add(new RendererInlineCode());
            ObjectRenderers.Add(new RendererInlineDelimiter());
            ObjectRenderers.Add(new RendererInlineEmphasis());
            ObjectRenderers.Add(new RendererInlineLineBreak());
            ObjectRenderers.Add(new RendererInlineHtml());
            ObjectRenderers.Add(new RendererInlineHtmlEntity());
            ObjectRenderers.Add(new RendererInlineTaskList());
            ObjectRenderers.Add(new RendererInlineLiteral());
        }

        /// <summary>
        /// Renders the specified Markdown object.
        /// </summary>
        /// <param name="document">The document or object to render.</param>
        /// <returns>This renderer instance.</returns>
        public override object Render(MarkdownObject document)
        {
            Write(document);
            return this;
        }

        /// <summary>
        /// Helper to add a text fragment to the layout using the current style and links.
        /// </summary>
        /// <param name="text">The text content.</param>
        internal void Text(string text)
        {
            Layout.Text(text, Style, Link, ToolTip);
        }

        /// <summary>
        /// Writes all inline elements within a leaf block.
        /// </summary>
        /// <param name="block">The leaf block containing inlines.</param>
        internal void WriteLeafBlockInline(LeafBlock block)
        {
            var inline = block.Inline as Inline;

            while (inline != null)
            {
                Write(inline);
                inline = inline.NextSibling;
            }
        }

        /// <summary>
        /// Writes the raw lines of a leaf block as separate text layout calls.
        /// </summary>
        /// <param name="block">The leaf block containing raw lines.</param>
        internal void WriteLeafRawLines(LeafBlock block)
        {
            if (block.Lines.Lines == null)
            {
                return;
            }

            var lines = block.Lines;
            var slices = lines.Lines;

            for (int i = 0; i < lines.Count; i++)
            {
                Text(slices[i].ToString());
                Layout.NewLine();
            }
        }

        /// <summary>
        /// Concatenates all literal fragments within a container inline into a single string.
        /// </summary>
        /// <param name="node">The container inline node.</param>
        /// <returns>The concatenated literal content.</returns>
        internal string GetContents(ContainerInline node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            var inline = node.FirstChild;
            var content = string.Empty;

            while (inline != null)
            {
                var lit = inline as LiteralInline;

                if (lit != null)
                {
                    content += lit.Content.ToString();
                }

                inline = inline.NextSibling;
            }

            return content;
        }

        /// <summary>
        /// Finalizes the current block, optionally adding a vertical space.
        /// </summary>
        /// <param name="space">True to request a block space; false for a simple newline.</param>
        internal void FinishBlock(bool space = false)
        {
            if (space && !ConsumeSpace)
            {
                Layout.Space();
            }
            else if (!ConsumeNewLine)
            {
                Layout.NewLine();
            }
        }
    }
}
