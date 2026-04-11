// ============================================================
// File:    MarkdownTheme.cs
// Purpose: Singleton ScriptableObject for managing global markdown themes.
// Author:  Ahmad Albahar
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// ScriptableObject that stores theme configuration for the Markdown viewer.
    /// Provides presets for VS Code and GitHub styles and manages light/dark skins.
    /// </summary>
    public class MarkdownTheme : ScriptableObject
    {
        private const string DefaultThemeFolder = "Assets/AB/Unity-Markdown-Viewer/Theme";
        private const string DefaultWritableThemeFolder = "Assets/AB/Unity-Markdown-Viewer/Theme";

        private static MarkdownTheme sInstance;

        /// <summary>
        /// One configurable emoji CDN template used by older Unity versions.
        /// </summary>
        [Serializable]
        public class EmojiSourceTemplate
        {
            /// <summary>
            /// Gets or sets the display label shown in the theme inspector.
            /// </summary>
            public string Label = string.Empty;

            /// <summary>
            /// Gets or sets a value indicating whether the source should be considered during fallback resolution.
            /// </summary>
            public bool Enabled = true;

            /// <summary>
            /// Gets or sets the URL template.
            /// Supported tokens: {emoji}, {codepoints_lower}, {codepoints_upper}, {codepoints_lower_no_vs}, {codepoints_upper_no_vs}.
            /// </summary>
            public string UrlTemplate = string.Empty;
        }

        /// <summary>
        /// Gets the singleton instance of the <see cref="MarkdownTheme"/>.
        /// Searches the asset database or creates a default one if not found.
        /// </summary>
        public static MarkdownTheme Instance
        {
            get
            {
                if (sInstance == null)
                {
                    var guids = AssetDatabase.FindAssets("t:MarkdownTheme");
                    if (guids.Length > 0)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        sInstance = AssetDatabase.LoadAssetAtPath<MarkdownTheme>(path);
                    }

                    if (sInstance == null)
                    {
                        sInstance = CreateDefaultTheme();
                    }
                }

                return sInstance;
            }
        }

        private static MarkdownTheme CreateDefaultTheme()
        {
            var theme = CreateInstance<MarkdownTheme>();
            theme.name = "MarkdownTheme";
            theme.SetVSCodeDark();
            theme.SetGitHubLight();

            // CHANGED: Resolve a writable theme asset location so the plugin works from both Assets/ and Packages/.
            var folderPath = ResolveWritableThemeFolder();
            EnsureAssetFolder(folderPath);

            var assetPath = $"{folderPath}/MarkdownTheme.asset";
            AssetDatabase.CreateAsset(theme, assetPath);
            AssetDatabase.SaveAssets();

            return theme;
        }

        private static void EnsureAssetFolder(string assetFolderPath)
        {
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            var segments = assetFolderPath.Split('/');
            if (segments.Length == 0)
            {
                throw new InvalidOperationException($"Invalid asset folder path: {assetFolderPath}");
            }

            var currentPath = segments[0];
            if (!AssetDatabase.IsValidFolder(currentPath))
            {
                throw new InvalidOperationException($"Asset root folder not found: {currentPath}");
            }

            for (var index = 1; index < segments.Length; index++)
            {
                var nextPath = $"{currentPath}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[index]);
                }

                currentPath = nextPath;
            }
        }

        private static string ResolveWritableThemeFolder()
        {
            if (AssetDatabase.IsValidFolder(DefaultThemeFolder))
            {
                return DefaultThemeFolder;
            }

            var packageRootPath = ResolvePackageRootPath();
            if (!string.IsNullOrEmpty(packageRootPath) && AssetDatabase.IsValidFolder($"{packageRootPath}/Theme"))
            {
                return DefaultWritableThemeFolder;
            }

            return DefaultWritableThemeFolder;
        }

        private static string ResolvePackageRootPath()
        {
            var packageGuids = AssetDatabase.FindAssets("AB.MDV t:AssemblyDefinitionAsset");
            var packagePath = packageGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => string.Equals(Path.GetFileName(path), "AB.MDV.asmdef", StringComparison.OrdinalIgnoreCase));

            return string.IsNullOrEmpty(packagePath)
                ? string.Empty
                : Path.GetDirectoryName(packagePath)?.Replace('\\', '/') ?? string.Empty;
        }

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Defines the color and typography settings for a specific theme skin (light or dark).
        /// </summary>
        [Serializable]
        public class ThemeColors
        {
            #region General

            [Header("General")]
            /// <summary>
            /// Gets or sets the background color used for the document canvas.
            /// </summary>
            public Color PageBackground = new Color(0.118f, 0.118f, 0.118f);

            /// <summary>
            /// Gets or sets the default body text color.
            /// </summary>
            public Color Text = new Color(0.831f, 0.831f, 0.831f);

            /// <summary>
            /// Gets or sets the color used for clickable links.
            /// </summary>
            public Color Link = new Color(0.341f, 0.620f, 1.000f);

            /// <summary>
            /// Gets or sets the color used for separators such as horizontal rules.
            /// </summary>
            public Color Separator = new Color(0.267f, 0.267f, 0.267f);

            /// <summary>
            /// Gets or sets the default border color for framed UI elements.
            /// </summary>
            public Color Border = new Color(0.290f, 0.290f, 0.290f);

            #endregion

            #region Typography

            [Header("Typography")]
            /// <summary>
            /// Gets or sets the base font size used for paragraph content.
            /// </summary>
            public int TextSize = 13;

            [SerializeField]
            private RectOffset mMargin;

            /// <summary>
            /// Gets or sets the outer margin of the markdown document.
            /// </summary>
            public RectOffset Margin
            {
                get
                {
                    if (mMargin == null) mMargin = new RectOffset(16, 16, 12, 12);
                    return mMargin;
                }
                set => mMargin = value;
            }

            #endregion

            #region Blocks

            [Header("Blocks")]
            /// <summary>
            /// Gets or sets the background color used for inline and fenced code.
            /// </summary>
            public Color CodeBackground = new Color(0.137f, 0.137f, 0.137f);

            /// <summary>
            /// Gets or sets the border color used for fenced code blocks.
            /// </summary>
            public Color CodeBlockBorder = new Color(0.220f, 0.220f, 0.220f);

            /// <summary>
            /// Gets or sets the background color used for block quotes.
            /// </summary>
            public Color QuoteBackground = new Color(0.165f, 0.165f, 0.165f);

            /// <summary>
            /// Gets or sets the accent border color used for block quotes.
            /// </summary>
            public Color QuoteBorder = new Color(0.380f, 0.380f, 0.380f);

            #endregion

            #region Headings

            [Header("Headings")]
            /// <summary>
            /// Gets or sets the color of first-level headings.
            /// </summary>
            public Color Heading1 = new Color(0.337f, 0.611f, 0.839f);

            /// <summary>
            /// Gets or sets the font size of first-level headings.
            /// </summary>
            public int Heading1Size = 28;

            /// <summary>
            /// Gets or sets the color of second-level headings.
            /// </summary>
            public Color Heading2 = new Color(0.337f, 0.611f, 0.839f);

            /// <summary>
            /// Gets or sets the font size of second-level headings.
            /// </summary>
            public int Heading2Size = 22;

            /// <summary>
            /// Gets or sets the color of third-level headings.
            /// </summary>
            public Color Heading3 = new Color(0.862f, 0.862f, 0.666f);

            /// <summary>
            /// Gets or sets the font size of third-level headings.
            /// </summary>
            public int Heading3Size = 18;

            /// <summary>
            /// Gets or sets the color of fourth-level headings.
            /// </summary>
            public Color Heading4 = new Color(0.862f, 0.862f, 0.666f);

            /// <summary>
            /// Gets or sets the font size of fourth-level headings.
            /// </summary>
            public int Heading4Size = 16;

            /// <summary>
            /// Gets or sets the color of fifth-level headings.
            /// </summary>
            public Color Heading5 = new Color(0.862f, 0.862f, 0.666f);

            /// <summary>
            /// Gets or sets the font size of fifth-level headings.
            /// </summary>
            public int Heading5Size = 14;

            /// <summary>
            /// Gets or sets the color of sixth-level headings.
            /// </summary>
            public Color Heading6 = new Color(0.862f, 0.862f, 0.666f);

            /// <summary>
            /// Gets or sets the font size of sixth-level headings.
            /// </summary>
            public int Heading6Size = 13;

            #endregion

            #region Tables

            [Header("Tables")]
            /// <summary>
            /// Gets or sets the background color of table header rows.
            /// </summary>
            public Color TableHeaderBackground = new Color(0.200f, 0.200f, 0.200f);

            /// <summary>
            /// Gets or sets the background color of odd-numbered table rows.
            /// </summary>
            public Color TableRowOddBackground = new Color(0.150f, 0.150f, 0.150f);

            /// <summary>
            /// Gets or sets the background color of even-numbered table rows.
            /// </summary>
            public Color TableRowEvenBackground = new Color(0.118f, 0.118f, 0.118f);

            #endregion

            #region Syntax Highlighting

            [Header("Syntax Highlighting")]
            /// <summary>
            /// Gets or sets the syntax color used for language keywords.
            /// </summary>
            public Color Keyword = FromHex("#569CD6");

            /// <summary>
            /// Gets or sets the syntax color used for type names.
            /// </summary>
            public Color Type = FromHex("#4EC9B0");

            /// <summary>
            /// Gets or sets the syntax color used for string literals.
            /// </summary>
            public Color String = FromHex("#CE9178");

            /// <summary>
            /// Gets or sets the syntax color used for comments.
            /// </summary>
            public Color Comment = FromHex("#6A9955");

            /// <summary>
            /// Gets or sets the syntax color used for numeric literals.
            /// </summary>
            public Color Number = FromHex("#B5CEA8");

            /// <summary>
            /// Gets or sets the syntax color used for method names.
            /// </summary>
            public Color Method = FromHex("#DCDCAA");

            /// <summary>
            /// Gets or sets the syntax color used for preprocessors and directives.
            /// </summary>
            public Color Preprocessor = FromHex("#C586C0");

            #endregion

            #region Inline Styles

            [Header("Inline Styles")]
            /// <summary>
            /// Gets or sets the background color used for highlighted text.
            /// </summary>
            public Color Highlight = new Color(1.0f, 0.9f, 0.0f, 0.35f);

            /// <summary>
            /// Gets or sets the text color used inside highlighted content.
            /// </summary>
            public Color HighlightText = new Color(0.0f, 0.0f, 0.0f, 1.0f);

            /// <summary>
            /// Gets or sets the color used for underline decoration.
            /// </summary>
            public Color UnderlineColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

            /// <summary>
            /// Gets or sets the color used for subscript text.
            /// </summary>
            public Color SubscriptColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);

            /// <summary>
            /// Gets or sets the color used for superscript text.
            /// </summary>
            public Color SuperscriptColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);

            #endregion

            #region Admonitions

            [Header("Admonitions")]
            /// <summary>
            /// Gets or sets the border color used for note admonitions.
            /// </summary>
            public Color AdmonitionNoteBorder = FromHex("#1f6feb");

            /// <summary>
            /// Gets or sets the background color used for note admonitions.
            /// </summary>
            public Color AdmonitionNoteBackground = FromHex("#0d1117");

            /// <summary>
            /// Gets or sets the border color used for tip admonitions.
            /// </summary>
            public Color AdmonitionTipBorder = FromHex("#3fb950");

            /// <summary>
            /// Gets or sets the background color used for tip admonitions.
            /// </summary>
            public Color AdmonitionTipBackground = FromHex("#0d1117");

            /// <summary>
            /// Gets or sets the border color used for warning admonitions.
            /// </summary>
            public Color AdmonitionWarnBorder = FromHex("#d29922");

            /// <summary>
            /// Gets or sets the background color used for warning admonitions.
            /// </summary>
            public Color AdmonitionWarnBackground = FromHex("#0d1117");

            /// <summary>
            /// Gets or sets the border color used for important admonitions.
            /// </summary>
            public Color AdmonitionImportantBorder = FromHex("#8957e5");

            /// <summary>
            /// Gets or sets the background color used for important admonitions.
            /// </summary>
            public Color AdmonitionImportantBackground = FromHex("#0d1117");

            /// <summary>
            /// Gets or sets the border color used for caution admonitions.
            /// </summary>
            public Color AdmonitionCautionBorder = FromHex("#f85149");

            /// <summary>
            /// Gets or sets the background color used for caution admonitions.
            /// </summary>
            public Color AdmonitionCautionBackground = FromHex("#0d1117");

            #endregion
        }

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Color settings for the Unity Editor's light skin.
        /// </summary>
        public ThemeColors Light = new ThemeColors();

        /// <summary>
        /// Color settings for the Unity Editor's dark skin.
        /// </summary>
        public ThemeColors Dark = new ThemeColors();

        /// <summary>
        /// Gets the active theme colors based on the current Unity Editor skin.
        /// </summary>
        public ThemeColors Active => EditorGUIUtility.isProSkin ? Dark : Light;

        /// <summary>
        /// Gets or sets the ordered emoji source chain used by older Unity versions when native color emoji is unavailable.
        /// </summary>
        public List<EmojiSourceTemplate> EmojiSourceChain = new List<EmojiSourceTemplate>();

        /// <summary>
        /// Gets the help text shown in the editor for emoji source configuration.
        /// </summary>
        public static string EmojiSourceChainInstructions =>
            "Used only in Unity versions older than Unity 6. Older IMGUI inspectors cannot render full-color emoji reliably, "
            + "so the viewer can resolve emoji through a configurable CDN source chain instead. Sources are tried in order until one succeeds; "
            + "if all sources fail, the viewer falls back to plain text. Supported tokens: {emoji}, {codepoints_lower}, {codepoints_upper}, "
            + "{codepoints_lower_no_vs}, {codepoints_upper_no_vs}. Put a grapheme-aware provider first, then codepoint-based fallbacks after it.";

        private void OnValidate()
        {
            if (Light.Margin == null) SetGitHubLight();
            if (Dark.Margin == null) SetVSCodeDark();
            EnsureEmojiSourceChainDefaults();
        }

        private void OnEnable()
        {
            EnsureEmojiSourceChainDefaults();
        }

        /// <summary>
        /// Returns the enabled emoji source templates in evaluation order.
        /// </summary>
        /// <returns>The configured emoji source chain.</returns>
        public IReadOnlyList<EmojiSourceTemplate> GetEnabledEmojiSources()
        {
            EnsureEmojiSourceChainDefaults();
            return EmojiSourceChain.Where(source => source != null && source.Enabled && !string.IsNullOrWhiteSpace(source.UrlTemplate)).ToList();
        }

        /// <summary>
        /// Resets the emoji fallback source chain to the built-in defaults.
        /// </summary>
        public void ResetEmojiSourceChainDefaults()
        {
            EmojiSourceChain = new List<EmojiSourceTemplate>();
            EnsureEmojiSourceChainDefaults();
        }

        private void EnsureEmojiSourceChainDefaults()
        {
            if (EmojiSourceChain == null)
            {
                EmojiSourceChain = new List<EmojiSourceTemplate>();
            }

            if (EmojiSourceChain.Count > 0)
            {
                return;
            }

            EmojiSourceChain.Add(new EmojiSourceTemplate
            {
                Label = "Emoji CDN (Google)",
                Enabled = true,
                UrlTemplate = "https://emoji-cdn.mqrio.dev/{emoji}?style=google",
            });
            EmojiSourceChain.Add(new EmojiSourceTemplate
            {
                Label = "Emoji CDN (OpenMoji)",
                Enabled = true,
                UrlTemplate = "https://emoji-cdn.mqrio.dev/{emoji}?style=openmoji",
            });
            EmojiSourceChain.Add(new EmojiSourceTemplate
            {
                Label = "OpenMoji PNG",
                Enabled = true,
                UrlTemplate = "https://cdn.jsdelivr.net/gh/hfg-gmuend/openmoji@master/color/72x72/{codepoints_upper}.png",
            });
            EmojiSourceChain.Add(new EmojiSourceTemplate
            {
                Label = "OpenMoji PNG (No VS)",
                Enabled = true,
                UrlTemplate = "https://cdn.jsdelivr.net/gh/hfg-gmuend/openmoji@master/color/72x72/{codepoints_upper_no_vs}.png",
            });
            EmojiSourceChain.Add(new EmojiSourceTemplate
            {
                Label = "Twemoji PNG",
                Enabled = true,
                UrlTemplate = "https://cdn.jsdelivr.net/gh/jdecked/twemoji@17.0.1/assets/72x72/{codepoints_lower}.png",
            });
            EmojiSourceChain.Add(new EmojiSourceTemplate
            {
                Label = "Twemoji PNG (No VS)",
                Enabled = true,
                UrlTemplate = "https://cdn.jsdelivr.net/gh/jdecked/twemoji@17.0.1/assets/72x72/{codepoints_lower_no_vs}.png",
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // Presets
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the default VS Code-inspired dark theme preset to the dark skin.
        /// </summary>
        public void SetVSCodeDark()
        {
            Dark.PageBackground  = FromHex( "#1E1E1E" );
            Dark.Text            = FromHex( "#D4D4D4" );
            Dark.Link            = FromHex( "#3794FF" );
            Dark.Separator       = FromHex( "#444444" );
            Dark.Border          = FromHex( "#454545" );

            Dark.TextSize = 14;
            Dark.Margin   = new RectOffset( 16, 16, 12, 12 );

            Dark.CodeBackground  = FromHex( "#2D2D2D" );
            Dark.CodeBlockBorder = FromHex( "#3E3E3E" );
            Dark.QuoteBackground = FromHex( "#252526" );
            Dark.QuoteBorder     = FromHex( "#454545" );

            Dark.Heading1     = FromHex( "#569CD6" ); Dark.Heading1Size = 28;
            Dark.Heading2     = FromHex( "#569CD6" ); Dark.Heading2Size = 22;
            Dark.Heading3     = FromHex( "#DCDCAA" ); Dark.Heading3Size = 18;
            Dark.Heading4     = FromHex( "#DCDCAA" ); Dark.Heading4Size = 16;
            Dark.Heading5     = FromHex( "#DCDCAA" ); Dark.Heading5Size = 14;
            Dark.Heading6     = FromHex( "#DCDCAA" ); Dark.Heading6Size = 13;

            Dark.TableHeaderBackground = FromHex( "#333333" );
            Dark.TableRowOddBackground  = FromHex( "#1E1E1E" );
            Dark.TableRowEvenBackground = FromHex( "#252526" );

            Dark.Keyword      = FromHex( "#569CD6" );
            Dark.Type         = FromHex( "#4EC9B0" );
            Dark.String       = FromHex( "#CE9178" );
            Dark.Comment      = FromHex( "#6A9955" );
            Dark.Number       = FromHex( "#B5CEA8" );
            Dark.Method       = FromHex( "#DCDCAA" );
            Dark.Preprocessor = FromHex( "#C586C0" );

            Dark.Highlight        = new Color( 1.0f, 0.9f, 0.0f, 0.30f );
            Dark.HighlightText    = FromHex( "#000000" );
            Dark.UnderlineColor   = FromHex( "#D4D4D4" );
            Dark.SubscriptColor   = FromHex( "#9CDCFE" );
            Dark.SuperscriptColor = FromHex( "#9CDCFE" );

            Dark.AdmonitionNoteBorder       = FromHex( "#3794FF" );
            Dark.AdmonitionNoteBackground   = FromHex( "#1A2332" );
            Dark.AdmonitionTipBorder        = FromHex( "#4EC994" );
            Dark.AdmonitionTipBackground    = FromHex( "#1A2A1E" );
            Dark.AdmonitionWarnBorder       = FromHex( "#CCA700" );
            Dark.AdmonitionWarnBackground   = FromHex( "#2A2200" );
            Dark.AdmonitionImportantBorder  = FromHex( "#B267E6" );
            Dark.AdmonitionImportantBackground = FromHex( "#200D33" );
            Dark.AdmonitionCautionBorder    = FromHex( "#F44747" );
            Dark.AdmonitionCautionBackground = FromHex( "#330D0D" );
        }

        /// <summary>
        /// Applies the GitHub light theme preset to the light skin.
        /// </summary>
        public void SetGitHubLight()
        {
            Light.PageBackground  = FromHex( "#FFFFFF" );
            Light.Text            = FromHex( "#24292f" );
            Light.Link            = FromHex( "#0969da" );
            Light.Separator       = FromHex( "#d0d7de" );
            Light.Border          = FromHex( "#d0d7de" );

            Light.TextSize = 14;
            Light.Margin   = new RectOffset( 20, 20, 16, 16 );

            Light.CodeBackground  = FromHex( "#f6f8fa" );
            Light.CodeBlockBorder = FromHex( "#d0d7de" );
            Light.QuoteBackground = FromHex( "#ffffff" );
            Light.QuoteBorder     = FromHex( "#d0d7de" );

            Light.Heading1     = FromHex( "#24292f" ); Light.Heading1Size = 28;
            Light.Heading2     = FromHex( "#24292f" ); Light.Heading2Size = 22;
            Light.Heading3     = FromHex( "#24292f" ); Light.Heading3Size = 18;
            Light.Heading4     = FromHex( "#24292f" ); Light.Heading4Size = 16;
            Light.Heading5     = FromHex( "#24292f" ); Light.Heading5Size = 14;
            Light.Heading6     = FromHex( "#24292f" ); Light.Heading6Size = 13;

            Light.TableHeaderBackground = FromHex( "#f6f8fa" );
            Light.TableRowOddBackground  = FromHex( "#ffffff" );
            Light.TableRowEvenBackground = FromHex( "#f6f8fa" );

            Light.Keyword      = FromHex( "#cf222e" );
            Light.Type         = FromHex( "#953800" );
            Light.String       = FromHex( "#0a3069" );
            Light.Comment      = FromHex( "#6e7781" );
            Light.Number       = FromHex( "#0550ae" );
            Light.Method       = FromHex( "#8250df" );
            Light.Preprocessor = FromHex( "#8250df" );

            Light.Highlight        = new Color( 1.0f, 0.85f, 0.0f, 0.40f );
            Light.HighlightText    = FromHex( "#24292f" );
            Light.UnderlineColor   = FromHex( "#24292f" );
            Light.SubscriptColor   = FromHex( "#57606a" );
            Light.SuperscriptColor = FromHex( "#57606a" );

            Light.AdmonitionNoteBorder       = FromHex( "#0969da" );
            Light.AdmonitionNoteBackground   = FromHex( "#ddf4ff" );
            Light.AdmonitionTipBorder        = FromHex( "#1a7f37" );
            Light.AdmonitionTipBackground    = FromHex( "#dafbe1" );
            Light.AdmonitionWarnBorder       = FromHex( "#9a6700" );
            Light.AdmonitionWarnBackground   = FromHex( "#fff8c5" );
            Light.AdmonitionImportantBorder  = FromHex( "#8250df" );
            Light.AdmonitionImportantBackground = FromHex( "#fbefff" );
            Light.AdmonitionCautionBorder    = FromHex( "#cf222e" );
            Light.AdmonitionCautionBackground = FromHex( "#ffebe9" );
        }

        /// <summary>
        /// Applies the GitHub dark theme preset to the dark skin.
        /// </summary>
        public void SetGitHubDark()
        {
            Dark.PageBackground  = FromHex( "#0d1117" );
            Dark.Text            = FromHex( "#c9d1d9" );
            Dark.Link            = FromHex( "#58a6ff" );
            Dark.Separator       = FromHex( "#30363d" );
            Dark.Border          = FromHex( "#30363d" );

            Dark.TextSize = 14;
            Dark.Margin   = new RectOffset( 20, 20, 16, 16 );

            Dark.CodeBackground  = FromHex( "#161b22" );
            Dark.CodeBlockBorder = FromHex( "#30363d" );
            Dark.QuoteBackground = FromHex( "#0d1117" );
            Dark.QuoteBorder     = FromHex( "#30363d" );

            Dark.Heading1     = FromHex( "#c9d1d9" ); Dark.Heading1Size = 28;
            Dark.Heading2     = FromHex( "#c9d1d9" ); Dark.Heading2Size = 22;
            Dark.Heading3     = FromHex( "#c9d1d9" ); Dark.Heading3Size = 18;
            Dark.Heading4     = FromHex( "#c9d1d9" ); Dark.Heading4Size = 16;
            Dark.Heading5     = FromHex( "#c9d1d9" ); Dark.Heading5Size = 14;
            Dark.Heading6     = FromHex( "#c9d1d9" ); Dark.Heading6Size = 13;

            Dark.TableHeaderBackground = FromHex( "#161b22" );
            Dark.TableRowOddBackground  = FromHex( "#0d1117" );
            Dark.TableRowEvenBackground = FromHex( "#161b22" );

            Dark.Keyword      = FromHex( "#ff7b72" );
            Dark.Type         = FromHex( "#ffa657" );
            Dark.String       = FromHex( "#a5d6ff" );
            Dark.Comment      = FromHex( "#8b949e" );
            Dark.Number       = FromHex( "#79c0ff" );
            Dark.Method       = FromHex( "#d2a8ff" );
            Dark.Preprocessor = FromHex( "#d2a8ff" );

            Dark.Highlight        = new Color( 1.0f, 0.9f, 0.0f, 0.28f );
            Dark.HighlightText    = FromHex( "#000000" );
            Dark.UnderlineColor   = FromHex( "#c9d1d9" );
            Dark.SubscriptColor   = FromHex( "#79c0ff" );
            Dark.SuperscriptColor = FromHex( "#79c0ff" );

            Dark.AdmonitionNoteBorder       = FromHex( "#1f6feb" );
            Dark.AdmonitionNoteBackground   = FromHex( "#0d1117" );
            Dark.AdmonitionTipBorder        = FromHex( "#3fb950" );
            Dark.AdmonitionTipBackground    = FromHex( "#0d1117" );
            Dark.AdmonitionWarnBorder       = FromHex( "#d29922" );
            Dark.AdmonitionWarnBackground   = FromHex( "#0d1117" );
            Dark.AdmonitionImportantBorder  = FromHex( "#8957e5" );
            Dark.AdmonitionImportantBackground = FromHex( "#0d1117" );
            Dark.AdmonitionCautionBorder    = FromHex( "#f85149" );
            Dark.AdmonitionCautionBackground = FromHex( "#0d1117" );
        }

        /// <summary>
        /// Applies the Monokai dark theme preset to the dark skin.
        /// </summary>
        public void SetMonokaiDark()
        {
            Dark.PageBackground  = FromHex( "#272822" );
            Dark.Text            = FromHex( "#F8F8F2" );
            Dark.Link            = FromHex( "#66D9EF" );
            Dark.Separator       = FromHex( "#49483E" );
            Dark.Border          = FromHex( "#49483E" );

            Dark.TextSize = 14;
            Dark.Margin   = new RectOffset( 16, 16, 12, 12 );

            Dark.CodeBackground  = FromHex( "#1E1F1C" );
            Dark.CodeBlockBorder = FromHex( "#49483E" );
            Dark.QuoteBackground = FromHex( "#2D2E27" );
            Dark.QuoteBorder     = FromHex( "#75715E" );

            Dark.Heading1     = FromHex( "#A6E22E" ); Dark.Heading1Size = 28;
            Dark.Heading2     = FromHex( "#A6E22E" ); Dark.Heading2Size = 22;
            Dark.Heading3     = FromHex( "#E6DB74" ); Dark.Heading3Size = 18;
            Dark.Heading4     = FromHex( "#E6DB74" ); Dark.Heading4Size = 16;
            Dark.Heading5     = FromHex( "#E6DB74" ); Dark.Heading5Size = 14;
            Dark.Heading6     = FromHex( "#E6DB74" ); Dark.Heading6Size = 13;

            Dark.TableHeaderBackground = FromHex( "#3E3D32" );
            Dark.TableRowOddBackground  = FromHex( "#272822" );
            Dark.TableRowEvenBackground = FromHex( "#2D2E27" );

            Dark.Keyword      = FromHex( "#F92672" );
            Dark.Type         = FromHex( "#66D9EF" );
            Dark.String       = FromHex( "#E6DB74" );
            Dark.Comment      = FromHex( "#75715E" );
            Dark.Number       = FromHex( "#AE81FF" );
            Dark.Method       = FromHex( "#A6E22E" );
            Dark.Preprocessor = FromHex( "#F92672" );

            Dark.Highlight        = new Color( 0.647f, 0.498f, 1.0f, 0.28f );
            Dark.HighlightText    = FromHex( "#F8F8F2" );
            Dark.UnderlineColor   = FromHex( "#F8F8F2" );
            Dark.SubscriptColor   = FromHex( "#66D9EF" );
            Dark.SuperscriptColor = FromHex( "#66D9EF" );

            Dark.AdmonitionNoteBorder       = FromHex( "#66D9EF" );
            Dark.AdmonitionNoteBackground   = FromHex( "#272822" );
            Dark.AdmonitionTipBorder        = FromHex( "#A6E22E" );
            Dark.AdmonitionTipBackground    = FromHex( "#272822" );
            Dark.AdmonitionWarnBorder       = FromHex( "#E6DB74" );
            Dark.AdmonitionWarnBackground   = FromHex( "#272822" );
            Dark.AdmonitionImportantBorder  = FromHex( "#AE81FF" );
            Dark.AdmonitionImportantBackground = FromHex( "#272822" );
            Dark.AdmonitionCautionBorder    = FromHex( "#F92672" );
            Dark.AdmonitionCautionBackground = FromHex( "#272822" );
        }

        /// <summary>
        /// Applies the Dracula dark theme preset to the dark skin.
        /// </summary>
        public void SetDraculaDark()
        {
            Dark.PageBackground  = FromHex( "#282A36" );
            Dark.Text            = FromHex( "#F8F8F2" );
            Dark.Link            = FromHex( "#8BE9FD" );
            Dark.Separator       = FromHex( "#44475A" );
            Dark.Border          = FromHex( "#44475A" );

            Dark.TextSize = 14;
            Dark.Margin   = new RectOffset( 16, 16, 12, 12 );

            Dark.CodeBackground  = FromHex( "#21222C" );
            Dark.CodeBlockBorder = FromHex( "#44475A" );
            Dark.QuoteBackground = FromHex( "#2D2F3F" );
            Dark.QuoteBorder     = FromHex( "#6272A4" );

            Dark.Heading1     = FromHex( "#BD93F9" ); Dark.Heading1Size = 28;
            Dark.Heading2     = FromHex( "#BD93F9" ); Dark.Heading2Size = 22;
            Dark.Heading3     = FromHex( "#FFB86C" ); Dark.Heading3Size = 18;
            Dark.Heading4     = FromHex( "#FFB86C" ); Dark.Heading4Size = 16;
            Dark.Heading5     = FromHex( "#FFB86C" ); Dark.Heading5Size = 14;
            Dark.Heading6     = FromHex( "#FFB86C" ); Dark.Heading6Size = 13;

            Dark.TableHeaderBackground = FromHex( "#44475A" );
            Dark.TableRowOddBackground  = FromHex( "#282A36" );
            Dark.TableRowEvenBackground = FromHex( "#2D2F3F" );

            Dark.Keyword      = FromHex( "#FF79C6" );
            Dark.Type         = FromHex( "#8BE9FD" );
            Dark.String       = FromHex( "#50FA7B" );
            Dark.Comment      = FromHex( "#6272A4" );
            Dark.Number       = FromHex( "#BD93F9" );
            Dark.Method       = FromHex( "#FFB86C" );
            Dark.Preprocessor = FromHex( "#FF79C6" );

            Dark.Highlight        = new Color( 0.741f, 0.576f, 0.976f, 0.28f );
            Dark.HighlightText    = FromHex( "#F8F8F2" );
            Dark.UnderlineColor   = FromHex( "#F8F8F2" );
            Dark.SubscriptColor   = FromHex( "#8BE9FD" );
            Dark.SuperscriptColor = FromHex( "#8BE9FD" );

            Dark.AdmonitionNoteBorder       = FromHex( "#8BE9FD" );
            Dark.AdmonitionNoteBackground   = FromHex( "#282A36" );
            Dark.AdmonitionTipBorder        = FromHex( "#50FA7B" );
            Dark.AdmonitionTipBackground    = FromHex( "#282A36" );
            Dark.AdmonitionWarnBorder       = FromHex( "#FFB86C" );
            Dark.AdmonitionWarnBackground   = FromHex( "#282A36" );
            Dark.AdmonitionImportantBorder  = FromHex( "#BD93F9" );
            Dark.AdmonitionImportantBackground = FromHex( "#282A36" );
            Dark.AdmonitionCautionBorder    = FromHex( "#FF5555" );
            Dark.AdmonitionCautionBackground = FromHex( "#282A36" );
        }

        // ─────────────────────────────────────────────────────────────────────
        // Nord — Arctic cool palette (https://www.nordtheme.com)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the Nord dark theme preset to the dark skin.
        /// </summary>
        public void SetNordDark()
        {
            Dark.PageBackground  = FromHex( "#2E3440" );
            Dark.Text            = FromHex( "#D8DEE9" );
            Dark.Link            = FromHex( "#88C0D0" );
            Dark.Separator       = FromHex( "#4C566A" );
            Dark.Border          = FromHex( "#4C566A" );

            Dark.TextSize = 14;
            Dark.Margin   = new RectOffset( 16, 16, 12, 12 );

            Dark.CodeBackground  = FromHex( "#3B4252" );
            Dark.CodeBlockBorder = FromHex( "#4C566A" );
            Dark.QuoteBackground = FromHex( "#373E4C" );
            Dark.QuoteBorder     = FromHex( "#5E81AC" );

            Dark.Heading1     = FromHex( "#88C0D0" ); Dark.Heading1Size = 28;
            Dark.Heading2     = FromHex( "#88C0D0" ); Dark.Heading2Size = 22;
            Dark.Heading3     = FromHex( "#81A1C1" ); Dark.Heading3Size = 18;
            Dark.Heading4     = FromHex( "#81A1C1" ); Dark.Heading4Size = 16;
            Dark.Heading5     = FromHex( "#81A1C1" ); Dark.Heading5Size = 14;
            Dark.Heading6     = FromHex( "#81A1C1" ); Dark.Heading6Size = 13;

            Dark.TableHeaderBackground = FromHex( "#434C5E" );
            Dark.TableRowOddBackground  = FromHex( "#2E3440" );
            Dark.TableRowEvenBackground = FromHex( "#3B4252" );

            Dark.Keyword      = FromHex( "#81A1C1" );
            Dark.Type         = FromHex( "#8FBCBB" );
            Dark.String       = FromHex( "#A3BE8C" );
            Dark.Comment      = FromHex( "#616E88" );
            Dark.Number       = FromHex( "#B48EAD" );
            Dark.Method       = FromHex( "#88C0D0" );
            Dark.Preprocessor = FromHex( "#EBCB8B" );

            Dark.Highlight        = new Color( 0.533f, 0.753f, 0.816f, 0.20f );
            Dark.HighlightText    = FromHex( "#D8DEE9" );
            Dark.UnderlineColor   = FromHex( "#88C0D0" );
            Dark.SubscriptColor   = FromHex( "#8FBCBB" );
            Dark.SuperscriptColor = FromHex( "#8FBCBB" );

            Dark.AdmonitionNoteBorder       = FromHex( "#5E81AC" );
            Dark.AdmonitionNoteBackground   = FromHex( "#2E3440" );
            Dark.AdmonitionTipBorder        = FromHex( "#A3BE8C" );
            Dark.AdmonitionTipBackground    = FromHex( "#2E3440" );
            Dark.AdmonitionWarnBorder       = FromHex( "#EBCB8B" );
            Dark.AdmonitionWarnBackground   = FromHex( "#2E3440" );
            Dark.AdmonitionImportantBorder  = FromHex( "#B48EAD" );
            Dark.AdmonitionImportantBackground = FromHex( "#2E3440" );
            Dark.AdmonitionCautionBorder    = FromHex( "#BF616A" );
            Dark.AdmonitionCautionBackground = FromHex( "#2E3440" );
        }

        // ─────────────────────────────────────────────────────────────────────
        // One Dark — Atom editor default dark
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the One Dark theme preset to the dark skin.
        /// </summary>
        public void SetOneDarkDark()
        {
            Dark.PageBackground  = FromHex( "#282C34" );
            Dark.Text            = FromHex( "#ABB2BF" );
            Dark.Link            = FromHex( "#61AFEF" );
            Dark.Separator       = FromHex( "#3E4451" );
            Dark.Border          = FromHex( "#3E4451" );

            Dark.TextSize = 14;
            Dark.Margin   = new RectOffset( 16, 16, 12, 12 );

            Dark.CodeBackground  = FromHex( "#21252B" );
            Dark.CodeBlockBorder = FromHex( "#3E4451" );
            Dark.QuoteBackground = FromHex( "#2C313A" );
            Dark.QuoteBorder     = FromHex( "#4B5263" );

            Dark.Heading1     = FromHex( "#61AFEF" ); Dark.Heading1Size = 28;
            Dark.Heading2     = FromHex( "#61AFEF" ); Dark.Heading2Size = 22;
            Dark.Heading3     = FromHex( "#E5C07B" ); Dark.Heading3Size = 18;
            Dark.Heading4     = FromHex( "#E5C07B" ); Dark.Heading4Size = 16;
            Dark.Heading5     = FromHex( "#E5C07B" ); Dark.Heading5Size = 14;
            Dark.Heading6     = FromHex( "#E5C07B" ); Dark.Heading6Size = 13;

            Dark.TableHeaderBackground = FromHex( "#2C313A" );
            Dark.TableRowOddBackground  = FromHex( "#282C34" );
            Dark.TableRowEvenBackground = FromHex( "#2C313A" );

            Dark.Keyword      = FromHex( "#C678DD" );
            Dark.Type         = FromHex( "#E5C07B" );
            Dark.String       = FromHex( "#98C379" );
            Dark.Comment      = FromHex( "#5C6370" );
            Dark.Number       = FromHex( "#D19A66" );
            Dark.Method       = FromHex( "#61AFEF" );
            Dark.Preprocessor = FromHex( "#C678DD" );

            Dark.Highlight        = new Color( 0.380f, 0.690f, 0.941f, 0.20f );
            Dark.HighlightText    = FromHex( "#ABB2BF" );
            Dark.UnderlineColor   = FromHex( "#61AFEF" );
            Dark.SubscriptColor   = FromHex( "#56B6C2" );
            Dark.SuperscriptColor = FromHex( "#56B6C2" );

            Dark.AdmonitionNoteBorder       = FromHex( "#61AFEF" );
            Dark.AdmonitionNoteBackground   = FromHex( "#282C34" );
            Dark.AdmonitionTipBorder        = FromHex( "#98C379" );
            Dark.AdmonitionTipBackground    = FromHex( "#282C34" );
            Dark.AdmonitionWarnBorder       = FromHex( "#E5C07B" );
            Dark.AdmonitionWarnBackground   = FromHex( "#282C34" );
            Dark.AdmonitionImportantBorder  = FromHex( "#C678DD" );
            Dark.AdmonitionImportantBackground = FromHex( "#282C34" );
            Dark.AdmonitionCautionBorder    = FromHex( "#E06C75" );
            Dark.AdmonitionCautionBackground = FromHex( "#282C34" );
        }

        // ─────────────────────────────────────────────────────────────────────
        // Solarized — Ethan Schoonover's precision colours (dark variant)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the Solarized dark theme preset to the dark skin.
        /// </summary>
        public void SetSolarizedDark()
        {
            Dark.PageBackground  = FromHex( "#002B36" );
            Dark.Text            = FromHex( "#839496" );
            Dark.Link            = FromHex( "#268BD2" );
            Dark.Separator       = FromHex( "#073642" );
            Dark.Border          = FromHex( "#073642" );

            Dark.TextSize = 14;
            Dark.Margin   = new RectOffset( 16, 16, 12, 12 );

            Dark.CodeBackground  = FromHex( "#073642" );
            Dark.CodeBlockBorder = FromHex( "#0D3D4A" );
            Dark.QuoteBackground = FromHex( "#073642" );
            Dark.QuoteBorder     = FromHex( "#586E75" );

            Dark.Heading1     = FromHex( "#268BD2" ); Dark.Heading1Size = 28;
            Dark.Heading2     = FromHex( "#268BD2" ); Dark.Heading2Size = 22;
            Dark.Heading3     = FromHex( "#2AA198" ); Dark.Heading3Size = 18;
            Dark.Heading4     = FromHex( "#2AA198" ); Dark.Heading4Size = 16;
            Dark.Heading5     = FromHex( "#2AA198" ); Dark.Heading5Size = 14;
            Dark.Heading6     = FromHex( "#2AA198" ); Dark.Heading6Size = 13;

            Dark.TableHeaderBackground = FromHex( "#073642" );
            Dark.TableRowOddBackground  = FromHex( "#002B36" );
            Dark.TableRowEvenBackground = FromHex( "#073642" );

            Dark.Keyword      = FromHex( "#859900" );
            Dark.Type         = FromHex( "#268BD2" );
            Dark.String       = FromHex( "#2AA198" );
            Dark.Comment      = FromHex( "#586E75" );
            Dark.Number       = FromHex( "#D33682" );
            Dark.Method       = FromHex( "#B58900" );
            Dark.Preprocessor = FromHex( "#CB4B16" );

            Dark.Highlight        = new Color( 0.153f, 0.545f, 0.824f, 0.20f );
            Dark.HighlightText    = FromHex( "#FDF6E3" );
            Dark.UnderlineColor   = FromHex( "#268BD2" );
            Dark.SubscriptColor   = FromHex( "#2AA198" );
            Dark.SuperscriptColor = FromHex( "#2AA198" );

            Dark.AdmonitionNoteBorder       = FromHex( "#268BD2" );
            Dark.AdmonitionNoteBackground   = FromHex( "#002B36" );
            Dark.AdmonitionTipBorder        = FromHex( "#859900" );
            Dark.AdmonitionTipBackground    = FromHex( "#002B36" );
            Dark.AdmonitionWarnBorder       = FromHex( "#B58900" );
            Dark.AdmonitionWarnBackground   = FromHex( "#002B36" );
            Dark.AdmonitionImportantBorder  = FromHex( "#6C71C4" );
            Dark.AdmonitionImportantBackground = FromHex( "#002B36" );
            Dark.AdmonitionCautionBorder    = FromHex( "#DC322F" );
            Dark.AdmonitionCautionBackground = FromHex( "#002B36" );
        }

        // ─────────────────────────────────────────────────────────────────────
        // Solarized Light — warm parchment light variant
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the Solarized light theme preset to the light skin.
        /// </summary>
        public void SetSolarizedLight()
        {
            Light.PageBackground  = FromHex( "#FDF6E3" );
            Light.Text            = FromHex( "#657B83" );
            Light.Link            = FromHex( "#268BD2" );
            Light.Separator       = FromHex( "#EEE8D5" );
            Light.Border          = FromHex( "#EEE8D5" );

            Light.TextSize = 14;
            Light.Margin   = new RectOffset( 20, 20, 16, 16 );

            Light.CodeBackground  = FromHex( "#EEE8D5" );
            Light.CodeBlockBorder = FromHex( "#D9D0BC" );
            Light.QuoteBackground = FromHex( "#EEE8D5" );
            Light.QuoteBorder     = FromHex( "#93A1A1" );

            Light.Heading1     = FromHex( "#268BD2" ); Light.Heading1Size = 28;
            Light.Heading2     = FromHex( "#268BD2" ); Light.Heading2Size = 22;
            Light.Heading3     = FromHex( "#2AA198" ); Light.Heading3Size = 18;
            Light.Heading4     = FromHex( "#2AA198" ); Light.Heading4Size = 16;
            Light.Heading5     = FromHex( "#2AA198" ); Light.Heading5Size = 14;
            Light.Heading6     = FromHex( "#2AA198" ); Light.Heading6Size = 13;

            Light.TableHeaderBackground = FromHex( "#EEE8D5" );
            Light.TableRowOddBackground  = FromHex( "#FDF6E3" );
            Light.TableRowEvenBackground = FromHex( "#EEE8D5" );

            Light.Keyword      = FromHex( "#859900" );
            Light.Type         = FromHex( "#268BD2" );
            Light.String       = FromHex( "#2AA198" );
            Light.Comment      = FromHex( "#93A1A1" );
            Light.Number       = FromHex( "#D33682" );
            Light.Method       = FromHex( "#B58900" );
            Light.Preprocessor = FromHex( "#CB4B16" );

            Light.Highlight        = new Color( 0.153f, 0.545f, 0.824f, 0.18f );
            Light.HighlightText    = FromHex( "#002B36" );
            Light.UnderlineColor   = FromHex( "#268BD2" );
            Light.SubscriptColor   = FromHex( "#2AA198" );
            Light.SuperscriptColor = FromHex( "#2AA198" );

            Light.AdmonitionNoteBorder       = FromHex( "#268BD2" );
            Light.AdmonitionNoteBackground   = FromHex( "#DDEFFF" );
            Light.AdmonitionTipBorder        = FromHex( "#859900" );
            Light.AdmonitionTipBackground    = FromHex( "#E8F5D0" );
            Light.AdmonitionWarnBorder       = FromHex( "#B58900" );
            Light.AdmonitionWarnBackground   = FromHex( "#FFF8DC" );
            Light.AdmonitionImportantBorder  = FromHex( "#6C71C4" );
            Light.AdmonitionImportantBackground = FromHex( "#F0EFFF" );
            Light.AdmonitionCautionBorder    = FromHex( "#DC322F" );
            Light.AdmonitionCautionBackground = FromHex( "#FFECEC" );
        }

        // ─────────────────────────────────────────────────────────────────────
        // Catppuccin Mocha — soft dark with pastel accents
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the Catppuccin Mocha dark theme preset to the dark skin.
        /// </summary>
        public void SetCatppuccinMochaDark()
        {
            Dark.PageBackground  = FromHex( "#1E1E2E" );
            Dark.Text            = FromHex( "#CDD6F4" );
            Dark.Link            = FromHex( "#89B4FA" );
            Dark.Separator       = FromHex( "#313244" );
            Dark.Border          = FromHex( "#45475A" );

            Dark.TextSize = 14;
            Dark.Margin   = new RectOffset( 16, 16, 12, 12 );

            Dark.CodeBackground  = FromHex( "#181825" );
            Dark.CodeBlockBorder = FromHex( "#313244" );
            Dark.QuoteBackground = FromHex( "#24273A" );
            Dark.QuoteBorder     = FromHex( "#6C7086" );

            Dark.Heading1     = FromHex( "#CBA6F7" ); Dark.Heading1Size = 28;
            Dark.Heading2     = FromHex( "#CBA6F7" ); Dark.Heading2Size = 22;
            Dark.Heading3     = FromHex( "#89DCEB" ); Dark.Heading3Size = 18;
            Dark.Heading4     = FromHex( "#89DCEB" ); Dark.Heading4Size = 16;
            Dark.Heading5     = FromHex( "#89DCEB" ); Dark.Heading5Size = 14;
            Dark.Heading6     = FromHex( "#89DCEB" ); Dark.Heading6Size = 13;

            Dark.TableHeaderBackground = FromHex( "#313244" );
            Dark.TableRowOddBackground  = FromHex( "#1E1E2E" );
            Dark.TableRowEvenBackground = FromHex( "#24273A" );

            Dark.Keyword      = FromHex( "#CBA6F7" );
            Dark.Type         = FromHex( "#89DCEB" );
            Dark.String       = FromHex( "#A6E3A1" );
            Dark.Comment      = FromHex( "#6C7086" );
            Dark.Number       = FromHex( "#FAB387" );
            Dark.Method       = FromHex( "#89B4FA" );
            Dark.Preprocessor = FromHex( "#F38BA8" );

            Dark.Highlight        = new Color( 0.537f, 0.706f, 0.980f, 0.22f );
            Dark.HighlightText    = FromHex( "#CDD6F4" );
            Dark.UnderlineColor   = FromHex( "#89B4FA" );
            Dark.SubscriptColor   = FromHex( "#89DCEB" );
            Dark.SuperscriptColor = FromHex( "#89DCEB" );

            Dark.AdmonitionNoteBorder       = FromHex( "#89B4FA" );
            Dark.AdmonitionNoteBackground   = FromHex( "#1E1E2E" );
            Dark.AdmonitionTipBorder        = FromHex( "#A6E3A1" );
            Dark.AdmonitionTipBackground    = FromHex( "#1E1E2E" );
            Dark.AdmonitionWarnBorder       = FromHex( "#F9E2AF" );
            Dark.AdmonitionWarnBackground   = FromHex( "#1E1E2E" );
            Dark.AdmonitionImportantBorder  = FromHex( "#CBA6F7" );
            Dark.AdmonitionImportantBackground = FromHex( "#1E1E2E" );
            Dark.AdmonitionCautionBorder    = FromHex( "#F38BA8" );
            Dark.AdmonitionCautionBackground = FromHex( "#1E1E2E" );
        }

        // ─────────────────────────────────────────────────────────────────────

        internal static Color FromHex( string hex )
        {
            if( ColorUtility.TryParseHtmlString( hex, out Color color ) )
            {
                return color;
            }
            return Color.white;
        }
    }
}
