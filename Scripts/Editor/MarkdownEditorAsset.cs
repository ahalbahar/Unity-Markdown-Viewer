// ============================================================
// File:    MarkdownEditorAsset.cs
// Purpose: Custom Inspector for MarkdownAsset (.md) files.
// Author:  Ahmad Albahar
// ============================================================

using UnityEditor;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Custom Inspector for <see cref="MarkdownAsset"/>, providing a specialized viewing experience.
    /// Manages its own <see cref="MarkdownViewer"/> instance and handles UI updates.
    /// Provides a tailored preview specifically for assets of type MarkdownAsset.
    /// </summary>
    [CustomEditor(typeof(MarkdownAsset))]
    public class MarkdownEditorAsset : Editor
    {
        /// <summary>
        /// Optional skin for light mode.
        /// </summary>
        public GUISkin SkinLight;

        /// <summary>
        /// Optional skin for dark mode.
        /// </summary>
        public GUISkin SkinDark;

        private MarkdownViewer mViewer;

        /// <summary>
        /// Initializes the markdown viewer when the editor is enabled.
        /// </summary>
        protected void OnEnable()
        {
            var markdownAsset = target as MarkdownAsset;
            if (markdownAsset == null)
            {
                return;
            }

            var content = markdownAsset.text;
            var path = AssetDatabase.GetAssetPath(target);

            mViewer = new MarkdownViewer(MarkdownPreferences.DarkSkin ? SkinDark : SkinLight, path, content);
            EditorApplication.update += UpdateRequests;
        }

        /// <summary>
        /// Cleans up the viewer and removes update callbacks.
        /// </summary>
        protected void OnDisable()
        {
            if (mViewer != null)
            {
                EditorApplication.update -= UpdateRequests;
                mViewer = null;
            }
        }

        /// <summary>
        /// Indicates that the editor should not use default margins.
        /// </summary>
        /// <returns>Always returns false.</returns>
        public override bool UseDefaultMargins()
        {
            return false;
        }

        /// <summary>
        /// Hides the default Unity Inspector header.
        /// </summary>
        protected override void OnHeaderGUI()
        {
            // Intentionally left empty to hide default header.
        }

        /// <summary>
        /// Renders the custom Inspector UI using the markdown viewer.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (mViewer != null)
            {
                mViewer.Draw();
            }
        }

        /// <summary>
        /// Periodically updates the viewer state and repaints if needed.
        /// </summary>
        private void UpdateRequests()
        {
            if (mViewer != null && mViewer.Update())
            {
                Repaint();
            }
        }
    }
}
