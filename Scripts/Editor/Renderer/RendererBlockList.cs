// ============================================================
// File:    RendererBlockList.cs
// Purpose: Renderer for ordered and unordered list blocks.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="ListBlock"/> (ordered or unordered) into the layout.
    /// Manages indentation, prefixes (bullets/numbers), and child item rendering.
    /// </summary>
    public class RendererBlockList : MarkdownObjectRenderer<RendererMarkdown, ListBlock>
    {
        /// <summary>
        /// Writes the list block content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The list block object.</param>
        protected override void Write(RendererMarkdown renderer, ListBlock block)
        {
            var layout = renderer.Layout;

            layout.Space();
            layout.Indent();

            var prevConsumeSpace = renderer.ConsumeSpace;
            renderer.ConsumeSpace = true;

            var prefixStyle = renderer.Style;

            if (!block.IsOrdered)
            {
                prefixStyle.Bold = true;
            }

            for (var i = 0; i < block.Count; i++)
            {
                var prefix = block.IsOrdered ? (i + 1).ToString() + "." : "\u2022";
                layout.Prefix(prefix, prefixStyle);
                renderer.WriteChildren(block[i] as ListItemBlock);
            }

            renderer.ConsumeSpace = prevConsumeSpace;
            layout.Outdent();
            layout.Space();
        }
    }
}
