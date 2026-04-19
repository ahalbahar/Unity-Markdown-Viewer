// ============================================================
// File:    MarkdownDiagramPreviewWindow.cs
// Purpose: Preview window for Mermaid diagrams rendered by the markdown viewer.
// Author:  Ahmad Albahar
// Created: 2026-04-19
// Notes:   Supports rendered preview and raw Mermaid source tabs.
// ============================================================

using UnityEditor;
using UnityEngine;

namespace AB.MDV
{
    /// <summary>
    /// Standalone preview window for Mermaid diagrams.
    /// </summary>
    public class MarkdownDiagramPreviewWindow : EditorWindow
    {
        private const float MinZoom = 0.25f;
        private const float MaxZoom = 4.0f;
        private const float ZoomStep = 0.1f;

        private MarkdownHandleImages mImages = new MarkdownHandleImages();
        private MarkdownImageRequest mImageRequest;
        private string mSource = string.Empty;
        private string mTitle = "Mermaid Diagram";
        private Vector2 mPreviewScroll;
        private Vector2 mSourceScroll;
        private bool mShowSource;
        private float mPreviewZoom = 1.0f;
        private bool mIsPanning;
        private Vector2 mPanStartMouse;
        private Vector2 mPanStartScroll;

        /// <summary>
        /// Opens a preview window for the specified Mermaid diagram.
        /// </summary>
        /// <param name="title">The display title.</param>
        /// <param name="source">The raw Mermaid source.</param>
        /// <param name="imageRequest">The diagram render request.</param>
        public static void ShowWindow(string title, string source, MarkdownImageRequest imageRequest)
        {
            var window = GetWindow<MarkdownDiagramPreviewWindow>(false, string.IsNullOrWhiteSpace(title) ? "Mermaid Diagram" : title, true);
            window.minSize = new Vector2(480.0f, 320.0f);
            window.mTitle = string.IsNullOrWhiteSpace(title) ? "Mermaid Diagram" : title;
            window.mSource = source ?? string.Empty;
            window.mImageRequest = imageRequest;
            window.mPreviewScroll = Vector2.zero;
            window.mSourceScroll = Vector2.zero;
            window.mPreviewZoom = 1.0f;
            window.mIsPanning = false;
            window.Show();
            window.Repaint();
        }

        private void OnEnable()
        {
            EditorApplication.update += HandleUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= HandleUpdate;
        }

        private void HandleUpdate()
        {
            if (mImages.Update())
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            var colors = MarkdownTheme.Instance.Active;
            EditorGUI.DrawRect(new Rect(0.0f, 0.0f, position.width, position.height), colors.PageBackground);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Toggle(!mShowSource, "Preview", EditorStyles.toolbarButton))
                {
                    mShowSource = false;
                }

                if (GUILayout.Toggle(mShowSource, "Source", EditorStyles.toolbarButton))
                {
                    mShowSource = true;
                }

                using (new EditorGUI.DisabledScope(mShowSource))
                {
                    if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
                    {
                        SetZoom(mPreviewZoom - ZoomStep, position.size * 0.5f);
                    }

                    GUILayout.Label($"{Mathf.RoundToInt(mPreviewZoom * 100.0f)}%", EditorStyles.miniLabel, GUILayout.Width(44.0f));

                    if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
                    {
                        SetZoom(mPreviewZoom + ZoomStep, position.size * 0.5f);
                    }

                    if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(44.0f)))
                    {
                        // CHANGED: Preview reset restores a predictable full-size view after zooming or panning.
                        mPreviewZoom = 1.0f;
                        mPreviewScroll = Vector2.zero;
                        Repaint();
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label(mTitle, EditorStyles.miniLabel);
            }

