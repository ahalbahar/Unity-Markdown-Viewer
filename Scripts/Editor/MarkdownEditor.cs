// ============================================================
// File:    MarkdownEditor.cs
// Purpose: Custom Inspector for .md and .markdown assets in the Unity Editor.
// Author:  Ahmad Albahar
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AB.MDV
{
    /// <summary>
    /// Custom Inspector for <see cref="TextAsset"/> focusing on Markdown files.
    /// Uses UI Toolkit for modern editors and falls back to IMGUI.
    /// Provides a high-fidelity scrollable preview of the markdown content.
    /// </summary>
    [CustomEditor(typeof(TextAsset))]
    public class MarkdownEditor : Editor
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
        private IMGUIContainer mIMGUI;
        private ScrollView mInspectorScrollView;

        private static readonly List<string> mExtensions = new List<string> { ".md", ".markdown" };

        /// <summary>
        /// Initializes the markdown viewer if the target asset has a supported extension.
        /// </summary>
        protected void OnEnable()
        {
            var textAsset = target as TextAsset;
            if (textAsset == null)
            {
                return;
            }

            var content = textAsset.text;
            var path = AssetDatabase.GetAssetPath(target);
            var ext = Path.GetExtension(path).ToLower();

            if (mExtensions.Contains(ext))
            {
                mViewer = new MarkdownViewer(MarkdownPreferences.DarkSkin ? SkinDark : SkinLight, path, content);
                EditorApplication.update += UpdateRequests;
            }
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

            mIMGUI = null;
            mInspectorScrollView = null;
        }

        /// <summary>
        /// Updates the viewer state and handles pending anchor scrolls within the Inspector.
        /// </summary>
        private void UpdateRequests()
        {
            if (mViewer != null && mViewer.Update())
            {
                Repaint();
            }

            // Apply pending anchor scroll to the Inspector's own ScrollView.
            if (mViewer != null && mIMGUI != null)
            {
                var scrollTarget = mViewer.ConsumePendingScroll();
                if (scrollTarget.HasValue)
                {
                    // Lazily find the Inspector's ScrollView by walking ancestors.
                    if (mInspectorScrollView == null)
                    {
                        mInspectorScrollView = mIMGUI.GetFirstAncestorOfType<ScrollView>();
                    }

                    if (mInspectorScrollView != null)
                    {
                        // block.Rect.y is relative to the top of our IMGUI content.
                        // The Inspector ScrollView scrolls ALL editors, so we need
                        // to add our IMGUIContainer's offset within the scroll content.
                        var contentContainer = mInspectorScrollView.contentContainer;
                        float editorTop = mIMGUI.worldBound.y
                                             - contentContainer.worldBound.y
                                             + mInspectorScrollView.scrollOffset.y;

                        mInspectorScrollView.scrollOffset = new Vector2(
                            0f, editorTop + scrollTarget.Value);
                    }

                    Repaint();
                }
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
        /// Creates the Inspector UI using UI Toolkit.
        /// </summary>
        /// <returns>A VisualElement representing the Inspector UI.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            if (mViewer == null)
            {
                return new IMGUIContainer(DrawDefaultEditor);
            }

            mIMGUI = new IMGUIContainer(() =>
            {
                if (mViewer != null)
                {
                    mViewer.Draw();
                }
            });

            mIMGUI.style.flexShrink = 0;
            return mIMGUI;
        }

        /// <summary>
        /// Pre-Unity 2020 layout header handler.
        /// </summary>
        protected override void OnHeaderGUI()
        {
#if UNITY_2019_2_OR_NEWER && !UNITY_2020_1_OR_NEWER
            DrawEditor();
#endif
        }

        /// <summary>
        /// Legacy IMGUI draw handler.
        /// </summary>
        public override void OnInspectorGUI()
        {
#if !UNITY_2019_2_OR_NEWER || UNITY_2020_1_OR_NEWER
            DrawEditor();
#endif
        }

        private Editor mDefaultEditor;

        /// <summary>
        /// Draws either the markdown viewer or the default text asset editor.
        /// </summary>
        private void DrawEditor()
        {
            if (mViewer != null)
            {
                mViewer.Draw();
            }
            else
            {
                DrawDefaultEditor();
            }
        }

        /// <summary>
        /// Falls back to the default Unity TextAsset inspector.
        /// </summary>
        private void DrawDefaultEditor()
        {
            if (mDefaultEditor == null)
            {
                var inspectorType = Type.GetType("UnityEditor.TextAssetInspector, UnityEditor");
                if (inspectorType != null)
                {
                    mDefaultEditor = CreateEditor(target, inspectorType);
                }
            }

            if (mDefaultEditor != null)
            {
                mDefaultEditor.OnInspectorGUI();
            }
        }
    }
}
