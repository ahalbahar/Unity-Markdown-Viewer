// ============================================================
// File:    RendererBlockDefinitionTerm.cs
// Purpose: Renderer for terms within a definition list.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Extensions.DefinitionLists;
using Markdig.Renderers;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="DefinitionTerm"/> by applying bold formatting to its content.
    /// Terminates the term block with a standard block finish.
    /// </summary>
    public class RendererBlockDefinitionTerm : MarkdownObjectRenderer<RendererMarkdown, DefinitionTerm>
    {
        /// <summary>
        /// Writes the definition term content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The definition term object.</param>
        protected override void Write(RendererMarkdown renderer, DefinitionTerm block)
        {
            var prevBold = renderer.Style.Bold;
            renderer.Style.Bold = true;
            renderer.WriteLeafBlockInline(block);
            renderer.Style.Bold = prevBold;
            renderer.FinishBlock(true);
        }
    }
}
