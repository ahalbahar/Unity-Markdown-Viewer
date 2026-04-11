// ============================================================
// File:    RendererBlockDefinitionItem.cs
// Purpose: Renderer for a single item (term and its definitions) in a definition list.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Extensions.DefinitionLists;
using Markdig.Renderers;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="DefinitionItem"/> which contains one or more <see cref="DefinitionTerm"/>s 
    /// followed by one or more definitions (other blocks).
    /// </summary>
    public class RendererBlockDefinitionItem : MarkdownObjectRenderer<RendererMarkdown, DefinitionItem>
    {
        /// <summary>
        /// Writes the definition item content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The definition item object.</param>
        protected override void Write(RendererMarkdown renderer, DefinitionItem block)
        {
            foreach (var child in block)
            {
                if (child is DefinitionTerm)
                {
                    renderer.Write(child);
                }
                else
                {
                    renderer.Layout.Indent();
                    renderer.Write(child);
                    renderer.Layout.Outdent();
                }
            }
        }
    }
}
