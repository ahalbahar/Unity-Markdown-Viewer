// ============================================================
// File:    RendererTable.cs
// Purpose: Renderer for markdown tables.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using System.Linq;
using UnityEngine;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="Table"/> block into the layout.
    /// Manages table structures including rows, cells, and header formatting.
    /// </summary>
    public class RendererTable : MarkdownObjectRenderer<RendererMarkdown, Table>
    {
        /// <summary>
        /// Writes the table content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="table">The table object.</param>
        protected override void Write(RendererMarkdown renderer, Table table)
        {
            var layout = renderer.Layout;

            if (table.Count == 0)
            {
                return;
            }

            layout.StartTable();

            // Limit the columns to the number of headers
            var numCols = (table[0] as TableRow).Count(c => (c as TableCell).Count > 0);

            // Column alignment
            var alignment = table.ColumnDefinitions.Select(cd => cd.Alignment.HasValue ? cd.Alignment.Value : TableColumnAlign.Left).ToArray();

            foreach (TableRow row in table)
            {
                if (row == null)
                {
                    continue;
                }

                layout.StartTableRow(row.IsHeader);
                var consumeSpace = renderer.ConsumeSpace;
                renderer.ConsumeSpace = true;

                var numCells = Mathf.Min(numCols, row.Count);

                for (var cellIndex = 0; cellIndex < numCells; cellIndex++)
                {
                    var cell = row[cellIndex] as TableCell;

                    if (cell == null || cell.Count == 0)
                    {
                        continue;
                    }

                    if (cell[0].Span.IsEmpty)
                    {
                        renderer.Write(new LiteralInline(" "));

                        if (cellIndex != row.Count - 1)
                        {
                            layout.NewLine();
                        }
                    }
                    else
                    {
                        var consumeNewLine = renderer.ConsumeNewLine;

                        if (cellIndex == numCols - 1)
                        {
                            renderer.ConsumeNewLine = true;
                        }

                        renderer.Write(new LiteralInline(" "));
                        renderer.WriteChildren(cell);
                        renderer.ConsumeNewLine = consumeNewLine;
                    }
                }

                renderer.ConsumeSpace = consumeSpace;
                layout.EndTableRow();
            }

            layout.EndTable();
        }
    }
}

