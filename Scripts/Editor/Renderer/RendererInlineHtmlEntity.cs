// ============================================================
// File:    RendererInlineHtmlEntity.cs
// Purpose: Renderer for HTML entities (e.g., &amp;, &lt;).
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders an <see cref="HtmlEntityInline"/> into the layout.
    /// Converts transcoded HTML entities into their literal text representation.
    /// Respects the global preference for stripping HTML content.
    /// </summary>
    public class RendererInlineHtmlEntity : MarkdownObjectRenderer<RendererMarkdown, HtmlEntityInline>
    {
        /// <summary>
        /// Writes the HTML entity content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The HTML entity inline node.</param>
        protected override void Write(RendererMarkdown renderer, HtmlEntityInline node)
        {
            if (!MarkdownPreferences.StripHTML)
            {
                renderer.Text(node.Transcoded.ToString());
            }
        }
    }
}
