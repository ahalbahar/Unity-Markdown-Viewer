// ============================================================
// File:    MarkdownPreferences.cs
// Purpose: Editor preferences and settings provider for the Markdown viewer.
// Author:  Ahmad Albahar
// ============================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Manages editor-wide preferences for the Markdown viewer.
    /// Provides a settings provider in the Unity Preferences window.
    /// </summary>
    public static class MarkdownPreferences
    {
        private static readonly string KeyJIRA = "AB/MDV/JIRA";
        private static readonly string KeyPipedTables = "AB/MDV/PIPED";
        private static readonly string KeyPipedTablesRequireHeaderSeparator = "AB/MDV/PIPED/USEHSEP";
        private static readonly string KeyHTML = "AB/MDV/HTML";
        private static readonly string KeyDarkSkin = "AB/MDV/DarkSkin";

        private static string mJIRA = string.Empty;
        private static bool mPipedTables = true;
        private static bool mPipedTablesRequireHeaderSeparator = true;
        private static bool mStripHTML = true;
        private static bool mPrefsLoaded = false;
        private static bool mDarkSkin = EditorGUIUtility.isProSkin;
        private static Editor sThemeEditor;

        /// <summary>
        /// Gets the JIRA URL suffix used for issue linking.
        /// </summary>
        public static string JIRA { get { LoadPrefs(); return mJIRA; } }

        /// <summary>
        /// Gets a value indicating whether HTML tags should be stripped from the source.
        /// </summary>
        public static bool StripHTML { get { LoadPrefs(); return mStripHTML; } }

        /// <summary>
        /// Gets a value indicating whether the viewer should use dark skin colors.
        /// </summary>
        public static bool DarkSkin { get { LoadPrefs(); return mDarkSkin; } }

        /// <summary>
        /// Gets a value indicating whether piped tables are enabled.
        /// </summary>
        public static bool PipedTables { get { LoadPrefs(); return mPipedTables; } }

        /// <summary>
        /// Gets a value indicating whether piped tables require a header separator.
        /// </summary>
        public static bool PipedTablesRequireHeaderSeparator { get { LoadPrefs(); return mPipedTablesRequireHeaderSeparator; } }

        /// <summary>
        /// Gets the currently active theme.
        /// </summary>
        public static MarkdownTheme ActiveTheme => MarkdownTheme.Instance;

        private static void LoadPrefs()
        {
            if (!mPrefsLoaded)
            {
                mJIRA = EditorPrefs.GetString(KeyJIRA, "");
                mStripHTML = EditorPrefs.GetBool(KeyHTML, true);
                mPipedTables = EditorPrefs.GetBool(KeyPipedTables, true);
                mPipedTablesRequireHeaderSeparator = EditorPrefs.GetBool(KeyPipedTablesRequireHeaderSeparator, true);
                mDarkSkin = EditorPrefs.GetBool(KeyDarkSkin, EditorGUIUtility.isProSkin);

                mPrefsLoaded = true;
            }
        }

#if UNITY_2019_1_OR_NEWER

        /// <summary>
        /// Settings provider for the Markdown viewer in the Unity Preferences window.
        /// </summary>
        public class MarkdownSettings : SettingsProvider
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MarkdownSettings"/> class.
            /// </summary>
            /// <param name="path">The settings path shown in the Unity preferences window.</param>
            /// <param name="scopes">The scope that owns the stored settings.</param>
            /// <param name="keywords">Optional search keywords for the provider.</param>
            public MarkdownSettings(string path, SettingsScope scopes = SettingsScope.User, IEnumerable<string> keywords = null)
                : base(path, scopes, keywords)
            {
            }

            /// <summary>
            /// Draws the Markdown viewer preference controls.
            /// </summary>
            /// <param name="searchContext">The current search text entered in the preferences window.</param>
            public override void OnGUI(string searchContext)
            {
                DrawPreferences();
            }
        }

        /// <summary>
        /// Settings provider for project-scoped Markdown Viewer configuration.
        /// </summary>
        public class MarkdownProjectSettings : SettingsProvider
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MarkdownProjectSettings"/> class.
            /// </summary>
            /// <param name="path">The project settings path shown in the Unity settings window.</param>
            /// <param name="scopes">The scope that owns the stored settings.</param>
            /// <param name="keywords">Optional search keywords for the provider.</param>
            public MarkdownProjectSettings(string path, SettingsScope scopes = SettingsScope.Project, IEnumerable<string> keywords = null)
                : base(path, scopes, keywords)
            {
            }

            /// <summary>
            /// Draws the project-scoped Markdown theme settings.
            /// </summary>
            /// <param name="searchContext">The current search text entered in the settings window.</param>
            public override void OnGUI(string searchContext)
            {
                DrawThemeSettings();
            }
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new MarkdownSettings("Preferences/AB/Markdown");
        }

        [SettingsProvider]
        private static SettingsProvider CreateProjectSettingsProvider()
        {
            return new MarkdownProjectSettings("Project/AB/Markdown Viewer");
        }
#else
        [PreferenceItem("Markdown")]
#endif
        private static void DrawPreferences()
        {
            LoadPrefs();

            EditorGUI.BeginChangeCheck();

            mJIRA = EditorGUILayout.TextField("JIRA URL", mJIRA);
            mStripHTML = EditorGUILayout.Toggle("Strip HTML", mStripHTML);
            mDarkSkin = EditorGUILayout.Toggle("Dark Skin", mDarkSkin);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Theme Management", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Active Theme", MarkdownTheme.Instance, typeof(MarkdownTheme), false);

            if (GUILayout.Button("Locate Theme Asset"))
            {
                Selection.activeObject = MarkdownTheme.Instance;
                EditorGUIUtility.PingObject(MarkdownTheme.Instance);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(KeyJIRA, mJIRA);
                EditorPrefs.SetBool(KeyHTML, mStripHTML);
                EditorPrefs.SetBool(KeyDarkSkin, mDarkSkin);
                EditorPrefs.SetBool(KeyPipedTables, mPipedTables);
                EditorPrefs.SetBool(KeyPipedTablesRequireHeaderSeparator, mPipedTablesRequireHeaderSeparator);
            }
        }

        private static void DrawThemeSettings()
        {
            var theme = MarkdownTheme.Instance;

            EditorGUILayout.LabelField("Markdown Theme", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure the active Markdown theme asset here or edit the asset directly in the Project window. "
                + "The theme asset also owns the emoji CDN fallback chain used by older Unity versions.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Theme Asset", theme, typeof(MarkdownTheme), false);
            }

            if (GUILayout.Button("Locate Theme Asset"))
            {
                Selection.activeObject = theme;
                EditorGUIUtility.PingObject(theme);
            }

            EditorGUILayout.Space();

            // CHANGED: Reuse the theme asset inspector in Project Settings so both edit paths stay in sync.
            Editor.CreateCachedEditor(theme, null, ref sThemeEditor);
            if (sThemeEditor != null)
            {
                sThemeEditor.OnInspectorGUI();
            }
        }
    }
}
