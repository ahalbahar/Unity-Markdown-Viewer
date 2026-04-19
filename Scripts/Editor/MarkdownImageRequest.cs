// ============================================================
// File:    MarkdownImageRequest.cs
// Purpose: Describes remote image requests with fallback candidates.
// Author:  Ahmad Albahar
// Created: 2026-04-19
// Notes:   Used for Mermaid chart rendering, caching, and fallback endpoints.
// ============================================================

using System;
using System.Collections.Generic;

namespace AB.MDV
{
    /// <summary>
    /// Identifies the high-level purpose of an image request.
    /// </summary>
    public enum MarkdownImageRequestKind
    {
        /// <summary>
        /// A regular markdown image without special retry or disk-cache behavior.
        /// </summary>
        Generic,

        /// <summary>
        /// A rendered Mermaid diagram with service fallback and optional disk caching.
        /// </summary>
        MermaidDiagram,
    }

    /// <summary>
    /// Describes one remote image fetch candidate.
    /// </summary>
    public sealed class MarkdownImageCandidate
    {
        /// <summary>
        /// Gets the request URL.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets the optional request body.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Gets the optional content type for requests with a body.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownImageCandidate"/> class.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="body">The optional request body.</param>
        /// <param name="contentType">The optional content type.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> or <paramref name="method"/> is empty.</exception>
        public MarkdownImageCandidate(string url, string method = "GET", string body = "", string contentType = "")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Image candidate URL must not be empty.", nameof(url));
            }

            if (string.IsNullOrWhiteSpace(method))
            {
                throw new ArgumentException("Image candidate method must not be empty.", nameof(method));
            }

            Url = url;
            Method = method;
            Body = body ?? string.Empty;
            ContentType = contentType ?? string.Empty;
        }
    }

    /// <summary>
    /// Describes a logical image request with one or more fallback candidates.
    /// </summary>
    public sealed class MarkdownImageRequest
    {
        /// <summary>
        /// Gets the ordered request candidates that should be tried.
        /// </summary>
        public IReadOnlyList<MarkdownImageCandidate> Candidates { get; }

        /// <summary>
        /// Gets the stable cache key for this logical request.
        /// </summary>
        public string CacheKey { get; }

        /// <summary>
        /// Gets the human-readable display name used in warnings and alt text.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the request category.
        /// </summary>
        public MarkdownImageRequestKind Kind { get; }

        /// <summary>
        /// Gets a value indicating whether successful responses should be stored on disk.
        /// </summary>
        public bool EnableDiskCache { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownImageRequest"/> class.
        /// </summary>
        /// <param name="candidates">The ordered fallback candidates.</param>
        /// <param name="cacheKey">The stable cache key for this request.</param>
        /// <param name="displayName">The user-facing request description.</param>
        /// <param name="kind">The request category.</param>
        /// <param name="enableDiskCache">True to save successful responses on disk.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="candidates"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="candidates"/> is empty or <paramref name="cacheKey"/> is empty.</exception>
        public MarkdownImageRequest(
            IReadOnlyList<MarkdownImageCandidate> candidates,
            string cacheKey,
            string displayName,
            MarkdownImageRequestKind kind,
            bool enableDiskCache)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (candidates.Count == 0)
            {
                throw new ArgumentException("At least one image request candidate is required.", nameof(candidates));
            }

            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentException("Image request cache key must not be empty.", nameof(cacheKey));
            }

            Candidates = candidates;
            CacheKey = cacheKey;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "image" : displayName;
            Kind = kind;
            EnableDiskCache = enableDiskCache;
        }
    }
}
