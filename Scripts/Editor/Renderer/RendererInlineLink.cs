// ============================================================
// File:    RendererInlineLink.cs
// Purpose: Renderer for hyperlinks and images.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="LinkInline"/> into the layout.
    /// Handles both standard hyperlinks and image tags, including support for 
    /// custom width/height attributes and dynamic URLs.
    /// </summary>
    public class RendererInlineLink : MarkdownObjectRenderer<RendererMarkdown, LinkInline>
    {
        /// <summary>
        /// Writes the link or image content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The link inline node.</param>
        protected override void Write(RendererMarkdown renderer, LinkInline node)
        {
            var url = node.GetDynamicUrl != null ? node.GetDynamicUrl() : node.Url;

            if (node.IsImage)
            {
                // Read optional size overrides: ![alt](url){width=200 height=100}
                int width = 0;
                int height = 0;
                var attrs = node.GetAttributes();
                if (attrs?.Properties != null)
                {
                    foreach (var prop in attrs.Properties)
                    {
                        if (prop.Key == "width" && int.TryParse(prop.Value, out var w)) width = w;
                        if (prop.Key == "height" && int.TryParse(prop.Value, out var h)) height = h;
                    }
                }

                renderer.Layout.Image(url, renderer.GetContents(node), node.Title, width, height);
            }
            else
            {
                renderer.Link = url;

                if (!string.IsNullOrEmpty(node.Title))
                {
                    renderer.ToolTip = node.Title;
                }

                renderer.WriteChildren(node);

                renderer.ToolTip = null;
                renderer.Link = null;
            }
        }
    }
}
