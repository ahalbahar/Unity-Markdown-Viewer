// ============================================================
// File:    MarkdownMenus.cs
// Purpose: Editor menu items for creating Markdown assets.
// Author:  Ahmad Albahar
// ============================================================

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Static class containing menu item definitions for the Markdown viewer.
    /// </summary>
    public static class MarkdownMenus
    {
        private const string DefaultMarkdownTemplate =
            "# New Markdown Document\n\n" +
            "## Overview\n\n" +
            "Write your documentation here.\n";

        private static string GetFilePath(string filename)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (AssetDatabase.IsValidFolder(path) == false)
            {
                path = Path.GetDirectoryName(path);
            }

            return AssetDatabase.GenerateUniqueAssetPath(path + "/" + filename);
        }

        /// <summary>
        /// Creates a new Markdown file in the currently selected folder.
        /// </summary>
        [MenuItem("Assets/Create/Markdown", priority = 0)]
        public static void CreateMarkdown()
        {
            var filepath = GetFilePath("NewMarkdown.md");
            using (var writer = File.CreateText(filepath))
            {
                // CHANGED: Use a built-in template because the standalone repo does not ship Editor Default Resources.
                writer.Write(DefaultMarkdownTemplate);
            }

            AssetDatabase.ImportAsset(filepath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(filepath);
        }
    }
}
