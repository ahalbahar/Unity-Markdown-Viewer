// ============================================================
// File:    RendererInlineTaskList.cs
// Purpose: Renderer for task-list checkboxes in lists.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Extensions.TaskLists;
using Markdig.Renderers;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="TaskList"/> inline into the layout.
    /// Produces ☑ (U+2611) for checked items and ☐ (U+2610) for unchecked.
    /// </summary>
    public class RendererInlineTaskList : MarkdownObjectRenderer<RendererMarkdown, TaskList>
    {
        /// <summary>
        /// Writes the task list glyph to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="node">The task list inline node.</param>
        protected override void Write(RendererMarkdown renderer, TaskList node)
        {
            var glyph = node.Checked ? "\u2611 " : "\u2610 "; // ☑ / ☐
            renderer.Text(glyph);
        }
    }
}
