// ============================================================
// File:    MarkdownPlantUml.cs
// Purpose: Builds themed PlantUML render requests with fallback endpoints.
// Author:  Ahmad Albahar
// Created: 2026-07-08
// Notes:   Uses the PlantUML server as primary renderer and Kroki as fallback.
// ============================================================

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Represents one PlantUML diagram together with its themed render request.
    /// </summary>
    public sealed class MarkdownPlantUmlDiagram
    {
        /// <summary>
        /// Gets the raw PlantUML source.
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
        /// Initializes a new instance of the <see cref="MarkdownPlantUmlDiagram"/> class.
        /// </summary>
        /// <param name="source">The raw PlantUML source.</param>
        /// <param name="title">The diagram title.</param>
        /// <param name="imageRequest">The render request.</param>
        public MarkdownPlantUmlDiagram(string source, string title, MarkdownImageRequest imageRequest)
        {
            Source = source;
            Title = title;
            ImageRequest = imageRequest;
        }
    }

    /// <summary>
    /// Builds PlantUML render requests that match the active Markdown theme when safe.
    /// </summary>
    public static class MarkdownPlantUml
    {
        private const int AttemptsPerEndpoint = 2;

        /// <summary>
        /// Creates a PlantUML diagram render request.
        /// </summary>
        /// <param name="source">The raw PlantUML source.</param>
        /// <param name="title">The display title.</param>
        /// <returns>The PlantUML diagram request.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is empty.</exception>
        public static MarkdownPlantUmlDiagram CreateDiagram(string source, string title)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException("PlantUML diagram source must not be empty.", nameof(source));
            }

            string rawSource = NormalizeLineEndings(source).Trim();
            string renderSource = BuildThemedSource(rawSource, MarkdownTheme.Instance.Active);
            string displayName = string.IsNullOrWhiteSpace(title) ? "PlantUML diagram" : title;
            string cacheKey = MarkdownDiagramEncoding.ComputeSha256($"plantuml\n---\n{renderSource}");

            var candidates = new List<MarkdownImageCandidate>(AttemptsPerEndpoint * 2);
            string plantUmlUrl = BuildPlantUmlServerUrl(renderSource);
            string krokiBody = BuildKrokiBody(renderSource);

            for (int attempt = 0; attempt < AttemptsPerEndpoint; attempt++)
            {
                candidates.Add(new MarkdownImageCandidate(plantUmlUrl));
            }

            for (int attempt = 0; attempt < AttemptsPerEndpoint; attempt++)
            {
                candidates.Add(new MarkdownImageCandidate("https://kroki.io/", "POST", krokiBody, "application/json"));
            }

            var imageRequest = new MarkdownImageRequest(
                candidates,
                cacheKey,
                displayName,
                MarkdownImageRequestKind.PlantUmlDiagram,
                MarkdownPreferences.DiagramDiskCacheEnabled);

            return new MarkdownPlantUmlDiagram(rawSource, displayName, imageRequest);
        }

        private static string BuildPlantUmlServerUrl(string source)
        {
            string encoded = MarkdownDiagramEncoding.ToPlantUmlEncoded(source);
            return $"https://www.plantuml.com/plantuml/png/{encoded}";
        }

        private static string BuildKrokiBody(string source)
        {
            return string.Format(
                "{{\"diagram_source\":\"{0}\",\"diagram_type\":\"plantuml\",\"output_format\":\"png\"}}",
                MarkdownDiagramEncoding.EscapeJson(source));
        }

        private static string BuildThemedSource(string source, MarkdownTheme.ThemeColors colors)
        {
            string skinParams = BuildSkinParams(colors);

            if (!TryGetFirstMeaningfulLine(source, out string firstLine, out int insertionIndex))
            {
                return source;
            }

            if (firstLine.StartsWith("@start", StringComparison.OrdinalIgnoreCase)
                && !firstLine.StartsWith("@startuml", StringComparison.OrdinalIgnoreCase))
            {
                return source;
            }

            if (firstLine.StartsWith("@startuml", StringComparison.OrdinalIgnoreCase))
            {
                string separator = insertionIndex > 0 && source[insertionIndex - 1] == '\n'
                    ? string.Empty
                    : "\n";
                return source.Insert(insertionIndex, $"{separator}{skinParams}\n");
            }

            return $"{skinParams}\n{source}";
        }

        private static string BuildSkinParams(MarkdownTheme.ThemeColors colors)
        {
            string background = ToHex(colors.PageBackground);
            string text = ToHex(colors.Text);
            string primary = ToHex(colors.CodeBackground);
            string border = ToHex(colors.CodeBlockBorder);
            string accent = ToHex(colors.Link);
            string secondary = ToHex(colors.TableHeaderBackground);
            string note = ToHex(colors.QuoteBackground);

            var builder = new StringBuilder();
            builder.AppendLine($"skinparam backgroundColor #{background}");
            builder.AppendLine("skinparam shadowing false");
            builder.AppendLine($"skinparam defaultFontColor #{text}");
            builder.AppendLine($"skinparam ArrowColor #{accent}");
            builder.AppendLine($"skinparam SequenceArrowColor #{accent}");
            builder.AppendLine($"skinparam SequenceLifeLineBorderColor #{border}");
            builder.AppendLine($"skinparam ParticipantBorderColor #{border}");
            builder.AppendLine($"skinparam ParticipantBackgroundColor #{primary}");
            builder.AppendLine($"skinparam ParticipantFontColor #{text}");
            builder.AppendLine($"skinparam ActorBorderColor #{border}");
            builder.AppendLine($"skinparam ActorBackgroundColor #{primary}");
            builder.AppendLine($"skinparam ActorFontColor #{text}");
            builder.AppendLine($"skinparam ClassBorderColor #{border}");
            builder.AppendLine($"skinparam ClassBackgroundColor #{primary}");
            builder.AppendLine($"skinparam ClassFontColor #{text}");
            builder.AppendLine($"skinparam ComponentBorderColor #{border}");
            builder.AppendLine($"skinparam ComponentBackgroundColor #{primary}");
            builder.AppendLine($"skinparam ComponentFontColor #{text}");
            builder.AppendLine($"skinparam PackageBorderColor #{border}");
            builder.AppendLine($"skinparam PackageBackgroundColor #{secondary}");
            builder.AppendLine($"skinparam PackageFontColor #{text}");
            builder.AppendLine($"skinparam NoteBackgroundColor #{note}");
            builder.AppendLine($"skinparam NoteBorderColor #{border}");
            builder.AppendLine($"skinparam NoteFontColor #{text}");
            builder.AppendLine($"skinparam ActivityBorderColor #{border}");
            builder.AppendLine($"skinparam ActivityBackgroundColor #{primary}");
            builder.AppendLine($"skinparam ActivityFontColor #{text}");
            builder.AppendLine($"skinparam StateBorderColor #{border}");
            builder.AppendLine($"skinparam StateBackgroundColor #{primary}");
            builder.AppendLine($"skinparam StateFontColor #{text}");
            builder.AppendLine($"skinparam DatabaseBorderColor #{border}");
            builder.AppendLine($"skinparam DatabaseBackgroundColor #{primary}");
            builder.AppendLine($"skinparam DatabaseFontColor #{text}");
            builder.AppendLine($"skinparam UsecaseBorderColor #{border}");
            builder.AppendLine($"skinparam UsecaseBackgroundColor #{primary}");
            builder.AppendLine($"skinparam UsecaseFontColor #{text}");
            return builder.ToString().TrimEnd();
        }

        private static bool TryGetFirstMeaningfulLine(string source, out string line, out int insertionIndex)
        {
            int index = 0;

            while (index < source.Length)
            {
                int lineEnd = source.IndexOf('\n', index);
                if (lineEnd < 0)
                {
                    lineEnd = source.Length;
                }

                string currentLine = source.Substring(index, lineEnd - index).Trim();
                insertionIndex = lineEnd < source.Length ? lineEnd + 1 : lineEnd;

                if (currentLine.Length > 0)
                {
                    line = currentLine;
                    return true;
                }

                index = insertionIndex;
            }

            line = string.Empty;
            insertionIndex = 0;
            return false;
        }

        private static string NormalizeLineEndings(string source)
        {
            return source.Replace("\r\n", "\n").Replace('\r', '\n');
        }

        private static string ToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
