// ============================================================
// File:    RendererBlockDefinitionList.cs
// Purpose: Container renderer for definition lists.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Extensions.DefinitionLists;
using Markdig.Renderers;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="DefinitionList"/> by writing its children.
    /// Definition lists typically consist of terms and their corresponding definitions.
    /// </summary>
    public class RendererBlockDefinitionList : MarkdownObjectRenderer<RendererMarkdown, DefinitionList>
    {
        /// <summary>
        /// Writes the definition list children to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The definition list object.</param>
        protected override void Write(RendererMarkdown renderer, DefinitionList block)
        {
            renderer.WriteChildren(block);
        }
    }
}
