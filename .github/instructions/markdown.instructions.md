---
applyTo: "**/*.md"
---
# Markdown Formatting

## Basics

- Headings use `#` through `####` without numeric prefixes.
- Ordered lists **always** start with `1.`.
- Unordered lists use `-`.
- Keep line length ≤100 characters (MD013).
- Ensure a blank line before each heading (MD022).
- Headings should be surrounded by blank lines (MD022).
- No duplicate headings with the same content in the same document (MD024).
- First line of the file should be a top-level heading (MD041).
- No trailing spaces at the end of lines (MD009).
- Files should end with a single newline character (MD047).
- Heading levels should only increment by one level at a time (MD001).
- Headings must start at the beginning of the line (MD023).
- No trailing punctuation in headings (MD026).

## Headings

- Use ATX-style headings with no closing hashes (`#` at start of line only) (MD003).
- Ensure consistent heading styles throughout documents.
- No spaces after opening hash and before heading text (`# Heading`, not `#Heading`) (MD018).
- Don't use multiple spaces after the hash (`# Heading`, not `#  Heading`) (MD019).
- Don't use emphasis (bold, italic) instead of appropriate heading levels (MD036).

## Code Blocks

- Fence with triple backticks and a language tag. Indent nested blocks by four spaces to avoid premature close.
- Fenced code blocks should be surrounded by blank lines (MD031).
- Always specify a language for fenced code blocks (MD040).
- Use consistent code fence style (MD048) - prefer backticks (```) over tildes (~~~).

Example:

```markdown
    ```csharp
    // demo
    ```
```

## Inline Code

- Use backticks for short identifiers like `Result<T>`.
- Don't put spaces inside code span elements (like `this`, not ` this `) (MD038).
- Don't put spaces inside emphasis markers (like *emphasis*, not *␣emphasis␣*) (MD037).

## Links and URLs

- Don't use bare URLs - wrap them in angle brackets or use link syntax (MD034).
- No empty links with no destinations (MD042).
- Link text should be descriptive rather than generic phrases like "click here" (MD059).
- Ensure link fragments are valid (MD051).
- Don't use reversed link syntax like `(text)[url]` instead of `[text](url)` (MD011).

## Lists

- Lists should be surrounded by blank lines (MD032).
- Consistent indentation for list items at the same level (MD005).
- Use one space after list markers (MD030).
- Unordered list indentation should be consistent (MD007).
- No multiple consecutive blank lines (MD012).

## Tables

- Avoid unless aligning numeric data.
- First row is header, separated by dashes.
- Tables should be surrounded by blank lines (MD058).
- Maintain consistent column counts in tables (MD056).
- Use consistent pipe style for table borders (MD055).

## Whitespace and Formatting

- Don't use hard tabs, use spaces instead (MD010).
- No trailing spaces at the end of lines (MD009).
- Avoid multiple consecutive blank lines (MD012).
- Use consistent emphasis style (* or _) for italic (MD049).
- Use consistent emphasis style (** or __) for bold (MD050).

## Emojis

Allowed sparingly for emphasis in templates (e.g., PR checklist).

## HTML

Avoid raw HTML in Markdown when Markdown syntax can be used instead (MD033).
