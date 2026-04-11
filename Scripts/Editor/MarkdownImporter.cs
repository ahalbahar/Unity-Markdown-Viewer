// ============================================================
// File:    MarkdownImporter.cs
// Purpose: Scripted importer for .markdown files.
// Author:  Ahmad Albahar
// ============================================================

#if UNITY_2018_1_OR_NEWER

using System.IO;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace AB.MDV
{
    /// <summary>
    /// Custom scripted importer for files with the .markdown extension.
    /// Imports the content as a <see cref="TextAsset"/>.
    /// </summary>
    [ScriptedImporter(1, "markdown")]
    public class MarkdownAssetImporter : ScriptedImporter
    {
        /// <summary>
        /// Handles the import process for the markdown asset.
        /// </summary>
        /// <param name="ctx">The import context provided by Unity.</param>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var content = File.ReadAllText(ctx.assetPath);
            var textAsset = new TextAsset(content);
            
            ctx.AddObjectToAsset("main", textAsset);
            ctx.SetMainObject(textAsset);
        }
    }
}

#endif
