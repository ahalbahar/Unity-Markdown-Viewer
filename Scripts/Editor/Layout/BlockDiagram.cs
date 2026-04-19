// ============================================================
// File:    BlockDiagram.cs
// Purpose: Renderable block for Mermaid diagrams with scroll and popout support.
// Author:  Ahmad Albahar
// Created: 2026-04-19
// Notes:   Provides horizontal scrolling for wide diagrams inside the Inspector.
// ============================================================

using UnityEditor;
using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// A dedicated block for rendered Mermaid diagrams.
    /// </summary>
    public class BlockDiagram : Block
    {
        private const float OuterPadding = 8.0f;
        private const float ToolbarHeight = 20.0f;
        private const float ToolbarSpacing = 4.0f;
        private const float HorizontalScrollbarHeight = 16.0f;
        private const float ButtonWidth = 68.0f;

        private readonly MarkdownImageRequest mImageRequest;
        private readonly string mSource;
        private readonly string mTitle;

        private Texture mTexture;
        private Vector2 mScroll;
        private Rect mToolbarRect;
        private Rect mViewportRect;
        private Rect mButtonRect;
        private bool mNeedsHorizontalScroll;
        private float mImageWidth;
        private float mImageHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockDiagram"/> class.
        /// </summary>
        /// <param name="indent">The indentation level.</param>
        /// <param name="imageRequest">The themed image request.</param>
        /// <param name="source">The raw Mermaid source.</param>
        /// <param name="title">The display title.</param>
        public BlockDiagram(float indent, MarkdownImageRequest imageRequest, string source, string title)
            : base(indent)
        {
            mImageRequest = imageRequest;
            mSource = source ?? string.Empty;
            mTitle = string.IsNullOrWhiteSpace(title) ? "Mermaid diagram" : title;
        }

        /// <summary>
        /// Arranges the diagram viewport and optional scroll region.
        /// </summary>
        public override void Arrange(Context context, Vector2 anchor, float maxWidth)
        {
            Rect.position = new Vector2(anchor.x + Indent, anchor.y);
            Rect.width = Mathf.Max(maxWidth - Indent, context.MinWidth);

            mTexture = context.FetchImage(mImageRequest);

            float innerWidth = Mathf.Max(Rect.width - (OuterPadding * 2.0f), context.MinWidth);
            float textHeight = context.CalcSize(new GUIContent($"[{mTitle}]")).y;

            mImageWidth = mTexture != null ? mTexture.width : innerWidth;
            mImageHeight = mTexture != null ? mTexture.height : textHeight + 6.0f;
            mNeedsHorizontalScroll = mTexture != null && mImageWidth > innerWidth;

            float viewportHeight = mImageHeight;
            if (mNeedsHorizontalScroll)
            {
                viewportHeight += HorizontalScrollbarHeight;
            }

            Rect.height = OuterPadding + ToolbarHeight + ToolbarSpacing + viewportHeight + OuterPadding;

            mToolbarRect = new Rect(
                Rect.x + OuterPadding,
                Rect.y + OuterPadding,
                innerWidth,
                ToolbarHeight);

            mButtonRect = new Rect(
                mToolbarRect.xMax - ButtonWidth,
                mToolbarRect.y + 1.0f,
                ButtonWidth,
                ToolbarHeight - 2.0f);

            mViewportRect = new Rect(
                Rect.x + OuterPadding,
                mToolbarRect.yMax + ToolbarSpacing,
                innerWidth,
                viewportHeight);
        }

        /// <summary>
        /// Draws the diagram block, its scrollable viewport, and the popout button.
        /// </summary>
        public override void Draw(Context context)
        {
            var colors = MarkdownTheme.Instance.Active;

            GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("blockcode"));
            EditorGUI.DrawRect(new Rect(Rect.x, Rect.y, Rect.width, 1.0f), colors.CodeBlockBorder);
            EditorGUI.DrawRect(new Rect(Rect.x, Rect.yMax - 1.0f, Rect.width, 1.0f), colors.CodeBlockBorder);
            EditorGUI.DrawRect(new Rect(Rect.x, Rect.y, 1.0f, Rect.height), colors.CodeBlockBorder);
            EditorGUI.DrawRect(new Rect(Rect.xMax - 1.0f, Rect.y, 1.0f, Rect.height), colors.CodeBlockBorder);

            var titleStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            titleStyle.normal.textColor = colors.Text;
            GUI.Label(new Rect(mToolbarRect.x, mToolbarRect.y + 2.0f, mToolbarRect.width - ButtonWidth - 6.0f, mToolbarRect.height), mTitle, titleStyle);

            if (GUI.Button(mButtonRect, "Expand", EditorStyles.miniButton))
            {
                MarkdownDiagramPreviewWindow.ShowWindow(mTitle, mSource, mImageRequest);
            }

            if (mTexture == null)
            {
                var fallbackStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                fallbackStyle.normal.textColor = colors.Text;
                GUI.Label(mViewportRect, $"[{mTitle}]", fallbackStyle);
                return;
            }

            if (mNeedsHorizontalScroll)
            {
                var viewRect = new Rect(0.0f, 0.0f, mImageWidth, mImageHeight);
                mScroll = GUI.BeginScrollView(mViewportRect, mScroll, viewRect, true, false);
                GUI.DrawTexture(viewRect, mTexture, ScaleMode.StretchToFill);
                GUI.EndScrollView();
            }
            else
            {
                var imageRect = new Rect(mViewportRect.x, mViewportRect.y, mImageWidth, mImageHeight);
                GUI.DrawTexture(imageRect, mTexture, ScaleMode.StretchToFill);
            }
        }
    }
}
