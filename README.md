# Unity Markdown Viewer for Unity Editor

> A Unity Markdown viewer and Unity Editor markdown inspector for `.md` and `.markdown` files, with syntax highlighting, theme presets, anchor links, tables, images, GIF support, and project documentation workflows.

## Overview

Unity Markdown Viewer is a Unity Editor plugin that renders Markdown files directly inside the Unity Inspector. It is designed for teams that keep game documentation, technical design docs, README files, changelogs, onboarding notes, and editor help content inside a Unity project.

This package is a complete rewrite and significant enhancement of the original [UnityMarkdownViewer](https://github.com/gwaredd/UnityMarkdownViewer) project. It provides a polished Markdown reader for Unity, with theme customization, syntax highlighting, anchor navigation, admonitions, images, GIF support, and a workflow that fits real Unity projects.

**Perfect for:**
- Displaying project documentation and guides
- Rendering game design documents
- Creating interactive changelogs and release notes
- Managing in-editor help and tutorials

## Keywords

Unity markdown viewer, Unity markdown editor, Unity markdown inspector, Unity README viewer, Unity documentation plugin, Unity changelog viewer, Unity `.unitypackage`, Unity Editor tools, Markdown for Unity, in-editor documentation for Unity.

## Features

### ✨ Core Features
- 📝 **Full Markdown Support** – Headers, emphasis, lists, code blocks, blockquotes, tables, and more
- 🎨 **10+ Theme Presets** – VS Code Dark+, GitHub Light/Dark, Dracula, Nord, Solarized, OneDark, Catppuccin, Monokai
- 🔗 **Interactive Anchor Navigation** – Click headings to jump to sections (anchor links like `[jump](#section-id)`)
- 📸 **Image Rendering** – Inline images and GIFs with automatic caching and web support
- 😀 **Emoji Support Across Unity Versions** – Unity 6 uses native emoji rendering, while Unity versions older than Unity 6 use a configurable CDN image source chain
- 🏷️ **GitHub-style Admonitions** – `> [!NOTE]`, `[!TIP]`, `[!WARNING]`, `[!IMPORTANT]`, `[!CAUTION]`
- 🎯 **Heading IDs** – Custom anchor IDs with `{#custom-id}` syntax

### 🎨 Syntax Highlighting
- **Multi-language support** – C#, HLSL, JavaScript, Python, Java, Go, Rust, CSS, HTML, YAML, and more
- **Smart color coding** – Keywords, types, strings, comments, numbers, methods
- **Theme-aware** – Colors adapt to selected theme

### 🎛️ Editor Features
- **Live theme preview** – Side-by-side light/dark skin comparison
- **One-click preset application** – Instantly switch between 10 beautiful themes
- **Project Settings integration** – Configure the active theme from `Project/AB/Markdown Viewer`
- **Raw text mode** – Toggle between formatted and raw markdown view
- **Responsive layout** – Automatically adapts to Inspector width
- **Dark mode support** – Seamless integration with Unity Editor UI
- **Pre-Unity-6 emoji fallback** – Unity versions older than Unity 6 display emoji through a configurable inline image fallback chain instead of unreliable IMGUI text glyphs

### 🔧 Advanced Capabilities
- **HTML entity handling** – Proper rendering of `<`, `>`, `&` and other special characters
- **Smart text wrapping** – Intelligent line breaking for long content
- **Code block syntax** – Specify language for proper highlighting (e.g., ` ```csharp `)
- **Table rendering** – Full support for GitHub-style Markdown tables
- **Escape sequences** – Raw text mode toggles markdown/plaintext view

## Current Highlights

This is a comprehensive rewrite with **nearly 100% new code**, built from the ground up with:

| Feature | Original | New |
|---------|----------|-----|
| **Anchor Navigation** | ❌ | ✅ Full scrolling support |
| **Admonitions** | ❌ | ✅ GitHub-style `[!NOTE]`, `[!TIP]`, etc. |
| **Syntax Highlighting** | ❌ | ✅ 9+ languages with theme-aware colors |
| **Theme Presets** | 1 (Dark) | ✅ 10 professional presets |
| **Image Support** | ✅ Basic | ✅ Enhanced with caching |
| **HTML Entities** | Partial | ✅ Full support |
| **Theme Editor UI** | ❌ | ✅ Visual side-by-side editor |
| **Markdig Parser** | ✅ | ✅ Enhanced integration |

## Installation

If you are searching for a Unity markdown plugin, Unity markdown renderer, or Unity README viewer for the Inspector, this package installs as a standard `.unitypackage` under `Assets/AB/Unity-Markdown-Viewer`.

### Option 1: Unity Package
1. Download the `.unitypackage` from [Releases](https://github.com/ahalbahar/Unity-Markdown-Viewer/releases)
2. Drag it into your Unity project, or use **Assets > Import Package > Custom Package**
3. Select any `.md` or `.markdown` file to view it

### Option 2: Unity Package Manager (Git URL)
1. Open **Window > Package Manager**
2. Click the **+** button
3. Choose **Add package from git URL...**
4. Enter:
   `https://github.com/ahalbahar/Unity-Markdown-Viewer.git`
5. Unity installs the package as `com.ab.unitymarkdownviewer`

You can also add it directly in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.ab.unitymarkdownviewer": "https://github.com/ahalbahar/Unity-Markdown-Viewer.git"
  }
}
```

When installed through Package Manager, the package lives under `Packages/com.ab.unitymarkdownviewer`. The editable `MarkdownTheme` asset is created automatically under `Assets/AB/Unity-Markdown-Viewer/Theme`, so project-specific theme changes stay writable.

### Option 3: Direct Folder
1. Clone or download this repository
2. Create `Assets/AB/Unity-Markdown-Viewer` in your Unity project
3. Copy this repository's package contents into that folder:
   `AB.MDV.asmdef`, `Scripts/`, `Theme/`, `Sample/`, `README.md`, `CHANGELOG.md`, `CONTRIBUTING.md`, `LICENSE.md`, and `package.json`
4. Unity automatically imports the custom editor
5. Select any `.md` or `.markdown` file to view it in the Inspector

## Release Packaging

GitHub Releases are built without Unity. The release workflow stages the repository contents into `Assets/AB/Unity-Markdown-Viewer` and uses [`natsuneko-laboratory/create-unitypackage@v3`](https://github.com/marketplace/actions/create-a-unitypackage) to generate the published `.unitypackage`.

Release workflow behavior:
- pushing a `v*` tag builds the package, uploads the artifact, and publishes the GitHub Release
- running **Actions > Release Unity Package > Run workflow** builds the package again without needing a new tag and uploads it as a workflow artifact
- manual runs can optionally set a version label; otherwise the artifact is named with `manual-<run number>`

The staged export intentionally includes only the Unity plugin payload:
`AB.MDV.asmdef`, `Scripts/`, `Theme/`, `Sample/`, `README.md`, `CHANGELOG.md`, and `LICENSE.md`.

## Usage

### Viewing Markdown Files
1. Create or place a `.md` file in your project
2. Select it in the Project window
3. View it in the Inspector (no configuration needed!)

This makes the package useful as a Unity README viewer, Unity changelog viewer, Unity documentation browser, and lightweight Markdown-based help system for editor tooling.

### Navigation
- **Back/Forward buttons** – Browse previously opened markdown files
- **Click anchor links** – `[Go to Section](#section-heading)` jumps to that heading
- **Raw toggle** – Switch between formatted markdown and raw text view

### Theming
1. Find or create a `MarkdownTheme` asset:
   - Right-click in Project window → **Create > Markdown > Theme**
2. Open the theme in either of these places:
   - Select the asset in the Project window to open the theme editor
   - Open **Project Settings > AB > Markdown Viewer** to edit the active theme from settings
3. Click any preset button to instantly apply a theme:
   - **VS Code Dark+**, **GitHub Light/Dark**, **Dracula Dark**
   - **Nord Dark**, **OneDark Dark**, **Monokai Dark**
   - **Solarized Dark/Light**, **Catppuccin Mocha Dark**
4. Fine-tune colors in the side-by-side editor
5. The Project Settings page shows the active theme asset and keeps edits in sync with the asset inspector
6. The same theme asset also contains the emoji fallback source chain used only by Unity versions older than Unity 6

### Emoji Source Chain

The `MarkdownTheme` asset includes an **Emoji Fallback Sources (Unity Versions Older Than Unity 6)** section. This is used only on Unity versions older than Unity 6, where IMGUI cannot render full-color emoji reliably.

Default behavior:
- a grapheme-aware CDN source is tried first
- OpenMoji and Twemoji codepoint PNG URLs are used as fallbacks
- sources are tried in order until one succeeds
- if all sources fail or the editor is offline, the viewer falls back to plain text instead of throwing hard errors

Supported URL template tokens:
- `{emoji}`: the raw emoji grapheme itself
- `{codepoints_lower}`: hyphen-separated lowercase codepoints
- `{codepoints_upper}`: hyphen-separated uppercase codepoints
- `{codepoints_lower_no_vs}`: lowercase codepoints with `FE0F` removed
- `{codepoints_upper_no_vs}`: uppercase codepoints with `FE0F` removed

Example template:
`https://emoji-cdn.mqrio.dev/{emoji}?style=google`

Recommended ordering:
- put grapheme-aware providers first
- put codepoint-based providers after them
- keep the broadest and most reliable source at the top of the list

### Writing Markdown

#### Headers with Anchor IDs
```markdown
## My Section {#my-section}

[Jump to section](#my-section)
```

#### GitHub-style Admonitions
```markdown
> [!NOTE]
> This is a note

> [!TIP]
> Here's a helpful tip

> [!WARNING]
> Be careful!
```

#### Code Blocks with Syntax Highlighting
````markdown
```csharp
public void MyMethod()
{
    Debug.Log("Highlighted!");
}
```
````

#### Tables
```markdown
| Header 1 | Header 2 |
|----------|----------|
| Cell 1   | Cell 2   |
| Cell 3   | Cell 4   |
```

#### Images (supports GIF)
```markdown
![Alt text](relative/path/to/image.png)
```

#### Emoji
```markdown
# Release Notes 🎉

Everything is green ✅
```

Unity 6 renders emoji through the editor's native text system. Unity versions older than Unity 6 use the configured emoji source chain to resolve inline color emoji images in the formatted markdown view.

## Architecture

```
Assets/AB/Unity-Markdown-Viewer/
├── Scripts/
│   └── Editor/
│       ├── MarkdownEditor.cs          # Custom Inspector for .md files
│       ├── MarkdownViewer.cs          # Core rendering engine
│       ├── MarkdownTheme.cs           # Theme system + presets
│       ├── MarkdownThemeEditor.cs     # Visual theme editor
│       ├── MarkdownPreferences.cs     # User preferences & configuration
│       ├── Layout/                    # Layout engine and positioning
│       └── Renderer/                  # Markdown element renderers
├── Theme/                             # Theme asset and GUI skins
├── Sample/                            # Example markdown files
└── README.md                          # Documentation
```

## Compatibility

| Feature | Status |
|---------|--------|
| **Unity 6** | ✅ Full support (UIElements + IMGUI) |
| **Unity 2022 LTS** | ✅ Tested |
| **Unity 2021 LTS** | ✅ Compatible |
| **Emoji Rendering** | ✅ Native on Unity 6, inline color image fallback on Unity 2021/2022 |
| **Windows** | ✅ Tested |
| **macOS** | ✅ Compatible |
| **Linux** | ✅ Compatible |

## Why Use This Package

- Keeps Markdown documentation inside the Unity Editor instead of forcing teammates to switch to an external app.
- Makes README files, changelogs, design notes, and internal guides searchable and reviewable in the same project workflow.
- Ships as a plain Unity plugin under `Assets/AB/Unity-Markdown-Viewer`, which keeps import paths predictable for teams and release automation.

## Performance

- **Fast rendering** – Lazy layout with caching
- **Responsive** – Optimized for large documents (100+ KB)
- **Low memory** – Efficient texture pooling for images
- **Smooth scrolling** – Native UIElements integration

## Customization

### Create Custom Themes
```csharp
// Create a new MarkdownTheme asset and customize in the editor
// Or programmatically:
var theme = ScriptableObject.CreateInstance<MarkdownTheme>();
theme.Light.Text = new Color(0.2f, 0.2f, 0.2f);
theme.Dark.Text = new Color(0.9f, 0.9f, 0.9f);
```

You can edit the active theme either by selecting the `MarkdownTheme` asset directly or from
**Project Settings > AB > Markdown Viewer**.

### Extend Syntax Highlighting
Edit `SyntaxHighlighter.cs` to add new language patterns or color schemes.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Markdown doesn't display | Ensure file extension is `.md` or `.markdown` |
| Images don't load | Check relative paths; absolute paths use project root |
| Emoji look missing or monochrome on Unity versions older than Unity 6 | Use the formatted markdown view and verify the `MarkdownTheme` emoji source chain foldout; pre-Unity-6 editors fetch emoji through the configured CDN sources and need network access on first load |
| Anchor links don't scroll | Ensure heading has proper ID (auto-generated or `{#id}`) |
| Theme colors wrong | Open **Project Settings > AB > Markdown Viewer** and verify the active `MarkdownTheme` asset |
| Text is tiny | Adjust zoom in Editor Preferences or theme font sizes |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on submitting issues, feature requests, and pull requests.

## Credits

- **Original Project:** [UnityMarkdownViewer](https://github.com/gwaredd/UnityMarkdownViewer) by gwaredd (MIT License)
- **Parser:** [Markdig](https://github.com/xoofx/markdig) – A fast, powerful .NET Markdown parser
- **Rewrite & Enhancements:** Ahmad Albahar

## License

MIT License – See [LICENSE.md](LICENSE.md) for details.

This is a complete rewrite of the original UnityMarkdownViewer project. It maintains the same MIT license to respect the original author's work while providing significant enhancements and modern features.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and updates.

## Support

- 🐛 **Found a bug?** [Open an issue](https://github.com/ahalbahar/Unity-Markdown-Viewer/issues)
- 💡 **Have an idea?** [Request a feature](https://github.com/ahalbahar/Unity-Markdown-Viewer/discussions)
- 🤝 **Want to help?** [Contribute](CONTRIBUTING.md)

---

**Happy documenting!** 📚✨
