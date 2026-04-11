// ============================================================
// File:    SyntaxHighlighter.cs
// Purpose: Multi-language syntax highlighting via Unity Rich Text color tags.
// Author:  Ahmad Albahar
// ============================================================

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AB.MDV.Renderer
{
    /// <summary>
    /// Provides static methods for syntax highlighting source code using Unity's rich text tags.
    /// Supports a wide range of programming and markup languages with automatic detection.
    /// </summary>
    public static class SyntaxHighlighter
    {
        private static Dictionary<string, Regex> mPatterns = new Dictionary<string, Regex>();

        static SyntaxHighlighter()
        {
            const RegexOptions Opts = RegexOptions.Compiled | RegexOptions.ExplicitCapture;

            // ── C# ────────────────────────────────────────────────────────────
            var csKeywords = @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|var|async|await|get|set|yield|where|partial|record|init|with|global|file|required|scoped)\b";

            mPatterns["cs"] = mPatterns["csharp"] = mPatterns["java"] = new Regex(
                $@"(?<comment>//.*|/\*[\s\S]*?\*/)|(?<string>""[^""]*""|'[^']*'|@""[^""]*"")|(?<preprocessor>#\w+)|(?<keyword>{csKeywords})|(?<number>\b\d+(\.\d+)?\b)|(?<method>\b\w+(?=\s*\())|(?<type>\b[A-Z]\w*\b)",
                Opts);

            // ── JavaScript / TypeScript ────────────────────────────────────────
            mPatterns["js"] = mPatterns["javascript"] = mPatterns["ts"] = mPatterns["typescript"] = new Regex(
                $@"(?<comment>//.*|/\*[\s\S]*?\*/)|(?<string>""[^""]*""|'[^']*'|`[^`]*`)|(?<keyword>\b(?:var|let|const|function|class|extends|implements|if|else|for|while|do|switch|case|break|continue|return|throw|try|catch|finally|async|await|import|export|from|default|new|this|super|typeof|instanceof|void|null|undefined|true|false|type|interface|enum|namespace|declare|readonly|public|private|protected|abstract|static|override)\b)|(?<number>\b\d+(\.\d+)?\b)|(?<method>\b\w+(?=\s*\())|(?<type>\b[A-Z]\w*\b)",
                Opts);

            // ── C / C++ ───────────────────────────────────────────────────────
            var cppKeywords = @"\b(auto|break|case|char|const|continue|default|do|double|else|enum|extern|float|for|goto|if|inline|int|long|namespace|new|nullptr|operator|private|protected|public|register|return|short|signed|sizeof|static|struct|switch|template|this|throw|try|typedef|union|unsigned|using|virtual|void|volatile|while|bool|class|delete|explicit|false|friend|mutable|noexcept|override|true|typename)\b";

            mPatterns["cpp"] = mPatterns["c++"] = mPatterns["c"] = new Regex(
                $@"(?<comment>//.*|/\*[\s\S]*?\*/)|(?<preprocessor>#\s*(?:include|define|ifdef|ifndef|endif|pragma|undef|if|else|elif|error|warning)\b[^\n]*)|(?<string>""[^""]*""|'[^']*')|(?<keyword>{cppKeywords})|(?<number>\b\d+(\.\d+)?[uUlLfF]*\b)|(?<method>\b\w+(?=\s*\())|(?<type>\b[A-Z]\w*\b)",
                Opts);

            // ── HLSL / GLSL ───────────────────────────────────────────────────
            var shaderKeywords = @"\b(void|float|float2|float3|float4|float4x4|float3x3|float2x2|int|int2|int3|int4|uint|uint2|uint3|uint4|bool|bool2|bool3|bool4|half|half2|half3|half4|double|matrix|vector|struct|cbuffer|register|sampler|sampler2D|sampler3D|samplerCUBE|Texture2D|Texture3D|TextureCube|SamplerState|RWTexture2D|Buffer|RWBuffer|uniform|varying|attribute|in|out|inout|const|static|extern|inline|return|if|else|for|while|do|break|continue|discard|layout|precision|highp|mediump|lowp|vec2|vec3|vec4|mat2|mat3|mat4|ivec2|ivec3|ivec4|bvec2|bvec3|bvec4|sampler2DShadow|gl_Position|gl_FragCoord|gl_FragColor|SV_Position|SV_Target|SV_Depth|POSITION|NORMAL|TEXCOORD|COLOR|TANGENT|vertex|fragment|kernel|shared|groupshared|numthreads|RasterizerOrderedTexture2D)\b";

            mPatterns["hlsl"] = mPatterns["glsl"] = mPatterns["shader"] = new Regex(
                $@"(?<comment>//.*|/\*[\s\S]*?\*/)|(?<preprocessor>#\s*(?:include|define|pragma|if|ifdef|ifndef|else|endif|undef)\b[^\n]*)|(?<string>""[^""]*"")|(?<keyword>{shaderKeywords})|(?<number>\b\d+(\.\d+)?[fF]?\b)|(?<method>\b\w+(?=\s*\())|(?<type>\b[A-Z]\w*\b)",
                Opts);

            // ── Python ────────────────────────────────────────────────────────
            mPatterns["python"] = mPatterns["py"] = new Regex(
                $@"(?<comment>#.*|""""""[\s\S]*?""""""|'''[\s\S]*?''')|(?<string>""[^""]*""|'[^']*'|f""[^""]*""|f'[^']*'|r""[^""]*""|r'[^']*')|(?<keyword>\b(?:def|class|if|else|elif|for|while|try|except|finally|with|as|return|yield|import|from|global|nonlocal|lambda|pass|break|continue|raise|del|assert|and|or|not|in|is|True|False|None|async|await)\b)|(?<number>\b\d+(\.\d+)?\b)|(?<method>\b\w+(?=\s*\())|(?<type>\b[A-Z]\w*\b)",
                Opts);

            // ── Bash / Shell ──────────────────────────────────────────────────
            mPatterns["bash"] = mPatterns["sh"] = mPatterns["zsh"] = mPatterns["shell"] = new Regex(
                $@"(?<comment>#.*)|(?<string>""[^""]*""|'[^']*'|\$\{{[^}}]*\}})|(?<keyword>\b(?:if|then|else|elif|fi|for|in|do|done|while|until|case|esac|function|local|return|exit|echo|printf|cd|ls|grep|cat|mkdir|rm|cp|mv|chmod|chown|export|source|alias|unset|read|test|true|false|sudo|apt|apt-get|yum|brew|pip|npm|yarn)\b)|(?<number>\b\d+\b)",
                Opts);

            // ── PowerShell ────────────────────────────────────────────────────
            mPatterns["powershell"] = mPatterns["ps1"] = new Regex(
                $@"(?<comment>#.*|<#[\s\S]*?#>)|(?<string>""[^""]*""|'[^']*')|(?<keyword>\b(?:if|else|elseif|switch|foreach|while|do|until|function|param|process|begin|end|return|try|catch|finally|throw|exit|break|continue|class|enum|using|namespace)\b)|(?<number>\b\d+\b)",
                Opts);

            // ── Rust ──────────────────────────────────────────────────────────
            var rustKeywords = @"\b(fn|let|mut|const|static|struct|enum|impl|trait|for|in|if|else|while|loop|match|return|use|mod|pub|crate|super|self|Self|type|where|async|await|move|ref|dyn|Box|Vec|String|Option|Result|Some|None|Ok|Err|true|false|break|continue)\b";

            mPatterns["rust"] = mPatterns["rs"] = new Regex(
                $@"(?<comment>//.*|/\*[\s\S]*?\*/)|(?<string>""[^""]*""|'[^']*'|r#""[^""]*""#)|(?<keyword>{rustKeywords})|(?<number>\b\d+(\.\d+)?(_\w+)?\b)|(?<method>\b\w+(?=\s*\())|(?<type>\b[A-Z]\w*\b)",
                Opts);

            // ── JSON ──────────────────────────────────────────────────────────
            mPatterns["json"] = new Regex(
                $@"(?<string>""[^""]*"")|(?<keyword>\b(?:true|false|null)\b)|(?<number>\b-?\d+(\.\d+)?([eE][+-]?\d+)?\b)",
                Opts);

            // ── YAML ──────────────────────────────────────────────────────────
            mPatterns["yaml"] = mPatterns["yml"] = new Regex(
                $@"(?<comment>#.*)|(?<type>^\s*[\w\-]+(?=\s*:))|(?<string>""[^""]*""|'[^']*'|(?<=:\s)\S.*$)|(?<keyword>\b(?:true|false|null|yes|no|on|off)\b)|(?<number>\b-?\d+(\.\d+)?\b)",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            // ── CSS ───────────────────────────────────────────────────────────
            mPatterns["css"] = new Regex(
                $@"(?<comment>/\*[\s\S]*?\*/)|(?<type>[.#]?[\w-]+(?=\s*\{{))|(?<string>""[^""]*""|'[^']*')|(?<keyword>(?<=[\s:])(?:auto|none|inherit|initial|unset|normal|bold|italic|underline|flex|grid|block|inline|absolute|relative|fixed|sticky|center|left|right|top|bottom|solid|dashed|dotted|transparent|important)(?=[\s;,)]))|(?<number>-?\d+(\.\d+)?(?:px|em|rem|vh|vw|%|pt|fr|s|ms)?)",
                Opts);

            // ── Lua ───────────────────────────────────────────────────────────
            mPatterns["lua"] = new Regex(
                $@"(?<comment>--\[\[[\s\S]*?\]\]|--.*)|(?<string>""[^""]*""|'[^']*'|\[\[[\s\S]*?\]\])|(?<keyword>\b(?:and|break|do|else|elseif|end|false|for|function|goto|if|in|local|nil|not|or|repeat|return|then|true|until|while)\b)|(?<number>\b\d+(\.\d+)?\b)|(?<method>\b\w+(?=\s*\())|(?<type>\b[A-Z]\w*\b)",
                Opts);

            // ── SQL ───────────────────────────────────────────────────────────
            mPatterns["sql"] = new Regex(
                $@"(?<comment>--.*|/\*[\s\S]*?\*/)|(?<string>'[^']*'|""[^""]*"")|(?<keyword>\b(?:SELECT|FROM|WHERE|INSERT|INTO|UPDATE|SET|DELETE|CREATE|DROP|ALTER|TABLE|VIEW|INDEX|DATABASE|SCHEMA|JOIN|LEFT|RIGHT|INNER|OUTER|FULL|CROSS|ON|AS|AND|OR|NOT|IN|EXISTS|BETWEEN|LIKE|IS|NULL|DISTINCT|GROUP|BY|ORDER|HAVING|LIMIT|OFFSET|UNION|ALL|CASE|WHEN|THEN|ELSE|END|BEGIN|COMMIT|ROLLBACK|TRANSACTION|PRIMARY|KEY|FOREIGN|REFERENCES|UNIQUE|DEFAULT|CHECK|CONSTRAINT|INT|INTEGER|VARCHAR|TEXT|BOOLEAN|FLOAT|DOUBLE|DECIMAL|DATE|TIMESTAMP|BLOB|AUTO_INCREMENT|SERIAL|IDENTITY)\b)",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

            // ── XML / HTML ────────────────────────────────────────────────────
            mPatterns["xml"] = mPatterns["html"] = new Regex(
                $@"(?<comment><!--[\s\S]*?-->)|(?<string>""[^""]*""|'[^']*')|(?<keyword><[!\/]?\w+[^>]*>)",
                Opts);
        }

        /// <summary>
        /// Highlights the specified source code using the chosen language rules.
        /// </summary>
        /// <param name="code">The raw source code text.</param>
        /// <param name="lang">The language identifier (e.g., "cs", "py"). If null, auto-detection is used.</param>
        /// <returns>The code string wrapped in Unity rich text color tags.</returns>
        public static string Highlight(string code, string lang)
        {
            if (string.IsNullOrEmpty(code)) return code;

            if (string.IsNullOrEmpty(lang))
            {
                lang = AutoDetectLanguage(code);
            }

            lang = lang.ToLower().Trim();
            if (!mPatterns.ContainsKey(lang)) return code;

            var colors = MarkdownTheme.Instance.Active;

            return mPatterns[lang].Replace(code, m =>
            {
                Color color = colors.Text;
                bool matched = false;

                if (m.Groups["comment"].Success) { color = colors.Comment; matched = true; }
                else if (m.Groups["preprocessor"].Success) { color = colors.Preprocessor; matched = true; }
                else if (m.Groups["string"].Success) { color = colors.String; matched = true; }
                else if (m.Groups["keyword"].Success) { color = colors.Keyword; matched = true; }
                else if (m.Groups["number"].Success) { color = colors.Number; matched = true; }
                else if (m.Groups["method"].Success) { color = colors.Method; matched = true; }
                else if (m.Groups["type"].Success) { color = colors.Type; matched = true; }

                if (!matched) return m.Value;

                var colorHex = GetColorTag(color);
                var content = m.Value;

                // Insert a zero-width space after every '<' to prevent Unity from stripping tags.
                content = content.Replace("<", "<\u200B");

                // Split tags across newlines for Unity IMGUI compatibility.
                if (content.Contains("\n"))
                {
                    content = content.Replace("\r", "");
                    return $"<color={colorHex}>" + content.Replace("\n", $"</color>\n<color={colorHex}>") + "</color>";
                }

                return $"<color={colorHex}>{content}</color>";
            });
        }

        /// <summary>
        /// Attempts to automatically detect the programming language based on content patterns.
        /// </summary>
        /// <param name="code">The source code to analyze.</param>
        /// <returns>A language identifier (e.g., "cs", "js", "sql") or "cs" as default.</returns>
        private static string AutoDetectLanguage(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return "cs";
            }

            // JavaScript / TypeScript
            if (Regex.IsMatch(code, @"\b(?:export|import|const|let|async|await)\b")
                && (code.Contains("{") || code.Contains("=>")))
            {
                return "ts";
            }

            // SQL
            if (Regex.IsMatch(code, @"\b(?:SELECT|CREATE\s+TABLE|INSERT\s+INTO)\b", RegexOptions.IgnoreCase))
            {
                return "sql";
            }

            // YAML
            if (Regex.IsMatch(code, @"^\w[\w\-]*\s*:", RegexOptions.Multiline) && !code.Contains("{"))
            {
                return "yaml";
            }

            // CSS
            if (Regex.IsMatch(code, @"[\w.#-]+\s*\{") && code.Contains(":") && code.Contains(";"))
            {
                return "css";
            }

            // Lua
            if (code.Contains("local ") || Regex.IsMatch(code, @"\bfunction\b[\s\S]*\bend\b"))
            {
                return "lua";
            }

            // JSON
            if (Regex.IsMatch(code, @"^\s*[\{\[]") && code.Contains("\":"))
            {
                return "json";
            }

            // XML / HTML
            if (code.Contains("<?xml") || code.Contains("<html") || code.Contains("</div>"))
            {
                return "xml";
            }

            // Bash
            if (code.Contains("#!") || code.Contains("sudo ") || code.Contains("echo "))
            {
                return "bash";
            }

            // PowerShell
            if (code.Contains("$") && (code.Contains("Get-") || code.Contains("Write-Host")))
            {
                return "ps1";
            }

            return "cs";
        }

        // ─────────────────────────────────────────────────────────────────────

        private static string GetColorTag( Color color )
        {
            return "#" + ColorUtility.ToHtmlStringRGB( color );
        }
    }
}
