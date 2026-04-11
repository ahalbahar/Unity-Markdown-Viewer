// ============================================================
// File:    MarkdownUtils.cs
// Purpose: Utility functions for path manipulation and normalization.
// Author:  Ahmad Albahar
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Static utility class for common markdown-related operations.
    /// Provides path normalization and texture generation helper methods.
    /// </summary>
    public static class MarkdownUtils
    {
        private static readonly char[] mSeparators = new char[] { '/', '\\' };
        private static readonly Dictionary<Color, Texture2D> sColorTextures = new Dictionary<Color, Texture2D>();

        /// <summary>
        /// Combines two paths with basic normalization.
        /// </summary>
        /// <param name="a">The base path.</param>
        /// <param name="b">The relative path to append.</param>
        /// <param name="separator">The directory separator to use.</param>
        /// <returns>The combined and normalized path.</returns>
        public static string PathCombine(string a, string b, string separator = "/")
        {
            var partsA = (a ?? "").Split(mSeparators, StringSplitOptions.RemoveEmptyEntries);
            var partsB = (b ?? "").Split(mSeparators, StringSplitOptions.RemoveEmptyEntries);

            var combined = partsA.Concat(partsB).Where(el => el != ".");

            var path = new List<string>();

            foreach (var el in combined)
            {
                if (el != "..")
                {
                    path.Add(el);
                }
                else if (path.Count > 0)
                {
                    path.RemoveAt(path.Count - 1);
                }
            }

            return string.Join(separator, path.ToArray());
        }

        /// <summary>
        /// Normalizes a path by resolving '.' and '..' segments.
        /// </summary>
        /// <param name="input">The path to normalize.</param>
        /// <param name="separator">The directory separator to use.</param>
        /// <returns>The normalized path string.</returns>
        public static string PathNormalise(string input, string separator = "/")
        {
            var parts = (input ?? "").Split(mSeparators, StringSplitOptions.RemoveEmptyEntries);
            var path = new List<string>();

            foreach (var el in parts)
            {
                if (el == ".")
                {
                    continue;
                }

                if (el != "..")
                {
                    path.Add(el);
                }
                else if (path.Count > 0)
                {
                    path.RemoveAt(path.Count - 1);
                }
            }

            return string.Join(separator, path.ToArray());
        }

        /// <summary>
        /// Gets a 1x1 Texture2D of the specified color.
        /// Results are cached to avoid redundant texture creation.
        /// </summary>
        /// <param name="color">The color of the texture.</param>
        /// <returns>A 1x1 texture of the specified color.</returns>
        public static Texture2D GetColorTexture(Color color)
        {
            if (sColorTextures.TryGetValue(color, out var tex) && tex != null)
            {
                return tex;
            }

            tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            sColorTextures[color] = tex;
            return tex;
        }
    }
}

