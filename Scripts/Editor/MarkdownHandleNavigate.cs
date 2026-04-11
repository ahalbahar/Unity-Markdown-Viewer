// ============================================================
// File:    MarkdownHandleNavigate.cs
// Purpose: Handles navigation between markdown pages and anchors.
// Author:  Ahmad Albahar
// ============================================================

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using AB.MDV.Layout;

namespace AB.MDV
{
    /// <summary>
    /// Handles navigation logic, including internal anchor links and external file links.
    /// Renamed from HandlerNavigate to match filename.
    /// </summary>
    public class MarkdownHandleNavigate
    {
        /// <summary>
        /// The navigation history manager.
        /// </summary>
        public MarkdownHistory History;

        /// <summary>
        /// The current path of the markdown file being viewed.
        /// </summary>
        public string CurrentPath;

        /// <summary>
        /// Action to scroll the view to a specific vertical position.
        /// </summary>
        public Action<float> ScrollTo;

        /// <summary>
        /// Function to find a layout block by its identifier.
        /// </summary>
        public Func<string, Block> FindBlock;

        /// <summary>
        /// Navigates to a specific page or anchor.
        /// </summary>
        /// <param name="url">The URL or anchor to navigate to.</param>
        public void SelectPage(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            // Internal anchor link
            if (url.StartsWith("#"))
            {
                var lookup = url.ToLower();
                var block = FindBlock?.Invoke(lookup);

                if (block != null)
                {
                    ScrollTo?.Invoke(block.Rect.y);
                }
                else
                {
                    Debug.LogWarning($"[AB.MDV] Unable to find section header '{lookup}'");
                }

                return;
            }

            // Relative or absolute link
            var newPath = string.Empty;

            if (url.StartsWith("/"))
            {
                newPath = url.Substring(1);
            }
            else
            {
                newPath = MarkdownUtils.PathCombine(Path.GetDirectoryName(CurrentPath), url);
            }

            // Load asset
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(newPath);

            if (asset != null)
            {
                History?.Add(newPath);
                Selection.activeObject = asset;
            }
            else
            {
                Debug.LogError(string.Format("[AB.MDV] Could not find asset {0}", newPath));
            }
        }
    }
}
