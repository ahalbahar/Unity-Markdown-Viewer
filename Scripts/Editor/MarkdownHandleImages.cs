// ============================================================
// File:    MarkdownHandleImages.cs
// Purpose: Handles loading, caching, and animating images in Markdown.
// Author:  Ahmad Albahar
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
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
        private const double MermaidRetryDelaySeconds = 0.75d;

        /// <summary>
        /// The base path used for remapping relative image URLs.
        /// </summary>
        public string CurrentPath;

        private Texture mPlaceholder = null;
        private List<ImageRequest> mActiveRequests = new List<ImageRequest>();
        private Dictionary<string, Texture> mTextureCache = new Dictionary<string, Texture>();
        private List<AnimatedTexture> mAnimatedTextures = new List<AnimatedTexture>();
        private HashSet<string> mFailedImageKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> mWarnedImageKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private class AnimatedTexture
        {
            private string mCacheKey = string.Empty;
            private int CurrentFrame = 0;
            private double FrameTime = 0.0f;
            private List<Texture2D> Textures = new List<Texture2D>();
            private List<float> Times = new List<float>();

            internal string CacheKey => mCacheKey;
            internal int FrameIndex => CurrentFrame;
            internal int TextureCount => Textures.Count;
            internal Texture2D CurrentTexture => Textures[CurrentFrame];
            internal Texture2D FirstTexture => Textures[0];

            private AnimatedTexture(string cacheKey)
            {
                mCacheKey = cacheKey;
                FrameTime = EditorApplication.timeSinceStartup;
            }

            internal static AnimatedTexture Create(string cacheKey)
            {
                return new AnimatedTexture(cacheKey);
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
            private readonly MarkdownImageRequest mLogicalRequest;
            private readonly int mCandidateIndex;
            private readonly double mStartAfterTime;
            private readonly MarkdownImageCandidate mCandidate;
            private UnityWebRequest Request;
            private readonly bool mIsGif;

            internal string CacheKey => mLogicalRequest.CacheKey;
            internal string CurrentUrl => mCandidate.Url;
            internal UnityWebRequest CurrentRequest => Request;
            internal bool IsAnimatedGif => mIsGif;
            internal bool CanAdvance => mCandidateIndex + 1 < mLogicalRequest.Candidates.Count;
            internal bool IsStarted => Request != null;
            internal MarkdownImageRequestKind Kind => mLogicalRequest.Kind;
            internal string DisplayName => mLogicalRequest.DisplayName;
            internal bool EnableDiskCache => mLogicalRequest.EnableDiskCache;
            internal int CandidateCount => mLogicalRequest.Candidates.Count;
            internal MarkdownImageRequest LogicalRequest => mLogicalRequest;

            internal ImageRequest(MarkdownImageRequest logicalRequest)
                : this(logicalRequest, 0, 0.0d)
            {
            }

            private ImageRequest(MarkdownImageRequest logicalRequest, int candidateIndex, double delaySeconds)
            {
                if (logicalRequest == null)
                {
                    throw new ArgumentNullException(nameof(logicalRequest));
                }

                mLogicalRequest = logicalRequest;
                mCandidateIndex = candidateIndex;
                mCandidate = logicalRequest.Candidates[candidateIndex];
                mStartAfterTime = EditorApplication.timeSinceStartup + Math.Max(delaySeconds, 0.0d);
                Request = null;
                mIsGif = string.Equals(mCandidate.Method, "GET", StringComparison.OrdinalIgnoreCase)
                    && mCandidate.Url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
                TryStart();
            }

            internal bool TryStart()
            {
                if (Request != null)
                {
                    return false;
                }

                if (EditorApplication.timeSinceStartup < mStartAfterTime)
                {
                    return false;
                }

                if (mIsGif)
                {
                    Request = UnityWebRequest.Get(mCandidate.Url);
                }
                else
                {
                    if (string.Equals(mCandidate.Method, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        Request = UnityWebRequest.Get(mCandidate.Url);
                    }
                    else
                    {
                        byte[] bodyBytes = Encoding.UTF8.GetBytes(mCandidate.Body ?? string.Empty);
                        UploadHandler uploadHandler = bodyBytes.Length > 0 ? new UploadHandlerRaw(bodyBytes) : null;
                        Request = new UnityWebRequest(mCandidate.Url, mCandidate.Method, new DownloadHandlerBuffer(), uploadHandler);

                        if (!string.IsNullOrEmpty(mCandidate.ContentType))
                        {
                            Request.SetRequestHeader("Content-Type", mCandidate.ContentType);
                        }
                    }
                }

                Request.SendWebRequest();
                return true;
            }

            internal ImageRequest CreateNext(double delaySeconds)
            {
                return new ImageRequest(mLogicalRequest, mCandidateIndex + 1, delaySeconds);
            }

            internal AnimatedTexture GetAnimatedTexture()
            {
                var decoder = new AB.MDV.GIF.Decoder(Request.downloadHandler.data);
                var img = decoder.NextImage();
                var anim = AnimatedTexture.Create(CacheKey);

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

                return CreateTexture(downloadHandler.data);
            }

            internal byte[] GetBytes()
            {
                var downloadHandler = Request.downloadHandler as DownloadHandlerBuffer;
                return downloadHandler?.data;
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
            return FetchImage(CreateGenericRequest(RemapURL(url)));
        }

        /// <summary>
        /// Fetches an image using a logical request with fallback candidates and optional disk caching.
        /// </summary>
        /// <param name="request">The image request to fetch.</param>
        /// <returns>The cached texture, or a placeholder if loading.</returns>
        public Texture FetchImage(MarkdownImageRequest request)
        {
            if (request == null)
            {
                return null;
            }

            if (mTextureCache.TryGetValue(request.CacheKey, out var tex))
            {
                return tex;
            }

            if (mFailedImageKeys.Contains(request.CacheKey))
            {
                return null;
            }

            if (mPlaceholder == null)
            {
                var style = GUI.skin.GetStyle("btnPlaceholder");
                mPlaceholder = style != null ? style.normal.background : null;
            }

            if (TryLoadDiskCache(request, out Texture diskTexture))
            {
                mTextureCache[request.CacheKey] = diskTexture;
                return diskTexture;
            }

            mActiveRequests.Add(new ImageRequest(request));
            mTextureCache[request.CacheKey] = mPlaceholder;

            return mPlaceholder;
        }

        /// <summary>
        /// Checks for completed image requests and updates the texture cache.
        /// </summary>
        /// <returns>True if any image was updated.</returns>
        public bool UpdateRequests()
        {
            foreach (var pendingRequest in mActiveRequests)
            {
                pendingRequest.TryStart();
            }

            var req = mActiveRequests.Find(r => r.IsStarted && r.CurrentRequest.isDone);

            if (req == null)
            {
                return false;
            }

#if UNITY_2020_2_OR_NEWER
            bool isProtocolError = req.CurrentRequest.result == UnityWebRequest.Result.ProtocolError;
            bool isConnectionError = req.CurrentRequest.result == UnityWebRequest.Result.ConnectionError;
#else
            bool isProtocolError = req.CurrentRequest.isHttpError;
            bool isConnectionError = req.CurrentRequest.isNetworkError;
#endif

            if (isProtocolError)
            {
                if (ShouldAdvanceMermaidRequest(req))
                {
                    AdvanceMermaidRequest(req, GetRetryDelaySeconds(req));
                    return true;
                }

                bool isEmojiCdnAsset = AB.MDV.Layout.EmojiImageSupport.IsConfiguredEmojiSourceUrl(req.CurrentUrl);
                if (IsMermaidDiagram(req))
                {
                    WarnOnce(req.CacheKey, string.Format(
                        "Mermaid diagram render failed after {0} attempts across all endpoints. The viewer is falling back to text for '{1}'.",
                        req.CandidateCount,
                        req.DisplayName));
                }
                else if (!isEmojiCdnAsset)
                {
                    Debug.LogError(string.Format("HTTP Error: {0} - {1} {2}", req.CurrentUrl, req.CurrentRequest.responseCode, req.CurrentRequest.error));
                }
                else
                {
                    WarnOnce(req.CacheKey, string.Format(
                        "Emoji fallback image was not found for '{0}'. The viewer will fall back to text rendering for this emoji.",
                        req.CurrentUrl));
                }

                mTextureCache[req.CacheKey] = null;
                mFailedImageKeys.Add(req.CacheKey);
            }
            else if (isConnectionError)
            {
                if (ShouldAdvanceMermaidRequest(req))
                {
                    AdvanceMermaidRequest(req, MermaidRetryDelaySeconds);
                    return true;
                }

                if (AB.MDV.Layout.EmojiImageSupport.IsConfiguredEmojiSourceUrl(req.CurrentUrl))
                {
                    WarnOnce(req.CacheKey, string.Format(
                        "Emoji fallback image could not be downloaded for '{0}'. Check your internet connection if you want color emoji rendering in older Unity versions.",
                        req.CurrentUrl));
                }
                else if (IsMermaidDiagram(req))
                {
                    WarnOnce(req.CacheKey, string.Format(
                        "Mermaid diagram could not be downloaded after {0} attempts across all endpoints. The viewer is falling back to text for '{1}'.",
                        req.CandidateCount,
                        req.DisplayName));
                }
                else
                {
                    Debug.LogError(string.Format("Network Error: {0} - {1}", req.CurrentUrl, req.CurrentRequest.error));
                }

                mTextureCache[req.CacheKey] = null;
                mFailedImageKeys.Add(req.CacheKey);
            }
            else if (req.IsAnimatedGif)
            {
                var anim = req.GetAnimatedTexture();

                if (anim != null && anim.TextureCount > 0)
                {
                    mTextureCache[req.CacheKey] = anim.FirstTexture;

                    if (anim.TextureCount > 1)
                    {
                        mAnimatedTextures.Add(anim);
                    }
                }
            }
            else
            {
                byte[] responseBytes = req.GetBytes();
                Texture texture = CreateTexture(responseBytes);

                if (texture == null && ShouldAdvanceMermaidRequest(req))
                {
                    AdvanceMermaidRequest(req, 0.0d);
                    return true;
                }

                if (texture == null)
                {
                    mTextureCache[req.CacheKey] = null;
                    mFailedImageKeys.Add(req.CacheKey);
                }
                else
                {
                    mTextureCache[req.CacheKey] = texture;
                    SaveDiskCache(req, responseBytes);
                }
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
                    mTextureCache[anim.CacheKey] = anim.CurrentTexture;
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

        private MarkdownImageRequest CreateGenericRequest(string url)
        {
            return new MarkdownImageRequest(
                new[] { new MarkdownImageCandidate(url) },
                url,
                url,
                MarkdownImageRequestKind.Generic,
                false);
        }

        private static bool IsMermaidDiagram(ImageRequest request)
        {
            return request.Kind == MarkdownImageRequestKind.MermaidDiagram;
        }

        private static bool IsTransientMermaidFailure(ImageRequest request)
        {
            if (!IsMermaidDiagram(request))
            {
                return false;
            }

#if UNITY_2020_2_OR_NEWER
            if (request.CurrentRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                long responseCode = request.CurrentRequest.responseCode;
                return responseCode == 429 || responseCode >= 500;
            }

            return request.CurrentRequest.result == UnityWebRequest.Result.ConnectionError;
#else
            if (request.CurrentRequest.isNetworkError)
            {
                return true;
            }

            if (request.CurrentRequest.isHttpError)
            {
                long responseCode = request.CurrentRequest.responseCode;
                return responseCode == 429 || responseCode >= 500;
            }

            return false;
#endif
        }

        private static Texture CreateTexture(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            return texture.LoadImage(imageData, true) ? texture : null;
        }

        private static bool ShouldAdvanceMermaidRequest(ImageRequest request)
        {
            return IsMermaidDiagram(request) && request.CanAdvance;
        }

        private static double GetRetryDelaySeconds(ImageRequest request)
        {
            return IsTransientMermaidFailure(request) ? MermaidRetryDelaySeconds : 0.0d;
        }

        private void AdvanceMermaidRequest(ImageRequest request, double delaySeconds)
        {
            // CHANGED: Mermaid diagrams now advance through retries and backup render services instead of failing permanently on the first outage.
            ImageRequest retryRequest = request.CreateNext(delaySeconds);
            mActiveRequests.Add(retryRequest);
            mTextureCache[request.CacheKey] = mPlaceholder;
            mActiveRequests.Remove(request);
        }

        private static string GetDiskCachePath(MarkdownImageRequest request)
        {
            return Path.Combine(MarkdownPreferences.MermaidDiskCacheDirectory, $"{request.CacheKey}.png");
        }

        private static bool CanUseDiskCache(MarkdownImageRequest request)
        {
            return request != null
                && request.EnableDiskCache
                && request.Kind == MarkdownImageRequestKind.MermaidDiagram
                && MarkdownPreferences.MermaidDiskCacheEnabled;
        }

        private bool TryLoadDiskCache(MarkdownImageRequest request, out Texture texture)
        {
            texture = null;

            if (!CanUseDiskCache(request))
            {
                return false;
            }

            string cachePath = GetDiskCachePath(request);
            if (!File.Exists(cachePath))
            {
                return false;
            }

            try
            {
                // CHANGED: Successful Mermaid renders are persisted so charts can reopen instantly without another network round-trip.
                texture = CreateTexture(File.ReadAllBytes(cachePath));
                return texture != null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to read Mermaid diagram cache '{cachePath}': {exception.Message}");
                return false;
            }
        }

        private void SaveDiskCache(ImageRequest request, byte[] imageData)
        {
            if (request == null || !CanUseDiskCache(request.LogicalRequest) || imageData == null || imageData.Length == 0)
            {
                return;
            }

            try
            {
                string cachePath = GetDiskCachePath(request.LogicalRequest);
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                File.WriteAllBytes(cachePath, imageData);
            }
            catch (Exception exception)
            {
                WarnOnce(request.CacheKey, $"Mermaid diagram cache could not be written: {exception.Message}");
            }
        }

        private void WarnOnce(string cacheKey, string message)
        {
            if (!mWarnedImageKeys.Add(cacheKey))
            {
                return;
            }

            Debug.LogWarning(message);
        }
    }
}
