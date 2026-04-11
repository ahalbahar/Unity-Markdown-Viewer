// ============================================================
// File:    RendererBlockCode.cs
// Purpose: Renderer for code blocks, supporting syntax highlighting.
// Author:  Ahmad Albahar
// ============================================================

using Markdig.Renderers;
using Markdig.Syntax;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="CodeBlock"/> (including fenced code blocks) into the layout.
    /// Handles language-specific syntax highlighting and fixed-width formatting.
    /// </summary>
    public class RendererBlockCode : MarkdownObjectRenderer<RendererMarkdown, CodeBlock>
    {
        /// <summary>
        /// Writes the code block content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The code block object.</param>
        protected override void Write(RendererMarkdown renderer, CodeBlock block)
        {
            var fencedCodeBlock = block as FencedCodeBlock;
            var lang = fencedCodeBlock?.Info;

            var prevStyle = renderer.Style;
            renderer.Style.Fixed = true;
            renderer.Style.Block = true;

            var content = string.Empty;
            for (var i = 0; i < block.Lines.Count; i++)
            {
                content += block.Lines.Lines[i].ToString() + "\n";
            }

            var highlighted = SyntaxHighlighter.Highlight(code: content, lang: lang);

            if (highlighted != content)
            {
                renderer.Style.RichText = true;

                renderer.Layout.StartBlock(false);

                var lines = highlighted.Split('\n');
                for (var i = 0; i < lines.Length; i++)
                {
                    if (i == lines.Length - 1 && string.IsNullOrEmpty(lines[i]))
                    {
                        break;
                    }

                    renderer.Text(lines[i]);
                    renderer.Layout.NewLine();
                }

                renderer.Layout.EndBlock();
                renderer.Style = prevStyle;
                renderer.FinishBlock(true);
                return;
            }

            renderer.Layout.StartBlock(false);
            renderer.WriteLeafRawLines(block);
            renderer.Layout.EndBlock();

            renderer.Style = prevStyle;
            renderer.FinishBlock(true);
        }
    }
}
