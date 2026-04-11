// ============================================================
// File:    RendererBlockThematicBreak.cs
// Purpose: Renderer for thematic breaks (horizontal rules).
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="ThematicBreakBlock"/> (horizontal rule) into the layout.
    /// </summary>
    public class RendererBlockThematicBreak : MarkdownObjectRenderer<RendererMarkdown, ThematicBreakBlock>
    {
        /// <summary>
        /// Writes the thematic break to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The thematic break block object.</param>
        protected override void Write(RendererMarkdown renderer, ThematicBreakBlock block)
        {
            renderer.Layout.HorizontalLine();
            renderer.FinishBlock();
        }
    }
}
