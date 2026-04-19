// ============================================================
// File:    MarkdownMermaid.cs
// Purpose: Builds themed Mermaid render requests with fallback endpoints.
// Author:  Ahmad Albahar
// Created: 2026-04-19
// Notes:   Uses Mermaid Ink as primary renderer and Kroki as fallback.
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Represents one Mermaid diagram together with its themed render request.
    /// </summary>
    public sealed class MarkdownMermaidDiagram
    {
        /// <summary>
        /// Gets the raw Mermaid source.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the display title used by the viewer.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the image request used to render the diagram.
        /// </summary>
        public MarkdownImageRequest ImageRequest { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownMermaidDiagram"/> class.
        /// </summary>
        /// <param name="source">The raw Mermaid source.</param>
        /// <param name="title">The diagram title.</param>
        /// <param name="imageRequest">The render request.</param>
        public MarkdownMermaidDiagram(string source, string title, MarkdownImageRequest imageRequest)
        {
            Source = source;
            Title = title;
            ImageRequest = imageRequest;
        }
    }

    /// <summary>
    /// Builds Mermaid render requests that match the active Markdown theme.
    /// </summary>
    public static class MarkdownMermaid
    {
        private const int AttemptsPerEndpoint = 2;

        /// <summary>
        /// Creates a themed Mermaid diagram render request.
        /// </summary>
        /// <param name="source">The raw Mermaid source.</param>
        /// <param name="title">The display title.</param>
        /// <returns>The themed Mermaid diagram request.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is empty.</exception>
        public static MarkdownMermaidDiagram CreateDiagram(string source, string title)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException("Mermaid diagram source must not be empty.", nameof(source));
            }

            var theme = MarkdownTheme.Instance.Active;
            string themeConfigJson = BuildThemeConfigJson(theme);
            string themedSource = BuildDirectiveSource(source, themeConfigJson);
            string mermaidInkPayload = BuildMermaidInkPayload(source, themeConfigJson);
            string backgroundHex = ToHex(theme.PageBackground);
            string cacheKey = ComputeSha256($"{themeConfigJson}\n---\n{source}");
            string displayName = string.IsNullOrWhiteSpace(title) ? "Mermaid diagram" : title;

            var candidates = new List<MarkdownImageCandidate>(AttemptsPerEndpoint * 2);
            string mermaidInkUrl = BuildMermaidInkUrl(mermaidInkPayload, backgroundHex);
            string krokiBody = BuildKrokiBody(themedSource);

            for (int attempt = 0; attempt < AttemptsPerEndpoint; attempt++)
            {
                candidates.Add(new MarkdownImageCandidate(mermaidInkUrl));
            }

            for (int attempt = 0; attempt < AttemptsPerEndpoint; attempt++)
            {
                candidates.Add(new MarkdownImageCandidate("https://kroki.io/", "POST", krokiBody, "application/json"));
            }

            var imageRequest = new MarkdownImageRequest(
                candidates,
                cacheKey,
                displayName,
                MarkdownImageRequestKind.MermaidDiagram,
                MarkdownPreferences.MermaidDiskCacheEnabled);

            return new MarkdownMermaidDiagram(source, displayName, imageRequest);
        }

        private static string BuildMermaidInkUrl(string payload, string backgroundHex)
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            byte[] compressed = CompressZlib(payloadBytes);
            string encoded = ToBase64Url(compressed);
            return $"https://mermaid.ink/img/pako:{encoded}?type=png&bgColor={backgroundHex}";
        }

        private static string BuildKrokiBody(string themedSource)
        {
            return string.Format(
                "{{\"diagram_source\":\"{0}\",\"diagram_type\":\"mermaid\",\"output_format\":\"png\"}}",
                EscapeJson(themedSource));
        }

        private static string BuildMermaidInkPayload(string source, string themeConfigJson)
        {
            return string.Format(
                "{{\"code\":\"{0}\",\"mermaid\":{1},\"autoSync\":true,\"updateDiagram\":false,\"editorMode\":\"code\"}}",
                EscapeJson(source),
                themeConfigJson);
        }

        private static string BuildDirectiveSource(string source, string themeConfigJson)
        {
            return $"%%{{init: {themeConfigJson}}}%%\n{source.Trim()}";
        }

        private static string BuildThemeConfigJson(MarkdownTheme.ThemeColors colors)
        {
            bool isDark = GetLuminance(colors.PageBackground) < 0.5f;

            string background = ToHex(colors.PageBackground);
            string text = ToHex(colors.Text);
            string primary = ToHex(colors.CodeBackground);
            string border = ToHex(colors.CodeBlockBorder);
            string accent = ToHex(colors.Link);
            string secondary = ToHex(colors.TableHeaderBackground);
            string tertiary = ToHex(colors.QuoteBackground);
            string grid = ToHex(Color.Lerp(colors.Border, colors.PageBackground, 0.35f));
            string crit = ToHex(Color.Lerp(colors.AdmonitionWarnBorder, colors.PageBackground, 0.15f));
            string done = ToHex(Color.Lerp(colors.Link, colors.PageBackground, 0.35f));

            return string.Format(
                "{{\"theme\":\"base\",\"themeVariables\":{{\"darkMode\":{0},\"background\":\"#{1}\",\"primaryColor\":\"#{2}\",\"primaryBorderColor\":\"#{3}\",\"primaryTextColor\":\"#{4}\",\"secondaryColor\":\"#{5}\",\"secondaryBorderColor\":\"#{3}\",\"secondaryTextColor\":\"#{4}\",\"tertiaryColor\":\"#{6}\",\"tertiaryBorderColor\":\"#{3}\",\"tertiaryTextColor\":\"#{4}\",\"lineColor\":\"#{7}\",\"textColor\":\"#{4}\",\"mainBkg\":\"#{2}\",\"secondBkg\":\"#{5}\",\"tertiaryBkg\":\"#{6}\",\"clusterBkg\":\"#{6}\",\"clusterBorder\":\"#{3}\",\"nodeBorder\":\"#{3}\",\"edgeLabelBackground\":\"#{1}\",\"labelBackground\":\"#{1}\",\"labelColor\":\"#{4}\",\"actorBkg\":\"#{2}\",\"actorBorder\":\"#{3}\",\"actorTextColor\":\"#{4}\",\"signalColor\":\"#{4}\",\"signalTextColor\":\"#{4}\",\"labelBoxBkgColor\":\"#{1}\",\"labelBoxBorderColor\":\"#{3}\",\"activationBkgColor\":\"#{5}\",\"activationBorderColor\":\"#{3}\",\"sectionBkgColor\":\"#{5}\",\"altSectionBkgColor\":\"#{6}\",\"taskBkgColor\":\"#{2}\",\"taskBorderColor\":\"#{3}\",\"taskTextColor\":\"#{4}\",\"taskTextDarkColor\":\"#{4}\",\"taskTextOutsideColor\":\"#{4}\",\"gridColor\":\"#{8}\",\"doneTaskBkgColor\":\"#{9}\",\"doneTaskBorderColor\":\"#{3}\",\"critBkgColor\":\"#{10}\",\"critBorderColor\":\"#{3}\",\"todayLineColor\":\"#{7}\"}}}}",
                isDark ? "true" : "false",
                background,
                primary,
                border,
                text,
                secondary,
                tertiary,
                accent,
                grid,
                done,
                crit);
        }

        private static float GetLuminance(Color color)
        {
            return (0.2126f * color.r) + (0.7152f * color.g) + (0.0722f * color.b);
        }

        private static string ToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length + 32);

            foreach (char character in value)
            {
                switch (character)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(character))
                        {
                            builder.AppendFormat("\\u{0:x4}", (int)character);
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            return builder.ToString();
        }

        private static byte[] CompressZlib(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                output.WriteByte(0x78);
                output.WriteByte(0xDA);

                using (var deflate = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal, true))
                {
                    deflate.Write(data, 0, data.Length);
                }

                uint checksum = ComputeAdler32(data);
                output.WriteByte((byte)((checksum >> 24) & 0xFF));
                output.WriteByte((byte)((checksum >> 16) & 0xFF));
                output.WriteByte((byte)((checksum >> 8) & 0xFF));
                output.WriteByte((byte)(checksum & 0xFF));

                return output.ToArray();
            }
        }

        private static uint ComputeAdler32(byte[] data)
        {
            const uint modulo = 65521;
            uint a = 1;
            uint b = 0;

            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % modulo;
                b = (b + a) % modulo;
            }

            return (b << 16) | a;
        }

        private static string ToBase64Url(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string ComputeSha256(string value)
        {
            using (var hash = SHA256.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(value);
                byte[] digest = hash.ComputeHash(data);
                var builder = new StringBuilder(digest.Length * 2);

                for (int i = 0; i < digest.Length; i++)
                {
                    builder.Append(digest[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
