// ============================================================
// File:    MarkdownHistory.cs
// Purpose: Manages navigation history for the Markdown viewer.
// Author:  Ahmad Albahar
// ============================================================

using System.Collections.Generic;

namespace AB.MDV
{
    /// <summary>
    /// Tracks the history of viewed markdown files to support forward and backward navigation.
    /// Renamed from History to match filename.
    /// </summary>
    public class MarkdownHistory
    {
        private int mIndex = -1;
        private List<string> mHistory = new List<string>();

        /// <summary>
        /// Gets a value indicating whether the history is empty.
        /// </summary>
        public bool IsEmpty => mHistory.Count == 0;

        /// <summary>
        /// Gets the number of items in the history.
        /// </summary>
        public int Count => mHistory.Count;

        /// <summary>
        /// Gets the current path in the history.
        /// </summary>
        public string Current => mIndex >= 0 ? mHistory[mIndex] : null;

        /// <summary>
        /// Gets a value indicating whether navigation backward is possible.
        /// </summary>
        public bool CanBack => mIndex > 0;

        /// <summary>
        /// Gets a value indicating whether navigation forward is possible.
        /// </summary>
        public bool CanForward => mIndex != mHistory.Count - 1;

        /// <summary>
        /// Clears the navigation history.
        /// </summary>
        public void Clear()
        {
            mHistory.Clear();
            mIndex = -1;
        }

        /// <summary>
        /// Joins the history items into a single semicolon-separated string.
        /// </summary>
        /// <returns>A semicolon-separated string of history paths up to the current index.</returns>
        public string Join()
        {
            if (mIndex < 0)
            {
                return string.Empty;
            }

            return string.Join(";", mHistory.GetRange(0, mIndex + 1).ToArray());
        }

        /// <summary>
        /// Navigates forward in the history.
        /// </summary>
        /// <returns>The path at the new forward position.</returns>
        public string Forward()
        {
            if (CanForward)
            {
                mIndex++;
            }

            return Current;
        }

        /// <summary>
        /// Navigates backward in the history.
        /// </summary>
        /// <returns>The path at the new backward position.</returns>
        public string Back()
        {
            if (CanBack)
            {
                mIndex--;
            }

            return Current;
        }

        /// <summary>
        /// Adds a new URL to the history, clearing any forward history if necessary.
        /// </summary>
        /// <param name="url">The path to add to history.</param>
        public void Add(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (Current == url)
            {
                return;
            }

            if (mIndex + 1 < mHistory.Count)
            {
                mHistory.RemoveRange(mIndex + 1, mHistory.Count - mIndex - 1);
            }

            mHistory.Add(url);
            mIndex++;
        }

        /// <summary>
        /// Called when a markdown file is opened. Resets history if a new file is opened.
        /// </summary>
        /// <param name="url">The path of the opened file.</param>
        public void OnOpen(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (Current != url)
            {
                Clear();
                Add(url);
            }
        }
    }
}
