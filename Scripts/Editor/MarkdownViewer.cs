// ============================================================
// File:    MarkdownViewer.cs
// Purpose: Main class for rendering Markdown content in the Unity Editor.
// Author:  Ahmad Albahar
// ============================================================

using AB.MDV.Layout;
using AB.MDV.Renderer;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.JiraLinks;
using Markdig.Extensions.Tables;
using System;
using UnityEditor;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Core class for parsing and rendering Markdown within the Unity Editor.
    /// Handles image loading, navigation, and layouting.
    /// Manages the overall rendering pipeline and state.
    /// </summary>
    public class MarkdownViewer
    {
        /// <summary>
        /// Default margin for the markdown content.
        /// </summary>
        public static readonly Vector2 Margin = new Vector2(6.0f, 4.0f);

        private GUISkin mSkin = null;
        private string mText = string.Empty;
        private string mCurrentPath = string.Empty;
        private MarkdownHandleImages mHandlerImages = new MarkdownHandleImages();
        private MarkdownHandleNavigate mHandlerNavigate = new MarkdownHandleNavigate();

        private Func<float> mViewWidthProvider = () => EditorGUIUtility.currentViewWidth;

        private MarkdownLayout mLayout = null;
        private bool mRaw = false;
        private float? mPendingScrollTarget = null;

        private static MarkdownHistory mHistory = new MarkdownHistory();

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownViewer"/> class.
        /// </summary>
        /// <param name="skin">The GUISkin to use for rendering.</param>
        /// <param name="path">The path of the markdown file.</param>
        /// <param name="content">The raw text content of the markdown file.</param>
        public MarkdownViewer(GUISkin skin, string path, string content)
        {
            mSkin = skin;
            mCurrentPath = path;
            mText = content;

            mHistory.OnOpen(mCurrentPath);
            mLayout = ParseDocument();

            mHandlerImages.CurrentPath = mCurrentPath;

            mHandlerNavigate.CurrentPath = mCurrentPath;
            mHandlerNavigate.History = mHistory;
            mHandlerNavigate.FindBlock = (id) => mLayout.Find(id);
            mHandlerNavigate.ScrollTo = (pos) => { mPendingScrollTarget = pos; };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownViewer"/> class with a custom view width provider.
        /// </summary>
        /// <param name="skin">The GUISkin to use for rendering.</param>
        /// <param name="path">The path of the markdown file.</param>
        /// <param name="content">The raw text content of the markdown file.</param>
        /// <param name="viewWidthProvider">Function that returns the current available view width.</param>
        public MarkdownViewer(GUISkin skin, string path, string content, Func<float> viewWidthProvider)
            : this(skin, path, content)
        {
            if (viewWidthProvider == null)
            {
                throw new ArgumentNullException(nameof(viewWidthProvider));
            }

            mViewWidthProvider = viewWidthProvider;
        }

        /// <summary>
        /// Updates the viewer state, primarily used for image loading and animations.
        /// </summary>
        /// <returns>True if any changes occurred that require a repaint.</returns>
        public bool Update()
        {
            return mHandlerImages.Update();
        }

        /// <summary>
        /// Returns and clears the pending anchor-scroll target (if any).
        /// </summary>
        /// <returns>The vertical scroll position to navigate to, or null if no scroll is pending.</returns>
        public float? ConsumePendingScroll()
        {
            if (!mPendingScrollTarget.HasValue)
            {
                return null;
            }

            var value = mPendingScrollTarget.Value;
            mPendingScrollTarget = null;
            return value;
        }

        /// <summary>
        /// Parses the raw markdown text into a layout tree using the Markdig pipeline.
        /// </summary>
        /// <returns>The generated Layout.</returns>
        private MarkdownLayout ParseDocument()
        {
            var context = new Context(mSkin, mHandlerImages, mHandlerNavigate);
            var builder = new LayoutBuilder(context);
            var renderer = new RendererMarkdown(builder);

            var pipelineBuilder = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .UseEmphasisExtras()
                .UseDefinitionLists()
                .UseTaskLists()
                .UseGenericAttributes()
                .UseAutoIdentifiers(AutoIdentifierOptions.GitHub);

            if (!string.IsNullOrEmpty(MarkdownPreferences.JIRA))
            {
                pipelineBuilder.UseJiraLinks(new JiraLinkOptions(MarkdownPreferences.JIRA));
            }

            if (MarkdownPreferences.PipedTables)
            {
                pipelineBuilder.UsePipeTables(new PipeTableOptions
                {
                    RequireHeaderSeparator = MarkdownPreferences.PipedTablesRequireHeaderSeparator
                });
            }

            var pipeline = pipelineBuilder.Build();
            pipeline.Setup(renderer);

            var doc = Markdown.Parse(mText, pipeline);
            renderer.Render(doc);

            return builder.GetLayout();
        }

        /// <summary>
        /// Clears the background with the theme's page background color.
        /// </summary>
        /// <param name="height">The height of the content.</param>
        /// <param name="margin">The content margin.</param>
        private void ClearBackground(float height, RectOffset margin)
        {
            var rectFullScreen = new Rect(0.0f, 0.0f, Screen.width, Mathf.Max(height + margin.top + margin.bottom, Screen.height));
            EditorGUI.DrawRect(rectFullScreen, MarkdownTheme.Instance.Active.PageBackground);
        }

        /// <summary>
        /// Renders the markdown content using IMGUI.
        /// </summary>
        public void Draw()
        {
            GUI.skin = mSkin;
            GUI.enabled = true;

            var theme = MarkdownTheme.Instance.Active;
            var margin = theme.Margin;

            var totalWidth = mViewWidthProvider();
            var contentWidth = totalWidth - margin.left - margin.right;

            DrawToolbar(contentWidth, margin);

            if (mRaw)
            {
                var style = mSkin.GetStyle("raw");
                var width = contentWidth - mSkin.button.fixedHeight;
                var height = style.CalcHeight(new GUIContent(mText), width);

                if (Event.current.type == EventType.Layout)
                {
                    GUILayout.Space(height + margin.top + margin.bottom);
                }
                else
                {
                    ClearBackground(height, margin);
                    var rect = new Rect(margin.left, margin.top, width, height);
                    EditorGUI.SelectableLabel(rect, mText, style);
                }
            }
            else
            {
                ClearBackground(mLayout.Height, margin);
                DrawMarkdown(contentWidth, margin);
            }
        }

        /// <summary>
        /// Renders the viewer toolbar for switching between rendered and raw view.
        /// </summary>
        /// <param name="contentWidth">The width of the content area.</param>
        /// <param name="margin">The content margin.</param>
        private void DrawToolbar(float contentWidth, RectOffset margin)
        {
            var style = GUI.skin.button;
            var size = style.fixedHeight;
            var btn = new Rect(margin.left + contentWidth - size, margin.top, size, size);

            if (GUI.Button(btn, string.Empty, GUI.skin.GetStyle(mRaw ? "btnRaw" : "btnFile")))
            {
                mRaw = !mRaw;
            }

            if (!mRaw)
            {
                if (mHistory.CanForward)
                {
                    btn.x -= size;

                    if (GUI.Button(btn, string.Empty, GUI.skin.GetStyle("btnForward")))
                    {
                        var path = mHistory.Forward();
                        if (!string.IsNullOrEmpty(path))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                        }
                    }
                }

                if (mHistory.CanBack)
                {
                    btn.x -= size;

                    if (GUI.Button(btn, string.Empty, GUI.skin.GetStyle("btnBack")))
                    {
                        var path = mHistory.Back();
                        if (!string.IsNullOrEmpty(path))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders the parsed markdown layout.
        /// </summary>
        /// <param name="width">The width of the content area.</param>
        /// <param name="margin">The content margin.</param>
        private void DrawMarkdown(float width, RectOffset margin)
        {
            var pos = new Vector2(margin.left, margin.top);

            switch (Event.current.type)
            {
                case EventType.Ignore:
                    break;

                case EventType.ContextClick:
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("View Source"), false, () => mRaw = !mRaw);
                    menu.ShowAsContext();
                    break;

                case EventType.Layout:
                    GUILayout.Space(mLayout.Height + margin.top + margin.bottom);
                    mLayout.Arrange(pos, width);
                    break;

                default:
                    mLayout.Draw(pos);
                    break;
            }
        }
    }
}
