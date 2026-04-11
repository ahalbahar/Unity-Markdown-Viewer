// ============================================================
// File:    RendererBlockQuote.cs
// Purpose: Renderer for blockquotes and GitHub-style admonitions.
// Author:  Ahmad Albahar
// ============================================================

using AB.MDV.Layout;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text;
using UnityEngine;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Renders a <see cref="QuoteBlock"/> which can be a standard blockquote or a GitHub-style admonition 
    /// (e.g., [!NOTE], [!TIP]). Handles nested block rendering and icon decoration.
    /// </summary>
    public class RendererBlockQuote : MarkdownObjectRenderer<RendererMarkdown, QuoteBlock>
    {
        /// <summary>
        /// Writes the blockquote content to the renderer's layout.
        /// </summary>
        /// <param name="renderer">The markdown renderer.</param>
        /// <param name="block">The quote block object.</param>
        protected override void Write(RendererMarkdown renderer, QuoteBlock block)
        {
            var prevConsumeSpace = renderer.ConsumeSpace;
            renderer.ConsumeSpace = false;

            var admonition = DetectAdmonition(block, out var firstPara, out var bodyStartInline);

            renderer.Layout.StartBlock(true);

            if (admonition != AdmonitionKind.None)
            {
                renderer.Layout.SetAdmonition(admonition);
                RenderAdmonitionTitle(renderer, admonition);

                // Same-paragraph format: "[!NOTE]\nbody text" — marker and body
                // are in one ParagraphBlock separated by a LineBreakInline.
                // Render the inline nodes that follow the line-break.
                if (bodyStartInline != null)
                {
                    RenderInlinesFrom(renderer, bodyStartInline);
                    renderer.Layout.NewLine();
                }

                // Render any additional paragraphs (two-paragraph format).
                for (var i = 1; i < block.Count; i++)
                {
                    renderer.Write(block[i]);
                }
            }
            else
            {
                renderer.WriteChildren(block);
            }

            renderer.Layout.EndBlock();

            renderer.ConsumeSpace = prevConsumeSpace;

            renderer.FinishBlock(true);
        }

        /// <summary>
        /// Detects the admonition type in the first paragraph of the blockquote.
        /// </summary>
        /// <param name="block">The quote block to inspect.</param>
        /// <param name="firstPara">Output parameter for the first paragraph block.</param>
        /// <param name="bodyStart">Output parameter for the first inline node of the body text.</param>
        /// <returns>The detected <see cref="AdmonitionKind"/>.</returns>
        private static AdmonitionKind DetectAdmonition(QuoteBlock block,
                                                       out ParagraphBlock firstPara,
                                                       out Inline bodyStart)
        {
            firstPara = null;
            bodyStart = null;

            if (block.Count == 0) return AdmonitionKind.None;

            firstPara = block[0] as ParagraphBlock;
            if (firstPara == null || firstPara.Inline == null) return AdmonitionKind.None;

            // Walk the first line only (stop at LineBreakInline).
            var sb = new StringBuilder();
            var inline = firstPara.Inline.FirstChild;

            while (inline != null)
            {
                if (inline is LineBreakInline)
                {
                    // Body follows on the next "line" inside the same paragraph.
                    bodyStart = inline.NextSibling;
                    break;
                }
                else if (inline is LiteralInline lit)
                {
                    sb.Append(lit.Content.ToString());
                }
                else if (inline is LinkInline link)
                {
                    // "[!NOTE]" → LinkInline whose label children spell out "!NOTE"
                    sb.Append('[');
                    AppendContainerText(sb, link);
                    sb.Append(']');
                }
                else
                {
                    // Any other inline on the marker line means this is not an admonition.
                    return AdmonitionKind.None;
                }

                inline = inline.NextSibling;
            }

            switch (sb.ToString().Trim().ToUpperInvariant())
            {
                case "[!NOTE]": return AdmonitionKind.Note;
                case "[!TIP]": return AdmonitionKind.Tip;
                case "[!WARNING]": return AdmonitionKind.Warning;
                case "[!IMPORTANT]": return AdmonitionKind.Important;
                case "[!CAUTION]": return AdmonitionKind.Caution;
                default:
                    bodyStart = null;
                    return AdmonitionKind.None;
            }
        }

        /// <summary>
        /// Recursively appends all literal text inside a ContainerInline to a StringBuilder.
        /// </summary>
        private static void AppendContainerText(StringBuilder sb, ContainerInline container)
        {
            var child = container.FirstChild;
            while (child != null)
            {
                if (child is LiteralInline lit)
                    sb.Append(lit.Content.ToString());
                else if (child is ContainerInline nested)
                    AppendContainerText(sb, nested);

                child = child.NextSibling;
            }
        }

        /// <summary>
        /// Renders inline nodes starting from a specific node.
        /// </summary>
        private static void RenderInlinesFrom(RendererMarkdown renderer, Inline start)
        {
            var cur = start;
            while (cur != null)
            {
                if (cur is LiteralInline lit)
                {
                    var style = new Style();
                    renderer.Layout.Text(lit.Content.ToString(), style, null, null);
                }
                else if (cur is LineBreakInline)
                {
                    renderer.Layout.NewLine();
                }

                cur = cur.NextSibling;
            }
        }

        /// <summary>
        /// Renders the title section of an admonition block using the theme colors.
        /// </summary>
        private static void RenderAdmonitionTitle(RendererMarkdown renderer, AdmonitionKind kind)
        {
            var colors = MarkdownTheme.Instance.Active;
            var color = GetBorderColor(kind, colors);
            var colorHex = "#" + ColorUtility.ToHtmlStringRGB(color);
            var icon = GetIcon(kind);
            var label = kind.ToString().ToUpperInvariant();

            var titleText = $"<color={colorHex}><b>{icon} {label}</b></color>";
            var titleStyle = new Style();
            titleStyle.RichText = true;

            renderer.Layout.Text(titleText, titleStyle, null, null);
            renderer.Layout.NewLine();
        }

        private static Color GetBorderColor(AdmonitionKind kind, MarkdownTheme.ThemeColors colors)
        {
            switch (kind)
            {
                case AdmonitionKind.Note: return colors.AdmonitionNoteBorder;
                case AdmonitionKind.Tip: return colors.AdmonitionTipBorder;
                case AdmonitionKind.Warning: return colors.AdmonitionWarnBorder;
                case AdmonitionKind.Important: return colors.AdmonitionImportantBorder;
                case AdmonitionKind.Caution: return colors.AdmonitionCautionBorder;
                default: return colors.QuoteBorder;
            }
        }

        private static string GetIcon(AdmonitionKind kind)
        {
            switch (kind)
            {
                case AdmonitionKind.Note: return "\u2139"; // ℹ
                case AdmonitionKind.Tip: return "\u2726"; // ✦
                case AdmonitionKind.Warning: return "\u26A0"; // ⚠
                case AdmonitionKind.Important: return "\u25CF"; // ●
                case AdmonitionKind.Caution: return "\u2715"; // ✕
                default: return ">";
            }
        }
    }
}
