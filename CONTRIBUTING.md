# Contributing to Unity Markdown Viewer

Thank you for your interest in contributing to the Unity Markdown Viewer! This document provides guidelines for contributing code, reporting issues, and submitting pull requests.

## Code of Conduct

We are committed to providing a welcoming and inspiring community. Please treat all members with respect and engage in constructive dialogue.

## How to Contribute

### Reporting Bugs

Before submitting a bug report, please:
1. Check the [Issues](https://github.com/ahalbahar/Unity-Markdown-Viewer/issues) page to see if the bug has already been reported
2. Include a clear title and description
3. Provide a minimal reproduction case
4. List your Unity version (we support 2021 LTS, 2022 LTS, and Unity 6)
5. Include relevant logs or error messages

### Suggesting Enhancements

Enhancement suggestions are welcome! When proposing a feature:
1. Use a clear and descriptive title
2. Provide a detailed description of the suggested enhancement
3. List some examples of how this enhancement would be used
4. Explain why this would be useful to most users
5. Link any related issues or pull requests

### Pull Requests

We actively welcome pull requests! Here's how to submit one:

1. **Fork the repository** and create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Follow the code style** (see below)

3. **Make atomic commits** with clear messages:
   ```bash
   git commit -m "Add support for custom heading anchors"
   ```

4. **Write or update tests** if applicable

5. **Test your changes** thoroughly:
   - Open the viewer with various markdown files
   - Verify theme presets work correctly
   - Verify the active theme is editable from **Project Settings > AB > Markdown Viewer**
   - Check anchor navigation and links
   - Test image loading and syntax highlighting
   - Test emoji rendering in formatted view, especially on Unity versions older than Unity 6 where emoji use the theme-configured CDN fallback chain

6. **Push to your fork** and submit a Pull Request:
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Fill in the PR template** with:
   - Description of changes
   - Motivation and context
   - Testing steps
   - Relevant screenshots or GIFs
   - Link any related issues

## Code Style

This project follows standard C# conventions:

### Naming Conventions
- **Classes, methods, properties**: `PascalCase`
- **Private fields, local variables**: `camelCase`
- **Constants**: `UPPER_CASE`
- **Interfaces**: `IPascalCase`

### Code Organization
```csharp
public class MyClass
{
    // Fields
    private string mPrivateField;
    public string PublicField;

    // Properties
    public string MyProperty { get; set; }

    // Constructors
    public MyClass() { }

    // Methods
    public void MyMethod() { }

    // Private methods
    private void PrivateMethod() { }
}
```

### Formatting Rules
- Use **spaces** (4 spaces per indent level), not tabs
- Use K&R style braces:
  ```csharp
  if (condition) {
      // code
  }
  ```
- Add comments for non-obvious logic
- Keep lines under 100 characters when possible
- Use `#region` / `#endregion` to organize large files

### XML Documentation
Public classes and methods should include XML documentation comments:

```csharp
/// <summary>
/// Renders markdown content in the Unity Inspector.
/// </summary>
/// <param name="skin">The GUISkin to use for rendering.</param>
/// <returns>The calculated height of the rendered content.</returns>
public float RenderMarkdown(GUISkin skin)
{
    // implementation
}
```

## Project Structure

```
Assets/AB/Unity-Markdown-Viewer/
├── Scripts/
│   └── Editor/                 # Editor-only implementation
│       ├── MarkdownEditor.cs   # Custom Inspector
│       ├── MarkdownViewer.cs   # Core rendering engine
│       ├── MarkdownTheme.cs    # Theme system
│       ├── MarkdownThemeEditor.cs
│       ├── MarkdownPreferences.cs
│       ├── Renderer/           # Element renderers
│       └── Layout/             # Layout engine
├── Theme/                      # Theme asset and GUI skins
└── Sample/                     # Example markdown files
```

## Key Files to Understand

- **MarkdownViewer.cs** — Main class for parsing and rendering. Contains the Markdig pipeline setup.
- **MarkdownTheme.cs** — Theme system with 10+ presets. Add new themes here.
- **Renderer/** — Handles different markdown element types (headings, code blocks, tables, etc.)
- **Layout/** — Layout engine that calculates positions and sizes for rendering

## Building and Testing

### Compile C# Code
### Test in Unity Editor
1. Open the project in Unity
2. Create or modify a `.md` file in your Assets folder
3. Select the markdown file in the Project window
4. View it in the Inspector to verify your changes

### Test Specific Features
- **Anchor navigation**: Click `[jump](#section-id)` links in markdown
- **Themes**: Open the theme asset or **Project Settings > AB > Markdown Viewer** and try each preset
- **Images**: Test relative and absolute image paths
- **Emoji**: Verify emoji render correctly in formatted view on both Unity 6 and older supported editor versions, and verify the theme editor foldout and custom emoji source templates still work
- **Code blocks**: Try different language syntax highlighting
- **Admonitions**: Render `> [!NOTE]`, `> [!WARNING]`, etc.

## Release Workflow

Tagged releases are packaged in GitHub Actions without Unity. The workflow stages the repository contents into `Assets/AB/Unity-Markdown-Viewer` and runs `natsuneko-laboratory/create-unitypackage@v3` to create the release `.unitypackage`.

The release package intentionally excludes repository-only content such as `.github/`, `tools/`, `PackageBuilds/`, `CODE_QUALITY_FINDINGS.md`, and `CONTRIBUTING.md`.

## Commit Message Guidelines

Write clear, descriptive commit messages:

- Use the present tense ("Add feature" not "Added feature")
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the subject line to 50 characters
- Reference issues and pull requests liberally after the first line
- Include a blank line between the summary and body

Example:
```
Add custom heading ID support with {#custom-id} syntax

This allows users to define custom IDs for headings that
anchor links can reference. IDs are specified using the
{#id-name} syntax at the end of a heading line.

Fixes #123
```

## License

By submitting a pull request, you agree that your contributions will be licensed under the MIT License.

## Questions?

Feel free to open an issue or discussion for any questions. We're here to help!

---

**Thank you for contributing!** 🙌
