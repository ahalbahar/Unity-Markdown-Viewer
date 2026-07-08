// ============================================================
// File:    MarkdownEditorAsset.cs
// Purpose: Custom Inspector for MarkdownAsset (.md) files.
// Author:  Ahmad Albahar
// ============================================================

using System;
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
        private Editor mDefaultEditor;
        private bool mShowOriginalInspector;

        private static readonly GUIContent[] mInspectorViewLabels =
        {
            new GUIContent("Rendered", "Show the rendered markdown preview."),
            new GUIContent("Original", "Show Unity's original TextAsset Inspector.")
        };

        private const string InspectorViewSessionKey = "AB.MDV.MarkdownEditor.ShowOriginalInspector";
        private const float InspectorViewSwitchWidth = 156.0f;
        private const float InspectorViewSwitchPadding = 8.0f;
        private const float InspectorViewSwitchTop = 4.0f;

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

            mShowOriginalInspector = SessionState.GetBool(InspectorViewSessionKey, false);
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

            if (mDefaultEditor != null)
            {
                DestroyImmediate(mDefaultEditor);
                mDefaultEditor = null;
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
            if (mShowOriginalInspector)
            {
                base.OnHeaderGUI();
            }
        }

        /// <summary>
        /// Renders the custom Inspector UI using the markdown viewer.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (mViewer != null)
            {
                if (mShowOriginalInspector)
                {
                    GUILayout.Space(EditorGUIUtility.singleLineHeight + InspectorViewSwitchTop + InspectorViewSwitchPadding);
                    DrawDefaultEditor();
                }
                else
                {
                    mViewer.Draw(InspectorViewSwitchWidth + InspectorViewSwitchPadding);
                }

                DrawInspectorViewSwitch();
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

        /// <summary>
        /// Draws the Inspector view switch as an overlay in the top-right corner.
        /// </summary>
        private void DrawInspectorViewSwitch()
        {
            var width = Mathf.Min(InspectorViewSwitchWidth, Mathf.Max(0.0f, EditorGUIUtility.currentViewWidth - InspectorViewSwitchPadding * 2.0f));
            var rect = new Rect(
                EditorGUIUtility.currentViewWidth - width - InspectorViewSwitchPadding,
                InspectorViewSwitchTop,
                width,
                EditorGUIUtility.singleLineHeight + 2.0f);

            var selected = mShowOriginalInspector ? 1 : 0;
            var next = GUI.Toolbar(rect, selected, mInspectorViewLabels, EditorStyles.toolbarButton);
            if (next == selected)
            {
                return;
            }

            mShowOriginalInspector = next == 1;
            SessionState.SetBool(InspectorViewSessionKey, mShowOriginalInspector);
            Repaint();
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
            else
            {
                base.OnInspectorGUI();
            }
        }
    }
}
