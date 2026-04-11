// ============================================================
// File:    RendererInlineHtml.cs
// Purpose: Renderer for inline HTML tags.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders an <see cref="HtmlInline"/> into the layout.
    /// Respects the global preference for stripping HTML content.
    /// </summary>
    public class RendererInlineHtml : MarkdownObjectRenderer<RendererMarkdown, HtmlInline>
    {
        /// <summary>
        /// Writes the inline HTML content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The HTML inline node.</param>
        protected override void Write(RendererMarkdown renderer, HtmlInline node)
        {
            if (!MarkdownPreferences.StripHTML)
            {
                renderer.Text(node.Tag);
            }
        }
    }
}
