# Changelog

All notable changes to Unity Markdown Viewer will be documented in this file.

## [1.1.0] — 2026-04-19

### Added

- **Mermaid diagram rendering** – Fenced `mermaid` code blocks now render as inline diagrams in the Inspector through a dedicated diagram block instead of plain inline images
- **Diagram preview window** – Mermaid charts now expose an `Expand` action that opens a standalone preview window with zoom controls and drag panning
- **Mermaid disk caching** – Successful Mermaid diagram renders are cached on disk by default and can be stored under the Unity project `Library` or `Temp` folder
- **Mermaid regression coverage** – `Sample/sample.md` now includes flowchart, sequence, state, and gantt diagrams to verify rendering in formatted view

### Changed

- **Mermaid service fallback** – Mermaid render requests now retry Mermaid Ink and automatically fall back to Kroki before giving up
- **Theme-aware diagram colors** – Mermaid render requests now derive colors from the active `MarkdownTheme` so charts maintain readable contrast in light and dark themes
- **Wide-diagram handling** – Mermaid charts now scroll horizontally inside the Inspector when they exceed the available width
- **Documentation refresh** – README, sample documentation, contributing guidance, and package metadata now document Mermaid support, preview expansion, zoom/pan, disk caching, theme-aware contrast, and fallback behavior

## [1.0.0] — 2026-04-11

### Initial Release

This is a **complete rewrite** of the original [UnityMarkdownViewer](https://github.com/gwaredd/UnityMarkdownViewer) project with nearly 100% new code, built from the ground up with modern features and professional rendering.

### Added

#### Core Features
- **Full Markdown Support** – Complete support for Headers (H1–H6), emphasis (bold, italic, strikethrough), lists, code blocks, blockquotes, tables, and more
- **10+ Professional Theme Presets** – VS Code Dark+, GitHub Light, GitHub Dark, Dracula Dark, Nord Dark, OneDark Dark, Monokai Dark, Solarized Dark, Solarized Light, and Catppuccin Mocha Dark
- **Interactive Anchor Navigation** – Click heading anchor links (e.g., `[jump](#section-id)`) to scroll directly to sections with smooth navigation
- **Image Rendering** – Inline image support with automatic caching and web URL support
- **GitHub-style Admonitions** – Full support for `> [!NOTE]`, `> [!TIP]`, `> [!WARNING]`, `> [!IMPORTANT]`, `> [!CAUTION]` with custom styling
- **Custom Heading IDs** – Define custom anchor IDs with `{#custom-id}` syntax or auto-generated IDs from heading text

#### Syntax Highlighting
- **Multi-language support** – C#, JavaScript, Python, Java, Go, Rust, CSS, HTML, YAML, and more
- **Smart color coding** – Keywords, types, strings, comments, numbers, methods, all properly highlighted
- **Theme-aware** – All syntax colors adapt dynamically to the selected theme preset

#### Editor Features
- **Live theme preview** – Side-by-side light/dark skin comparison in the theme editor
- **One-click preset application** – Instantly switch between 10 beautiful themes with preset buttons
- **Project Settings theme access** – Configure the active theme from `Project/AB/Markdown Viewer` in addition to editing the theme asset directly
- **Unity Package Manager support** – Git-based installation through package name `com.ab.unitymarkdownviewer`
- **Cross-version emoji rendering** – Unity 6 uses native text emoji rendering, while Unity versions older than Unity 6 render emoji through a configurable CDN source chain with a clearer theme-editor configuration UI
- **Raw text mode** – Toggle between formatted markdown and raw text view for debugging
- **Navigation history** – Back/forward buttons to browse previously opened markdown files
- **Responsive layout** – Automatically adapts to Inspector width changes
- **Dark mode support** – Seamless integration with Unity 6 Editor UI and light/dark themes

#### Advanced Capabilities
- **HTML entity handling** – Proper rendering of `<`, `>`, `&` and other special characters
- **Smart text wrapping** – Intelligent line breaking for long content and code blocks
- **Code block syntax** – Specify language for proper syntax highlighting (e.g., ` ```csharp `)
- **Table rendering** – Full support for GitHub-style Markdown tables with alignment
- **Escape sequences** – Raw text mode toggles between markdown and plaintext view

### Comparison with Original Project

| Feature | Original | v1.0.0 |
|---------|----------|--------|
| **Anchor Navigation** | ❌ | ✅ Full scrolling support |
| **GitHub Admonitions** | ❌ | ✅ `[!NOTE]`, `[!TIP]`, `[!WARNING]`, `[!IMPORTANT]`, `[!CAUTION]` |
| **Syntax Highlighting** | ❌ | ✅ 9+ languages with theme-aware colors |
| **Theme Presets** | 1 (Dark) | ✅ 10 professional presets |
| **Image Support** | ✅ Basic | ✅ Enhanced with caching and web URLs |
| **HTML Entities** | Partial | ✅ Full support |
| **Theme Editor UI** | ❌ | ✅ Visual side-by-side color editor |
| **Markdig Parser** | ✅ | ✅ Enhanced integration |
| **Custom Heading IDs** | ❌ | ✅ Auto-generated and custom `{#id}` syntax |

### Architecture Improvements

- **Modular design** – Clear separation between parser, renderer, layout, image handling, and navigation
- **Comprehensive theming system** – Centralized `MarkdownTheme` with 10 presets and full customization
- **Robust rendering pipeline** – Markdig-based parsing with custom renderers for all element types
- **Performance optimized** – Lazy layout with caching and efficient texture pooling
- **Editor integration** – Custom Inspector editor with UIElements for modern UI

### Documentation

- **README.md** – Complete feature overview, installation, usage, and customization guide
- **CONTRIBUTING.md** – Guidelines for reporting issues and submitting pull requests
- **CHANGELOG.md** – This file, documenting all changes and releases

### Packaging

- **Release workflow** – GitHub Actions stages the repository into `Assets/AB/Unity-Markdown-Viewer` and uses `natsuneko-laboratory/create-unitypackage@v3` to build the `.unitypackage`
- **Node 24 ready workflow** – The release action now opts JavaScript actions into Node 24 ahead of the GitHub runner deprecation window
- **Manual package rebuilds** – The same workflow supports `workflow_dispatch`, so package artifacts can be regenerated without pushing a new tag
- **Curated package contents** – The release package excludes repository-only folders such as `.github`, `tools`, `PackageBuilds`, `CODE_QUALITY_FINDINGS.md`, and `CONTRIBUTING.md`
- **Package Manager theme fallback** – Default theme asset creation supports Package Manager installs by creating the writable theme asset under `Assets/AB/Unity-Markdown-Viewer/Theme` when the package itself lives under `Packages/`

### Credits

- **Original Project** – [UnityMarkdownViewer](https://github.com/gwaredd/UnityMarkdownViewer) by gwaredd (MIT License)
- **Markdown Parser** – [Markdig](https://github.com/xoofx/markdig) – A powerful .NET Markdown parser
- **Rewrite & Enhancements** – Ahmad Albahar

### License

MIT License – See LICENSE.md for full details

---

## Versioning

This project follows [Semantic Versioning](https://semver.org/):
- **MAJOR.MINOR.PATCH** (e.g., 1.0.0)
- MAJOR – Incompatible API changes
- MINOR – New functionality (backward compatible)
- PATCH – Bug fixes (backward compatible)

## Future Roadmap

Potential features for future releases:

- [ ] PlantUML rendering
- [ ] Full-text search with highlighting

---

**For more information, visit:** [GitHub Repository](https://github.com/ahalbahar/Unity-Markdown-Viewer)
