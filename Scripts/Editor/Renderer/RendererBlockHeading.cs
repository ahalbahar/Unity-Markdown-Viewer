// ============================================================
// File:    RendererBlockHeading.cs
// Purpose: Renderer for heading blocks (H1-H6).
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="HeadingBlock"/> into the layout.
    /// Sets the appropriate font size based on the heading level and handles custom IDs.
    /// </summary>
    public class RendererBlockHeading : MarkdownObjectRenderer<RendererMarkdown, HeadingBlock>
    {
        /// <summary>
        /// Writes the heading block content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The heading block object.</param>
        protected override void Write(RendererMarkdown renderer, HeadingBlock block)
        {
            var prevSize = renderer.Style.Size;
            renderer.Style.Size = block.Level;
            renderer.WriteLeafBlockInline(block);
            renderer.Style.Size = prevSize;

            // Honor custom IDs: ## My Heading {#custom-id} (requires UseGenericAttributes in pipeline)
            var attrs = block.GetAttributes();
            if (attrs?.Id != null)
            {
                renderer.Layout.OverrideCurrentID("#" + attrs.Id);
            }

            if (block.Level == 1)
            {
                renderer.Layout.HorizontalLine();
                renderer.FinishBlock(true);
            }
            else
            {
                renderer.FinishBlock();
            }
        }
    }
}
