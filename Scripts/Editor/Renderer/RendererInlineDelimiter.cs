// ============================================================
// File:    RendererInlineDelimiter.cs
// Purpose: Renderer for delimiter inlines.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="DelimiterInline"/> into the layout.
    /// Delimiters are characters like '*', '_', '~' that haven't been resolved into formatting yet.
    /// </summary>
    public class RendererInlineDelimiter : MarkdownObjectRenderer<RendererMarkdown, DelimiterInline>
    {
        /// <summary>
        /// Writes the delimiter content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The delimiter inline node.</param>
        protected override void Write(RendererMarkdown renderer, DelimiterInline node)
        {
            renderer.Text(node.ToLiteral());
            renderer.WriteChildren(node);
        }
    }
}
