// ============================================================
// File:    StyleConverter.cs
// Purpose: Converts the abstract Style struct into concrete Unity GUIStyles.
// Author:  Ahmad Albahar
// ============================================================

using UnityEngine;

namespace AB.MDV.Layout
{
    /// <summary>
    /// Manages the conversion of <see cref="Style"/> values into <see cref="GUIStyle"/> objects.
    /// Uses a set of reference styles and applies theme colors and attributes dynamically.
    /// </summary>
    public class StyleConverter
    {
        private Style      mCurrentStyle = Style.Default;
        private GUIStyle[] mWorking;
        private GUIStyle[] mReference;

        private const int FixedBlock  = 7;
        private const int Variable    = 8;
        private const int FixedInline = 12;

        private static readonly string[] CustomStyles = new string[] {
            "variable",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "fixed_block",
            "variable",
            "variable_bold",
            "variable_italic",
            "variable_bolditalic",
            "fixed_inline",
            "fixed_inline_bold",
            "fixed_inline_italic",
            "fixed_inline_bolditalic",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleConverter"/> class.
        /// </summary>
        /// <param name="skin">The GUISkin containing the reference styles.</param>
        public StyleConverter( GUISkin skin )
        {
            mReference = new GUIStyle[ CustomStyles.Length ];
            mWorking   = new GUIStyle[ CustomStyles.Length ];

            for( var i = 0; i < CustomStyles.Length; i++ )
            {
                mReference[ i ] = GetOrCreateStyle( skin, CustomStyles[ i ] );
                mWorking[ i ]   = new GUIStyle( mReference[ i ] );
            }

            var colors = MarkdownTheme.Instance.Active;

            ApplyBlockStyle( GetOrCreateStyle( skin, "blockcode" ),  colors.CodeBackground,        new RectOffset( 12, 12, 8,  8  ) );
            ApplyBlockStyle( GetOrCreateStyle( skin, "blockquote" ), colors.QuoteBackground,        new RectOffset( 20, 12, 8,  8  ) );
            ApplyBlockStyle( GetOrCreateStyle( skin, "th" ),         colors.TableHeaderBackground,  new RectOffset( 12, 12, 4,  4  ) );
            ApplyBlockStyle( GetOrCreateStyle( skin, "tr" ),         colors.TableRowOddBackground,  new RectOffset( 12, 12, 4,  4  ) );
            ApplyBlockStyle( GetOrCreateStyle( skin, "trl" ),        colors.TableRowEvenBackground, new RectOffset( 12, 12, 4,  4  ) );
            ApplyBlockStyle( GetOrCreateStyle( skin, "hr" ),         colors.Separator,              new RectOffset( 0,  0,  0,  0  ) );
        }

        /// <summary>
        /// Gets a style from the skin, or creates a default one if it doesn't exist.
        /// Ensures all styles have wordWrap enabled for proper text layout.
        /// </summary>
        private GUIStyle GetOrCreateStyle( GUISkin skin, string styleName )
        {
            var style = skin.GetStyle( styleName );
            if( style != null )
            {
                return style;
            }

            // Create fallback styles based on the style name
            style = new GUIStyle( skin.label );

            // Enable text wrapping by default for all styles
            style.wordWrap = true;

            // Apply base styles for specific element types
            if( styleName.Contains( "h1" ) )
            {
                style.fontSize = 24;
                style.fontStyle = FontStyle.Bold;
            }
            else if( styleName.Contains( "h2" ) )
            {
                style.fontSize = 20;
                style.fontStyle = FontStyle.Bold;
            }
            else if( styleName.Contains( "h3" ) )
            {
                style.fontSize = 18;
                style.fontStyle = FontStyle.Bold;
            }
            else if( styleName.Contains( "h4" ) )
            {
                style.fontSize = 16;
                style.fontStyle = FontStyle.Bold;
            }
            else if( styleName.Contains( "h5" ) )
            {
                style.fontSize = 14;
                style.fontStyle = FontStyle.Bold;
            }
            else if( styleName.Contains( "h6" ) )
            {
                style.fontSize = 13;
                style.fontStyle = FontStyle.Bold;
            }
            else if( styleName.Contains( "fixed" ) || styleName.Contains( "blockcode" ) )
            {
                style.fontStyle = FontStyle.Normal;
            }

            // Apply font style variants
            if( styleName.Contains( "bold" ) && !styleName.Contains( "h" ) )
            {
                style.fontStyle = FontStyle.Bold;
            }
            if( styleName.Contains( "italic" ) )
            {
                style.fontStyle |= FontStyle.Italic;
            }

            return style;
        }

        private void ApplyBlockStyle( GUIStyle style, Color color, RectOffset padding )
        {
            if( style == null ) return;
            style.padding              = padding;
            style.normal.background    = MarkdownUtils.GetColorTexture( color );
        }

        /// <summary>
        /// Applies the specified <see cref="Style"/> and returns the corresponding <see cref="GUIStyle"/>.
        /// </summary>
        /// <param name="src">The source style attributes.</param>
        /// <returns>The resulting GUIStyle.</returns>
        public GUIStyle Apply( Style src )
        {
            var style  = src.Block ? mWorking[ FixedBlock ] : mWorking[ src.Size ];
            var colors = MarkdownTheme.Instance.Active;

            if( mCurrentStyle != src )
            {
                var fontIdx   = ( src.Fixed ? FixedInline : Variable ) + ( src.Bold ? 1 : 0 ) + ( src.Italic ? 2 : 0 );
                var reference = mReference[ fontIdx ];

                style.font      = reference.font;
                style.fontStyle = reference.fontStyle;
                style.richText  = src.RichText;
                style.fontSize  = colors.TextSize;
                style.padding   = new RectOffset( 0, 0, 0, 0 );

                if( src.Subscript || src.Superscript )
                {
                    style.fontSize         = Mathf.RoundToInt( colors.TextSize * 0.75f );
                    style.normal.textColor = src.Subscript ? colors.SubscriptColor : colors.SuperscriptColor;
                }
                else if( src.Link )
                {
                    style.normal.textColor = colors.Link;
                }
                else
                {
                    switch( src.Size )
                    {
                        case 1:
                            style.normal.textColor = colors.Heading1;
                            style.fontSize         = colors.Heading1Size;
                            style.fontStyle        = FontStyle.Bold;
                            style.padding          = new RectOffset( 0, 0, 15, 10 );
                            break;
                        case 2:
                            style.normal.textColor = colors.Heading2;
                            style.fontSize         = colors.Heading2Size;
                            style.fontStyle        = FontStyle.Bold;
                            style.padding          = new RectOffset( 0, 0, 10, 5 );
                            break;
                        case 3:  style.normal.textColor = colors.Heading3; style.fontSize = colors.Heading3Size; break;
                        case 4:  style.normal.textColor = colors.Heading4; style.fontSize = colors.Heading4Size; break;
                        case 5:  style.normal.textColor = colors.Heading5; style.fontSize = colors.Heading5Size; break;
                        case 6:  style.normal.textColor = colors.Heading6; style.fontSize = colors.Heading6Size; break;
                        default: style.normal.textColor = colors.Text; break;
                    }
                }

                if( src.Strikethrough )
                {
                    style.richText = true;
                }

                style.font = FontCompatibility.ResolveFont(reference.font, src, style.fontSize);

                mCurrentStyle = src;
            }

            return style;
        }
    }
}
