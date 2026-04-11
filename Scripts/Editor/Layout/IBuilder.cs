// ============================================================
// File:    IBuilder.cs
// Purpose: Interface for building the layout structure of a Markdown document.
// Author:  Ahmad Albahar
// ============================================================

namespace AB.MDV.Layout
{
    /// <summary>
    /// Defines the methods required to build a structured layout from Markdown source.
    /// Used by the parser to translate Markdown tokens into renderable blocks and content.
    /// </summary>
    public interface IBuilder
    {
        /// <summary>
        /// Adds a text fragment to the current block.
        /// </summary>
        /// <param name="text">The text content.</param>
        /// <param name="style">The style to apply.</param>
        /// <param name="link">Optional URL or anchor link.</param>
        /// <param name="tooltip">Optional tooltip text.</param>
        void Text(string text, Style style, string link, string tooltip);

        /// <summary>
        /// Adds an image to the current block.
        /// </summary>
        /// <param name="url">The image URL or path.</param>
        /// <param name="alt">Optional alternative text.</param>
        /// <param name="tooltip">Optional tooltip text.</param>
        /// <param name="width">Optional override width.</param>
        /// <param name="height">Optional override height.</param>
        void Image(string url, string alt, string tooltip, int width = 0, int height = 0);

        /// <summary>
        /// Inserts a line break within the current content block.
        /// </summary>
        void NewLine();

        /// <summary>
        /// Inserts vertical spacing between blocks.
        /// </summary>
        void Space();

        /// <summary>
        /// Inserts a horizontal rule (hr) separator.
        /// </summary>
        void HorizontalLine();

        /// <summary>
        /// Increases the current indentation level.
        /// </summary>
        void Indent();

        /// <summary>
        /// Decreases the current indentation level.
        /// </summary>
        void Outdent();

        /// <summary>
        /// Sets a prefix (e.g., bullet point or list number) for the current content block.
        /// </summary>
        /// <param name="text">The prefix text.</param>
        /// <param name="style">The style for the prefix.</param>
        void Prefix(string text, Style style);

        /// <summary>
        /// Starts a new layout block (e.g., a paragraph or blockquote).
        /// </summary>
        /// <param name="quoted">True if the block is a blockquote.</param>
        void StartBlock(bool quoted);

        /// <summary>
        /// Ends the current layout block.
        /// </summary>
        void EndBlock();

        /// <summary>
        /// Starts a new table structure.
        /// </summary>
        void StartTable();

        /// <summary>
        /// Ends the current table structure.
        /// </summary>
        void EndTable();

        /// <summary>
        /// Starts a new row within a table.
        /// </summary>
        /// <param name="isHeader">True if the row is a header row.</param>
        void StartTableRow(bool isHeader);

        /// <summary>
        /// Ends the current table row.
        /// </summary>
        void EndTableRow();
    }
}
