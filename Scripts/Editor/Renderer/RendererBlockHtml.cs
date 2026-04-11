// ============================================================
// File:    RendererBlockHtml.cs
// Purpose: Renderer for raw HTML blocks.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="HtmlBlock"/> into the layout.
    /// Respects the global preference for stripping HTML content.
    /// </summary>
    public class RendererBlockHtml : MarkdownObjectRenderer<RendererMarkdown, HtmlBlock>
    {
        /// <summary>
        /// Writes the HTML block content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The HTML block object.</param>
        protected override void Write(RendererMarkdown renderer, HtmlBlock block)
        {
            if (!MarkdownPreferences.StripHTML)
            {
                renderer.WriteLeafRawLines(block);
                renderer.FinishBlock();
            }
        }
    }
}
