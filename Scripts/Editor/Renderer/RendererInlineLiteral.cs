// ============================================================
// File:    RendererInlineLiteral.cs
// Purpose: Renderer for literal text inlines.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="LiteralInline"/> (plain text) into the layout.
    /// </summary>
    public class RendererInlineLiteral : MarkdownObjectRenderer<RendererMarkdown, LiteralInline>
    {
        /// <summary>
        /// Writes the literal text to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The literal inline node.</param>
        protected override void Write(RendererMarkdown renderer, LiteralInline node)
        {
            renderer.Text(node.Content.ToString());
        }
    }
}
