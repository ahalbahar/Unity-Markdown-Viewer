// ============================================================
// File:    MarkdownHandleImages.cs
// Purpose: Handles loading, caching, and animating images in Markdown.
// Author:  Ahmad Albahar
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using AB.MDV.GIF;

namespace AB.MDV
{
    /// <summary>
    /// Manages image requests, caching, and animations for the Markdown viewer.
    /// Supports local and remote images, including animated GIFs.
    /// Renamed from HandlerImages to match filename.
    /// </summary>
    public class MarkdownHandleImages
    {
        /// <summary>
        /// The base path used for remapping relative image URLs.
        /// </summary>
        public string CurrentPath;

        private Texture mPlaceholder = null;
        private List<ImageRequest> mActiveRequests = new List<ImageRequest>();
        private Dictionary<string, Texture> mTextureCache = new Dictionary<string, Texture>();
        private List<AnimatedTexture> mAnimatedTextures = new List<AnimatedTexture>();
        private HashSet<string> mFailedImageUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> mWarnedImageUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private class AnimatedTexture
        {
            private string URL = string.Empty;
            private int CurrentFrame = 0;
            private double FrameTime = 0.0f;
            private List<Texture2D> Textures = new List<Texture2D>();
            private List<float> Times = new List<float>();

            internal string CurrentUrl => URL;
            internal int FrameIndex => CurrentFrame;
            internal int TextureCount => Textures.Count;
            internal Texture2D CurrentTexture => Textures[CurrentFrame];
            internal Texture2D FirstTexture => Textures[0];

            private AnimatedTexture(string url)
            {
                URL = url;
                FrameTime = EditorApplication.timeSinceStartup;
            }

            internal static AnimatedTexture Create(string url)
            {
                return new AnimatedTexture(url);
            }

            private void Add(Texture2D tex, float delay)
            {
                Textures.Add(tex);
                Times.Add(delay);
            }

            internal void AddFrame(Texture2D tex, float delay)
            {
                Add(tex, delay);
            }

            internal bool Update()
            {
                var span = EditorApplication.timeSinceStartup - FrameTime;

                if (span < Times[CurrentFrame])
                {
                    return false;
                }

                FrameTime = EditorApplication.timeSinceStartup;
                CurrentFrame = (CurrentFrame + 1) % Textures.Count;

                return true;
            }
        }

        private class ImageRequest
        {
            private string URL;
            private UnityWebRequest Request;
            private bool IsGif;
            private int AttemptIndex;
            private List<string> CandidateUrls;

            internal string CurrentUrl => URL;
            internal UnityWebRequest CurrentRequest => Request;
            internal bool IsAnimatedGif => IsGif;
            internal bool CanRetryFallback => CandidateUrls != null && AttemptIndex + 1 < CandidateUrls.Count;

            internal ImageRequest(string url)
                : this(new List<string> { url }, 0)
            {
            }

            private ImageRequest(List<string> candidateUrls, int attemptIndex)
            {
                CandidateUrls = candidateUrls;
                AttemptIndex = attemptIndex;
                URL = candidateUrls[attemptIndex];

                if (URL.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    IsGif = true;
                    Request = UnityWebRequest.Get(URL);
                }
                else
                {
                    IsGif = false;
                    Request = new UnityWebRequest(URL, "GET", new DownloadHandlerBuffer(), null);
                }

                Request.SendWebRequest();
            }

            internal ImageRequest CreateFallback()
            {
                return new ImageRequest(CandidateUrls, AttemptIndex + 1);
            }

            internal AnimatedTexture GetAnimatedTexture()
            {
                var decoder = new Decoder(Request.downloadHandler.data);
                var img = decoder.NextImage();
                var anim = AnimatedTexture.Create(URL);

                while (img != null)
                {
                    anim.AddFrame(img.CreateTexture(), img.Delay / 1000.0f);
                    img = decoder.NextImage();
                }

                return anim;
            }

            internal Texture GetTexture()
            {
                var downloadHandler = Request.downloadHandler as DownloadHandlerBuffer;

                if (downloadHandler == null)
                {
                    return null;
                }

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(downloadHandler.data, true);
                return texture;
            }
        }

        private string RemapURL(string url)
        {
            if (Regex.IsMatch(url, @"^\w+:", RegexOptions.Singleline))
            {
                return url;
            }

            var projectDir = Path.GetDirectoryName(Application.dataPath);

            if (url.StartsWith("/"))
            {
                return string.Format("file:///{0}{1}", projectDir, url);
            }

            var assetDir = Path.GetDirectoryName(Path.GetFullPath(CurrentPath));
            return "file:///" + MarkdownUtils.PathNormalise(string.Format("{0}/{1}", assetDir, url));
        }

