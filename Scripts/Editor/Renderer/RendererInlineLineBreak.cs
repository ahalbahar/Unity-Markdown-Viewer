// ============================================================
// File:    RendererInlineLineBreak.cs
// Purpose: Renderer for line breaks (hard and soft).
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="LineBreakInline"/> into the layout.
    /// Handles hard breaks (resulting in a new layout line) and soft breaks (resulting in a space).
    /// </summary>
    public class RendererInlineLineBreak : MarkdownObjectRenderer<RendererMarkdown, LineBreakInline>
    {
        /// <summary>
        /// Writes the line break to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The line break inline node.</param>
        protected override void Write(RendererMarkdown renderer, LineBreakInline node)
        {
            if (node.IsHard)
            {
                renderer.FinishBlock();
            }
            else
            {
                renderer.Text(" ");
            }
        }
    }
}
