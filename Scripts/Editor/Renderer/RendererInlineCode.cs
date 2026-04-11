// ============================================================
// File:    RendererInlineCode.cs
// Purpose: Renderer for inline code snippets.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="CodeInline"/> into the layout.
    /// Applies fixed-width formatting to the nested content.
    /// </summary>
    public class RendererInlineCode : MarkdownObjectRenderer<RendererMarkdown, CodeInline>
    {
        /// <summary>
        /// Writes the inline code content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The code inline node.</param>
        protected override void Write(RendererMarkdown renderer, CodeInline node)
        {
            var prevStyle = renderer.Style;
            renderer.Style.Fixed = true;
            renderer.Text(node.Content);
            renderer.Style = prevStyle;
        }
    }
}
