// ============================================================
// File:    Style.cs
// Purpose: A lightweight struct for managing markdown text styles using bitflags.
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Represents a collection of text style attributes (bold, italic, size, etc.) stored as bitflags.
    /// </summary>
    public struct Style
    {
        /// <summary>
        /// The default, unstyled state.
        /// </summary>
        public static readonly Style Default = new Style();

        private const int FlagBold = 0x0100;
        private const int FlagItalic = 0x0200;
        private const int FlagFixed = 0x0400;
        private const int FlagLink = 0x0800;
        private const int FlagBlock = 0x1000;
        private const int FlagRichText = 0x2000;
        private const int FlagStrikethrough = 0x4000;
        private const int FlagHighlight = 0x00010000;
        private const int FlagSubscript = 0x00020000;
        private const int FlagSuperscript = 0x00040000;
        private const int FlagUnderline = 0x00080000;

        private const int MaskSize = 0x000F;
        private const int MaskWeight = 0x0300;

        private int mStyle;

        /// <summary>
        /// Determines whether two styles contain the same flags.
        /// </summary>
        /// <param name="a">The first style value.</param>
        /// <param name="b">The second style value.</param>
        /// <returns><see langword="true"/> when both styles are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Style a, Style b) => a.mStyle == b.mStyle;

        /// <summary>
        /// Determines whether two styles contain different flags.
        /// </summary>
        /// <param name="a">The first style value.</param>
        /// <param name="b">The second style value.</param>
        /// <returns><see langword="true"/> when the styles differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Style a, Style b) => a.mStyle != b.mStyle;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Style other && other.mStyle == mStyle;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return mStyle.GetHashCode();
        }

        /// <summary>
        /// Resets the style to its default state.
        /// </summary>
        public void Clear()
        {
            mStyle = 0x0000;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text is bold.
        /// </summary>
        public bool Bold
        {
            get => (mStyle & FlagBold) != 0;
            set { if (value) mStyle |= FlagBold; else mStyle &= ~FlagBold; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text is italic.
        /// </summary>
        public bool Italic
        {
            get => (mStyle & FlagItalic) != 0;
            set { if (value) mStyle |= FlagItalic; else mStyle &= ~FlagItalic; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text uses a fixed-width font.
        /// </summary>
        public bool Fixed
        {
            get => (mStyle & FlagFixed) != 0;
            set { if (value) mStyle |= FlagFixed; else mStyle &= ~FlagFixed; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text is a link.
        /// </summary>
        public bool Link
        {
            get => (mStyle & FlagLink) != 0;
            set { if (value) mStyle |= FlagLink; else mStyle &= ~FlagLink; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text is part of a block.
        /// </summary>
        public bool Block
        {
            get => (mStyle & FlagBlock) != 0;
            set { if (value) mStyle |= FlagBlock; else mStyle &= ~FlagBlock; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text should be treated as rich text.
        /// </summary>
        public bool RichText
        {
            get => (mStyle & FlagRichText) != 0;
            set { if (value) mStyle |= FlagRichText; else mStyle &= ~FlagRichText; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text has a strikethrough.
        /// </summary>
        public bool Strikethrough
        {
            get => (mStyle & FlagStrikethrough) != 0;
            set { if (value) mStyle |= FlagStrikethrough; else mStyle &= ~FlagStrikethrough; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text is highlighted.
        /// </summary>
        public bool Highlight
        {
            get => (mStyle & FlagHighlight) != 0;
            set { if (value) mStyle |= FlagHighlight; else mStyle &= ~FlagHighlight; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text is a subscript.
        /// </summary>
        public bool Subscript
        {
            get => (mStyle & FlagSubscript) != 0;
            set { if (value) mStyle |= FlagSubscript; else mStyle &= ~FlagSubscript; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text is a superscript.
        /// </summary>
        public bool Superscript
        {
            get => (mStyle & FlagSuperscript) != 0;
            set { if (value) mStyle |= FlagSuperscript; else mStyle &= ~FlagSuperscript; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text is underlined.
        /// </summary>
        public bool Underline
        {
            get => (mStyle & FlagUnderline) != 0;
            set { if (value) mStyle |= FlagUnderline; else mStyle &= ~FlagUnderline; }
        }

        /// <summary>
        /// Gets or sets the font size multiplier (0-6).
        /// </summary>
        public int Size
        {
            get => mStyle & MaskSize;
            set { mStyle = (mStyle & ~MaskSize) | Mathf.Clamp(value, 0, 6); }
        }

        /// <summary>
        /// Converts the internal bitflags to a Unity <see cref="UnityEngine.FontStyle"/>.
        /// </summary>
        /// <returns>The corresponding font style.</returns>
        public FontStyle GetFontStyle()
        {
            switch (mStyle & MaskWeight)
            {
                case FlagBold: return FontStyle.Bold;
                case FlagItalic: return FontStyle.Italic;
                case FlagBold | FlagItalic: return FontStyle.BoldAndItalic;
                default: return FontStyle.Normal;
            }
        }
    }
}
