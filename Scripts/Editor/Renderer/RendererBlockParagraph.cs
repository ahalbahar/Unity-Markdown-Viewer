// ============================================================
// File:    RendererBlockParagraph.cs
// Purpose: Renderer for paragraph blocks.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="ParagraphBlock"/> into the layout.
    /// Terminates the paragraph with a standard block space.
    /// </summary>
    public class RendererBlockParagraph : MarkdownObjectRenderer<RendererMarkdown, ParagraphBlock>
    {
        /// <summary>
        /// Writes the paragraph content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The paragraph block object.</param>
        protected override void Write(RendererMarkdown renderer, ParagraphBlock block)
        {
            renderer.WriteLeafBlockInline(block);
            renderer.FinishBlock(true);
        }
    }
}
