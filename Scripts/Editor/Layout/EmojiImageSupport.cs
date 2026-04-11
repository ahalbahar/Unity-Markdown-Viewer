// ============================================================
// File:    EmojiImageSupport.cs
// Purpose: Converts emoji grapheme clusters into inline image sources for older Unity editors.
// Author:  Ahmad Albahar
// Notes:   Older IMGUI text rendering does not support full-color emoji glyphs reliably.
// ============================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Provides emoji segmentation and URL generation for inline emoji image rendering.
    /// </summary>
    internal static class EmojiImageSupport
    {
        /// <summary>
        /// Gets a value indicating whether emoji should be rendered as inline images on this Unity version.
        /// </summary>
#if UNITY_6000_0_OR_NEWER
        internal static bool Enabled => false;
#else
        internal static bool Enabled => true;
#endif

        /// <summary>
        /// Splits a text fragment into plain-text and emoji-image segments.
        /// </summary>
        /// <param name="text">The source text fragment.</param>
        /// <returns>An ordered list of segments ready for layout emission.</returns>
        internal static List<EmojiSegment> Split(string text)
        {
            List<EmojiSegment> segments = new List<EmojiSegment>();
            if (string.IsNullOrEmpty(text))
            {
                return segments;
            }

            StringBuilder plainText = new StringBuilder(text.Length);
            int index = 0;
            while (index < text.Length)
            {
                if (TryReadEmojiCluster(text, index, out string emojiCluster, out int nextIndex)
                    && TryCreateEmojiSegment(emojiCluster, out EmojiSegment emojiSegment))
                {
                    FlushPlainText(segments, plainText);
                    segments.Add(emojiSegment);
                    index = nextIndex;
                    continue;
                }

                int codePoint = char.ConvertToUtf32(text, index);
                plainText.Append(char.ConvertFromUtf32(codePoint));
                index += codePoint > 0xFFFF ? 2 : 1;
            }

            FlushPlainText(segments, plainText);
            return segments;
        }

        private static void FlushPlainText(List<EmojiSegment> segments, StringBuilder plainText)
        {
            if (plainText.Length == 0)
            {
                return;
            }

            segments.Add(EmojiSegment.CreateText(plainText.ToString()));
            plainText.Length = 0;
        }

        private static bool TryCreateEmojiSegment(string textElement, out EmojiSegment segment)
        {
            segment = null;
            if (!ContainsEmojiCodePoint(textElement))
            {
                return false;
            }

            List<string> candidateUrls = BuildCandidateUrls(textElement);
            if (candidateUrls.Count == 0)
            {
                return false;
            }

            segment = EmojiSegment.CreateEmoji(textElement, candidateUrls);
            return true;
        }

        private static List<string> BuildCandidateUrls(string textElement)
        {
            List<string> rawCodePoints = new List<string>();
            for (int index = 0; index < textElement.Length; index++)
            {
                int codePoint = char.ConvertToUtf32(textElement, index);
                if (char.IsHighSurrogate(textElement[index]))
                {
                    index++;
                }

                rawCodePoints.Add(codePoint.ToString("x"));
            }

            if (rawCodePoints.Count == 0)
            {
                return new List<string>();
            }

            List<string> noVariationSelectorCodePoints = rawCodePoints.FindAll(codePoint => !string.Equals(codePoint, "fe0f", System.StringComparison.OrdinalIgnoreCase));

            List<string> candidateUrls = new List<string>();
            string emoji = textElement;
            string codepointsLower = BuildCodePointPath(rawCodePoints, uppercase: false);
            string codepointsUpper = BuildCodePointPath(rawCodePoints, uppercase: true);
            string codepointsLowerNoVs = BuildCodePointPath(noVariationSelectorCodePoints, uppercase: false);
            string codepointsUpperNoVs = BuildCodePointPath(noVariationSelectorCodePoints, uppercase: true);

            foreach (MarkdownTheme.EmojiSourceTemplate source in MarkdownTheme.Instance.GetEnabledEmojiSources())
            {
                string url = source.UrlTemplate;
                url = url.Replace("{emoji}", emoji);
                url = url.Replace("{codepoints_lower}", codepointsLower);
                url = url.Replace("{codepoints_upper}", codepointsUpper);
                url = url.Replace("{codepoints_lower_no_vs}", codepointsLowerNoVs);
                url = url.Replace("{codepoints_upper_no_vs}", codepointsUpperNoVs);
                AddCandidate(candidateUrls, url);
            }

            return candidateUrls;
        }

        private static string BuildCodePointPath(List<string> codePoints, bool uppercase)
        {
            if (codePoints == null || codePoints.Count == 0)
            {
                return string.Empty;
            }

            List<string> normalizedCodePoints = new List<string>(codePoints.Count);
            foreach (string codePoint in codePoints)
            {
                normalizedCodePoints.Add(uppercase ? codePoint.ToUpperInvariant() : codePoint.ToLowerInvariant());
            }

            return string.Join("-", normalizedCodePoints);
        }

        private static void AddCandidate(List<string> candidateUrls, string url)
        {
            if (string.IsNullOrEmpty(url) || candidateUrls.Contains(url))
            {
                return;
            }

            candidateUrls.Add(url);
        }

        private static bool TryReadEmojiCluster(string text, int startIndex, out string cluster, out int nextIndex)
        {
            cluster = string.Empty;
            nextIndex = startIndex;

            if (startIndex >= text.Length)
            {
                return false;
            }

            int currentIndex = startIndex;
            int firstCodePoint = ReadCodePoint(text, ref currentIndex);

            if (IsRegionalIndicator(firstCodePoint))
            {
                if (currentIndex < text.Length)
                {
                    int lookaheadIndex = currentIndex;
                    int secondCodePoint = ReadCodePoint(text, ref lookaheadIndex);
                    if (IsRegionalIndicator(secondCodePoint))
                    {
                        currentIndex = lookaheadIndex;
                    }
                }

                cluster = text.Substring(startIndex, currentIndex - startIndex);
                nextIndex = currentIndex;
                return true;
            }

            if (IsKeycapBase(firstCodePoint))
            {
                int keycapIndex = currentIndex;
                if (keycapIndex < text.Length)
                {
                    int lookaheadIndex = keycapIndex;
                    int nextCodePoint = ReadCodePoint(text, ref lookaheadIndex);
                    if (nextCodePoint == 0xFE0F)
                    {
                        keycapIndex = lookaheadIndex;
                    }
                }

                if (keycapIndex < text.Length)
                {
                    int lookaheadIndex = keycapIndex;
                    int nextCodePoint = ReadCodePoint(text, ref lookaheadIndex);
                    if (nextCodePoint == 0x20E3)
                    {
                        cluster = text.Substring(startIndex, lookaheadIndex - startIndex);
                        nextIndex = lookaheadIndex;
                        return true;
                    }
                }

                return false;
            }

            if (!IsEmojiBase(firstCodePoint))
            {
                return false;
            }

            ConsumeEmojiModifiers(text, ref currentIndex);

            while (currentIndex < text.Length)
            {
                int joinerIndex = currentIndex;
                int nextCodePoint = ReadCodePoint(text, ref joinerIndex);
                if (nextCodePoint != 0x200D || joinerIndex >= text.Length)
                {
                    break;
                }

                int emojiIndex = joinerIndex;
                int joinedCodePoint = ReadCodePoint(text, ref emojiIndex);
                if (!IsEmojiBase(joinedCodePoint) && !IsRegionalIndicator(joinedCodePoint))
                {
                    break;
                }

                currentIndex = emojiIndex;
                ConsumeEmojiModifiers(text, ref currentIndex);
            }

            cluster = text.Substring(startIndex, currentIndex - startIndex);
            nextIndex = currentIndex;
            return true;
        }

        private static void ConsumeEmojiModifiers(string text, ref int index)
        {
            while (index < text.Length)
            {
                int lookaheadIndex = index;
                int codePoint = ReadCodePoint(text, ref lookaheadIndex);
                if (codePoint == 0xFE0F || IsSkinToneModifier(codePoint))
                {
                    index = lookaheadIndex;
                    continue;
                }

                break;
            }
        }

        private static int ReadCodePoint(string text, ref int index)
        {
            int codePoint = char.ConvertToUtf32(text, index);
            index += codePoint > 0xFFFF ? 2 : 1;
            return codePoint;
        }

        private static bool IsRegionalIndicator(int codePoint)
        {
            return codePoint >= 0x1F1E6 && codePoint <= 0x1F1FF;
        }

        private static bool IsSkinToneModifier(int codePoint)
        {
            return codePoint >= 0x1F3FB && codePoint <= 0x1F3FF;
        }

        private static bool IsKeycapBase(int codePoint)
        {
            return (codePoint >= '0' && codePoint <= '9') || codePoint == '#' || codePoint == '*';
        }

        private static bool IsEmojiBase(int codePoint)
        {
            return IsRegionalIndicator(codePoint)
                || (codePoint >= 0x1F300 && codePoint <= 0x1FAFF)
                || (codePoint >= 0x2600 && codePoint <= 0x27BF)
                || codePoint == 0x00A9
                || codePoint == 0x00AE
                || codePoint == 0x2122
                || codePoint == 0x2139
                || codePoint == 0x3030
                || codePoint == 0x303D;
        }

        /// <summary>
        /// Returns true when the URL belongs to one of the configured emoji sources.
        /// </summary>
        /// <param name="url">The image URL to test.</param>
        /// <returns>True when the URL matches the current emoji source chain.</returns>
        internal static bool IsConfiguredEmojiSourceUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            foreach (MarkdownTheme.EmojiSourceTemplate source in MarkdownTheme.Instance.GetEnabledEmojiSources())
            {
                if (string.IsNullOrWhiteSpace(source.UrlTemplate))
                {
                    continue;
                }

                int tokenIndex = source.UrlTemplate.IndexOf('{');
                string prefix = tokenIndex >= 0 ? source.UrlTemplate.Substring(0, tokenIndex) : source.UrlTemplate;
                if (!string.IsNullOrEmpty(prefix) && url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsEmojiCodePoint(string textElement)
        {
            bool hasRegionalIndicator = false;
            bool hasKeycapCombiner = false;
            bool hasEmojiRange = false;
            bool hasEmojiJoiner = false;

            for (int index = 0; index < textElement.Length; index++)
            {
                int codePoint = char.ConvertToUtf32(textElement, index);
                if (char.IsHighSurrogate(textElement[index]))
                {
                    index++;
                }

                if (codePoint == 0x200D || codePoint == 0xFE0F)
                {
                    hasEmojiJoiner = true;
                    continue;
                }

                if (codePoint == 0x20E3)
                {
                    hasKeycapCombiner = true;
                    continue;
                }

                if (codePoint >= 0x1F1E6 && codePoint <= 0x1F1FF)
                {
                    hasRegionalIndicator = true;
                    continue;
                }

                if (codePoint >= 0x1F300 && codePoint <= 0x1FAFF)
                {
                    hasEmojiRange = true;
                    continue;
                }

                if (codePoint >= 0x2600 && codePoint <= 0x27BF)
                {
                    hasEmojiRange = true;
                    continue;
                }
            }

            return hasEmojiRange || hasRegionalIndicator || hasKeycapCombiner || hasEmojiJoiner;
        }

        /// <summary>
        /// Represents one layout fragment produced by emoji-aware text splitting.
        /// </summary>
        internal sealed class EmojiSegment
        {
            /// <summary>
            /// Gets the original text content for this segment.
            /// </summary>
            internal string Text { get; private set; }

            /// <summary>
            /// Gets the image URLs when the segment is an emoji image.
            /// </summary>
            internal List<string> ImageUrls { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this segment should render as an emoji image.
            /// </summary>
            internal bool IsEmojiImage => ImageUrls != null && ImageUrls.Count > 0;

            private EmojiSegment()
            {
            }

            internal static EmojiSegment CreateText(string text)
            {
                return new EmojiSegment
                {
                    Text = text,
                    ImageUrls = new List<string>(),
                };
            }

            internal static EmojiSegment CreateEmoji(string text, List<string> imageUrls)
            {
                return new EmojiSegment
                {
                    Text = text,
                    ImageUrls = imageUrls,
                };
            }
        }
    }
}
