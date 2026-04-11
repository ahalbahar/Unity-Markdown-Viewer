// ============================================================
// File:    RendererInlineEmphasis.cs
// Purpose: Renderer for emphasized text (bold, italic, etc.).
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders an <see cref="EmphasisInline"/> into the layout.
    /// Supports various emphasis types including bold, italic, strikethrough, highlight, 
    /// subscript, superscript, and underline depending on the delimiter character.
    /// </summary>
    public class RendererInlineEmphasis : MarkdownObjectRenderer<RendererMarkdown, EmphasisInline>
    {
        /// <summary>
        /// Writes the emphasis content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The emphasis inline node.</param>
        protected override void Write(RendererMarkdown renderer, EmphasisInline node)
        {
            var ch = node.DelimiterChar;
            var isDouble = node.IsDouble;

            // Save previous state for all flags we might change
            var prevBold = renderer.Style.Bold;
            var prevItalic = renderer.Style.Italic;
            var prevStrikethrough = renderer.Style.Strikethrough;
            var prevHighlight = renderer.Style.Highlight;
            var prevSubscript = renderer.Style.Subscript;
            var prevSuperscript = renderer.Style.Superscript;
            var prevUnderline = renderer.Style.Underline;

            if (ch == '*' || ch == '_')
            {
                if (isDouble) renderer.Style.Bold = true;
                else renderer.Style.Italic = true;
            }
            else if (ch == '~')
            {
                if (isDouble) renderer.Style.Strikethrough = true;
                else renderer.Style.Subscript = true;
            }
            else if (ch == '^')
            {
                renderer.Style.Superscript = true;
            }
            else if (ch == '+' && isDouble)
            {
                renderer.Style.Underline = true;
            }
            else if (ch == '=' && isDouble)
            {
                renderer.Style.Highlight = true;
            }

            renderer.WriteChildren(node);

            // Restore
            renderer.Style.Bold = prevBold;
            renderer.Style.Italic = prevItalic;
            renderer.Style.Strikethrough = prevStrikethrough;
            renderer.Style.Highlight = prevHighlight;
            renderer.Style.Subscript = prevSubscript;
            renderer.Style.Superscript = prevSuperscript;
            renderer.Style.Underline = prevUnderline;
        }
    }
}
