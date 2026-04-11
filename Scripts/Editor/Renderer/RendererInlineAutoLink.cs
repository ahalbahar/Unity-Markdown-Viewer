// ============================================================
// File:    RendererInlineAutoLink.cs
// Purpose: Renderer for automatic hyperlink inlines.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders an <see cref="AutolinkInline"/> into the layout.
    /// Automatically treats the URL text as a clickable link.
    /// </summary>
    public class RendererInlineAutoLink : MarkdownObjectRenderer<RendererMarkdown, AutolinkInline>
    {
        /// <summary>
        /// Writes the auto-link content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The auto-link inline node.</param>
        protected override void Write(RendererMarkdown renderer, AutolinkInline node)
        {
            renderer.Link = node.Url;
            renderer.Text(node.Url);
            renderer.Link = null;
        }
    }
}