            if (mShowSource)
            {
                DrawSource(colors);
            }
            else
            {
                DrawPreview(colors);
            }
        }

        private void DrawPreview(MarkdownTheme.ThemeColors colors)
        {
            if (mImageRequest == null)
            {
                EditorGUILayout.HelpBox("No Mermaid diagram request is available.", MessageType.Warning);
                return;
            }

            Texture texture = mImages.FetchImage(mImageRequest);
            if (texture == null)
            {
                EditorGUILayout.HelpBox("The diagram is not available yet. If all render endpoints fail, the viewer falls back to text.", MessageType.Info);
                return;
            }

            Rect viewport = GUILayoutUtility.GetRect(0.0f, 100000.0f, 0.0f, 100000.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            var viewRect = new Rect(0.0f, 0.0f, texture.width * mPreviewZoom, texture.height * mPreviewZoom);
            HandlePreviewInput(viewport, texture, viewRect.size);
            mPreviewScroll = ClampPreviewScroll(mPreviewScroll, viewport.size, viewRect.size);
            mPreviewScroll = GUI.BeginScrollView(viewport, mPreviewScroll, viewRect, true, true);
            GUI.DrawTexture(viewRect, texture, ScaleMode.StretchToFill);
            GUI.EndScrollView();
        }

        private void DrawSource(MarkdownTheme.ThemeColors colors)
        {
            var textStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = false,
            };

            textStyle.normal.textColor = colors.Text;
            textStyle.normal.background = Texture2D.whiteTexture;

            Rect rect = GUILayoutUtility.GetRect(0.0f, 100000.0f, 0.0f, 100000.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, colors.CodeBackground);

            Rect paddedRect = new Rect(rect.x + 8.0f, rect.y + 8.0f, rect.width - 16.0f, rect.height - 16.0f);
            Rect sourceRect = new Rect(0.0f, 0.0f, Mathf.Max(paddedRect.width, 200.0f), textStyle.CalcHeight(new GUIContent(mSource), Mathf.Max(paddedRect.width, 200.0f)));
            mSourceScroll = GUI.BeginScrollView(paddedRect, mSourceScroll, sourceRect, true, true);
            EditorGUI.SelectableLabel(sourceRect, mSource, textStyle);
            GUI.EndScrollView();
        }

        private void HandlePreviewInput(Rect viewport, Texture texture, Vector2 zoomedSize)
        {
            Event currentEvent = Event.current;
            if (currentEvent == null)
            {
                return;
            }

            bool isMouseInsideViewport = viewport.Contains(currentEvent.mousePosition);
            if (isMouseInsideViewport)
            {
                EditorGUIUtility.AddCursorRect(viewport, mIsPanning ? MouseCursor.Pan : MouseCursor.Zoom);
            }

            switch (currentEvent.type)
            {
                case EventType.ScrollWheel:
                    if (!isMouseInsideViewport)
                    {
                        return;
                    }

                    float zoomDelta = -Mathf.Sign(currentEvent.delta.y) * ZoomStep;
                    SetZoom(mPreviewZoom + zoomDelta, currentEvent.mousePosition, viewport, texture);
                    currentEvent.Use();
                    break;

                case EventType.MouseDown:
                    if (!isMouseInsideViewport)
                    {
                        return;
                    }

                    if (currentEvent.button == 2 || (currentEvent.button == 0 && currentEvent.alt))
                    {
                        mIsPanning = true;
                        mPanStartMouse = currentEvent.mousePosition;
                        mPanStartScroll = mPreviewScroll;
                        currentEvent.Use();
                    }

                    break;

                case EventType.MouseDrag:
                    if (!mIsPanning)
                    {
                        return;
                    }

                    Vector2 dragDelta = currentEvent.mousePosition - mPanStartMouse;
                    mPreviewScroll = ClampPreviewScroll(mPanStartScroll - dragDelta, viewport.size, zoomedSize);
                    Repaint();
                    currentEvent.Use();
                    break;

                case EventType.MouseUp:
                case EventType.Ignore:
                    if (!mIsPanning)
                    {
                        return;
                    }

                    mIsPanning = false;
                    currentEvent.Use();
                    break;
            }
        }

        private void SetZoom(float requestedZoom, Vector2 pivotInWindow)
        {
            SetZoom(requestedZoom, pivotInWindow, new Rect(0.0f, 0.0f, position.width, position.height), null);
        }

        private void SetZoom(float requestedZoom, Vector2 pivotInWindow, Rect viewport, Texture texture)
        {
            float clampedZoom = Mathf.Clamp(requestedZoom, MinZoom, MaxZoom);
            if (Mathf.Approximately(clampedZoom, mPreviewZoom))
            {
                return;
            }

            Vector2 localPivot = pivotInWindow - viewport.position;
            Vector2 imageSpacePivot = localPivot + mPreviewScroll;
            float scaleFactor = clampedZoom / mPreviewZoom;
            mPreviewZoom = clampedZoom;
            mPreviewScroll = (imageSpacePivot * scaleFactor) - localPivot;

            if (texture != null)
            {
                Vector2 zoomedSize = new Vector2(texture.width * mPreviewZoom, texture.height * mPreviewZoom);
                mPreviewScroll = ClampPreviewScroll(mPreviewScroll, viewport.size, zoomedSize);
            }

            Repaint();
        }

        private static Vector2 ClampPreviewScroll(Vector2 scroll, Vector2 viewportSize, Vector2 contentSize)
        {
            float maxScrollX = Mathf.Max(0.0f, contentSize.x - viewportSize.x);
            float maxScrollY = Mathf.Max(0.0f, contentSize.y - viewportSize.y);
            return new Vector2(
                Mathf.Clamp(scroll.x, 0.0f, maxScrollX),
                Mathf.Clamp(scroll.y, 0.0f, maxScrollY));
        }
    }
}
