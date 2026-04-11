// ============================================================
// File:    MarkdownThemeEditor.cs
// Purpose: Custom inspector for MarkdownTheme with side-by-side color comparison and presets.
// Author:  Ahmad Albahar
// ============================================================

using UnityEditor;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Custom editor for <see cref="MarkdownTheme"/> to provide a better user interface for theme editing.
    /// Includes preset buttons and side-by-side comparison of light and dark skins.
    /// </summary>
    [CustomEditor(typeof(MarkdownTheme))]
    public class MarkdownThemeEditor : Editor
    {
        private static bool sEmojiSourcesFoldout = true;

        /// <summary>
        /// Renders the custom inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var theme = (MarkdownTheme)target;

            EditorGUILayout.Space();
            DrawToolbar(theme);
            EditorGUILayout.Space();

            DrawColorTable(theme);
            EditorGUILayout.Space();
            DrawEmojiSourceChain(theme);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(theme);
            }
        }

        private void DrawToolbar(MarkdownTheme theme)
        {
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

            // Row 1: VS Code, GitHub, Monokai
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("VS Code Dark+", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply VS Code Dark+ Theme");
                theme.SetVSCodeDark();
                Repaint();
            }

            if (GUILayout.Button("GitHub Light", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply GitHub Light Theme");
                theme.SetGitHubLight();
                Repaint();
            }

            if (GUILayout.Button("GitHub Dark", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply GitHub Dark Theme");
                theme.SetGitHubDark();
                Repaint();
            }

            if (GUILayout.Button("Monokai Dark", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply Monokai Dark Theme");
                theme.SetMonokaiDark();
                Repaint();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Row 2: Dracula, Nord, OneDark
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Dracula Dark", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply Dracula Dark Theme");
                theme.SetDraculaDark();
                Repaint();
            }

            if (GUILayout.Button("Nord Dark", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply Nord Dark Theme");
                theme.SetNordDark();
                Repaint();
            }

            if (GUILayout.Button("OneDark Dark", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply OneDark Dark Theme");
                theme.SetOneDarkDark();
                Repaint();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Row 3: Solarized
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Solarized Dark", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply Solarized Dark Theme");
                theme.SetSolarizedDark();
                Repaint();
            }

            if (GUILayout.Button("Solarized Light", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply Solarized Light Theme");
                theme.SetSolarizedLight();
                Repaint();
            }

            if (GUILayout.Button("Catppuccin Mocha", GUILayout.Width(120)))
            {
                Undo.RecordObject(theme, "Apply Catppuccin Mocha Dark Theme");
                theme.SetCatppuccinMochaDark();
                Repaint();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawColorTable(MarkdownTheme theme)
        {
            EditorGUILayout.BeginVertical("box");

            // Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Property", EditorStyles.boldLabel, GUILayout.Width(150));
            EditorGUILayout.LabelField("Light Skin", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Dark Skin", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            DrawSeparator();

            // General Section
            EditorGUILayout.LabelField("General Coloring", EditorStyles.miniBoldLabel);
            DrawColorRow("Page Background", ref theme.Light.PageBackground, ref theme.Dark.PageBackground);
            DrawColorRow("Text", ref theme.Light.Text, ref theme.Dark.Text);
            DrawColorRow("Link", ref theme.Light.Link, ref theme.Dark.Link);
            DrawColorRow("Separator", ref theme.Light.Separator, ref theme.Dark.Separator);
            DrawColorRow("Border", ref theme.Light.Border, ref theme.Dark.Border);

            DrawSeparator();

            // Blocks Section
            EditorGUILayout.LabelField("Blocks", EditorStyles.miniBoldLabel);
            DrawColorRow("Code Background", ref theme.Light.CodeBackground, ref theme.Dark.CodeBackground);
            DrawColorRow("Quote Background", ref theme.Light.QuoteBackground, ref theme.Dark.QuoteBackground);
            DrawColorRow("Quote Border", ref theme.Light.QuoteBorder, ref theme.Dark.QuoteBorder);

            DrawSeparator();

            // Headings Section
            EditorGUILayout.LabelField("Headings", EditorStyles.miniBoldLabel);
            DrawColorAndSizeRow("Heading 1", ref theme.Light.Heading1, ref theme.Dark.Heading1, ref theme.Light.Heading1Size, ref theme.Dark.Heading1Size);
            DrawColorAndSizeRow("Heading 2", ref theme.Light.Heading2, ref theme.Dark.Heading2, ref theme.Light.Heading2Size, ref theme.Dark.Heading2Size);
            DrawColorAndSizeRow("Heading 3", ref theme.Light.Heading3, ref theme.Dark.Heading3, ref theme.Light.Heading3Size, ref theme.Dark.Heading3Size);
            DrawColorAndSizeRow("Heading 4", ref theme.Light.Heading4, ref theme.Dark.Heading4, ref theme.Light.Heading4Size, ref theme.Dark.Heading4Size);
            DrawColorAndSizeRow("Heading 5", ref theme.Light.Heading5, ref theme.Dark.Heading5, ref theme.Light.Heading5Size, ref theme.Dark.Heading5Size);
            DrawColorAndSizeRow("Heading 6", ref theme.Light.Heading6, ref theme.Dark.Heading6, ref theme.Light.Heading6Size, ref theme.Dark.Heading6Size);

            DrawSeparator();

            // Tables Section
            EditorGUILayout.LabelField("Tables", EditorStyles.miniBoldLabel);
            DrawColorRow("Header BG", ref theme.Light.TableHeaderBackground, ref theme.Dark.TableHeaderBackground);
            DrawColorRow("Row Odd BG", ref theme.Light.TableRowOddBackground, ref theme.Dark.TableRowOddBackground);
            DrawColorRow("Row Even BG", ref theme.Light.TableRowEvenBackground, ref theme.Dark.TableRowEvenBackground);

            DrawSeparator();

            // Syntax Section
            EditorGUILayout.LabelField("Syntax Highlighting", EditorStyles.miniBoldLabel);
            DrawColorRow("Keyword", ref theme.Light.Keyword, ref theme.Dark.Keyword);
            DrawColorRow("Type", ref theme.Light.Type, ref theme.Dark.Type);
            DrawColorRow("String", ref theme.Light.String, ref theme.Dark.String);
            DrawColorRow("Comment", ref theme.Light.Comment, ref theme.Dark.Comment);
            DrawColorRow("Number", ref theme.Light.Number, ref theme.Dark.Number);
            DrawColorRow("Method", ref theme.Light.Method, ref theme.Dark.Method);

            EditorGUILayout.EndVertical();
        }

        private void DrawEmojiSourceChain(MarkdownTheme theme)
        {
            EditorGUILayout.BeginVertical("box");
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
            sEmojiSourcesFoldout = EditorGUILayout.Foldout(
                sEmojiSourcesFoldout,
                "Emoji Fallback Sources (Unity Versions Older Than Unity 6)",
                true,
                foldoutStyle);

            if (!sEmojiSourcesFoldout)
            {
                EditorGUILayout.HelpBox(
                    "Expand this section to review or customize the CDN source chain used for emoji rendering on Unity 2021/2022.",
                    MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.HelpBox(MarkdownTheme.EmojiSourceChainInstructions, MessageType.Warning);
            EditorGUILayout.LabelField("Template Tokens", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(
                "{emoji}, {codepoints_lower}, {codepoints_upper}, {codepoints_lower_no_vs}, {codepoints_upper_no_vs}",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(2f);

            if (theme.EmojiSourceChain == null)
            {
                theme.EmojiSourceChain = new System.Collections.Generic.List<MarkdownTheme.EmojiSourceTemplate>();
            }

            for (int index = 0; index < theme.EmojiSourceChain.Count; index++)
            {
                MarkdownTheme.EmojiSourceTemplate source = theme.EmojiSourceChain[index];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                source.Enabled = EditorGUILayout.Toggle(source.Enabled, GUILayout.Width(18));
                source.Label = EditorGUILayout.TextField("Label", source.Label);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    Undo.RecordObject(theme, "Remove Emoji Source");
                    theme.EmojiSourceChain.RemoveAt(index);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                source.UrlTemplate = EditorGUILayout.TextField("URL Template", source.UrlTemplate);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Source"))
            {
                Undo.RecordObject(theme, "Add Emoji Source");
                theme.EmojiSourceChain.Add(new MarkdownTheme.EmojiSourceTemplate
                {
                    Label = "Custom Source",
                    Enabled = true,
                    UrlTemplate = string.Empty,
                });
            }

            if (GUILayout.Button("Reset Defaults"))
            {
                Undo.RecordObject(theme, "Reset Emoji Sources");
                theme.ResetEmojiSourceChainDefaults();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawColorRow(string name, ref Color light, ref Color dark)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(150));
            light = EditorGUILayout.ColorField(light);
            dark = EditorGUILayout.ColorField(dark);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawColorAndSizeRow(string name, ref Color lightColor, ref Color darkColor, ref int lightSize, ref int darkSize)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(150));

            EditorGUILayout.BeginVertical();
            lightColor = EditorGUILayout.ColorField(lightColor);
            lightSize = EditorGUILayout.IntField(lightSize);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            darkColor = EditorGUILayout.ColorField(darkColor);
            darkSize = EditorGUILayout.IntField(darkSize);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUILayout.Space(5);
        }
    }
}