        /// <summary>
        /// Fetches an image from the specified URL, returning a placeholder if not cached.
        /// </summary>
        /// <param name="url">The image URL or path.</param>
        /// <returns>The cached texture, or a placeholder if loading.</returns>
        public Texture FetchImage(string url)
        {
            url = RemapURL(url);

            if (mTextureCache.TryGetValue(url, out var tex))
            {
                return tex;
            }

            if (mFailedImageUrls.Contains(url))
            {
                return null;
            }

            if (mPlaceholder == null)
            {
                var style = GUI.skin.GetStyle("btnPlaceholder");
                mPlaceholder = style != null ? style.normal.background : null;
            }

            mActiveRequests.Add(new ImageRequest(url));
            mTextureCache[url] = mPlaceholder;

            return mPlaceholder;
        }

        /// <summary>
        /// Checks for completed image requests and updates the texture cache.
        /// </summary>
        /// <returns>True if any image was updated.</returns>
        public bool UpdateRequests()
        {
            var req = mActiveRequests.Find(r => r.CurrentRequest.isDone);

            if (req == null)
            {
                return false;
            }

#if UNITY_2020_2_OR_NEWER
            if (req.CurrentRequest.result == UnityWebRequest.Result.ProtocolError)
#else
            if (req.CurrentRequest.isHttpError)
#endif
            {
                if (req.CurrentRequest.responseCode == 404
                    && req.CanRetryFallback
                    && AB.MDV.Layout.EmojiImageSupport.IsConfiguredEmojiSourceUrl(req.CurrentUrl))
                {
                    ImageRequest fallbackRequest = req.CreateFallback();
                    mActiveRequests.Add(fallbackRequest);
                    mTextureCache[fallbackRequest.CurrentUrl] = mPlaceholder;
                    mTextureCache[req.CurrentUrl] = mPlaceholder;
                    mActiveRequests.Remove(req);
                    return true;
                }

                bool isEmojiCdnAsset = AB.MDV.Layout.EmojiImageSupport.IsConfiguredEmojiSourceUrl(req.CurrentUrl);
                if (!isEmojiCdnAsset)
                {
                    Debug.LogError(string.Format("HTTP Error: {0} - {1} {2}", req.CurrentUrl, req.CurrentRequest.responseCode, req.CurrentRequest.error));
                }
                else
                {
                    WarnOnce(req.CurrentUrl, string.Format(
                        "Emoji fallback image was not found for '{0}'. The viewer will fall back to text rendering for this emoji.",
                        req.CurrentUrl));
                }

                mTextureCache[req.CurrentUrl] = null;
                mFailedImageUrls.Add(req.CurrentUrl);
            }
#if UNITY_2020_2_OR_NEWER
            else if (req.CurrentRequest.result == UnityWebRequest.Result.ConnectionError)
#else
            else if (req.CurrentRequest.isNetworkError)
#endif
            {
                if (AB.MDV.Layout.EmojiImageSupport.IsConfiguredEmojiSourceUrl(req.CurrentUrl))
                {
                    WarnOnce(req.CurrentUrl, string.Format(
                        "Emoji fallback image could not be downloaded for '{0}'. Check your internet connection if you want color emoji rendering in older Unity versions.",
                        req.CurrentUrl));
                }
                else
                {
                    Debug.LogError(string.Format("Network Error: {0} - {1}", req.CurrentUrl, req.CurrentRequest.error));
                }

                mTextureCache[req.CurrentUrl] = null;
                mFailedImageUrls.Add(req.CurrentUrl);
            }
            else if (req.IsAnimatedGif)
            {
                var anim = req.GetAnimatedTexture();

                if (anim != null && anim.TextureCount > 0)
                {
                    mTextureCache[req.CurrentUrl] = anim.FirstTexture;

                    if (anim.TextureCount > 1)
                    {
                        mAnimatedTextures.Add(anim);
                    }
                }
            }
            else
            {
                mTextureCache[req.CurrentUrl] = req.GetTexture();
            }

            mActiveRequests.Remove(req);
            return true;
        }

        /// <summary>
        /// Updates any active animated textures.
        /// </summary>
        /// <returns>True if any animation was updated.</returns>
        public bool UpdateAnimations()
        {
            var update = false;

            foreach (var anim in mAnimatedTextures)
            {
                if (anim.Update())
                {
                    mTextureCache[anim.CurrentUrl] = anim.CurrentTexture;
                    update = true;
                }
            }

            return update;
        }

        /// <summary>
        /// Main update call for handling image requests and animations.
        /// </summary>
        /// <returns>True if any changes occurred that require a repaint.</returns>
        public bool Update()
        {
            return UpdateRequests() || UpdateAnimations();
        }

        private void WarnOnce(string url, string message)
        {
            if (!mWarnedImageUrls.Add(url))
            {
                return;
            }

            Debug.LogWarning(message);
        }
    }
}
