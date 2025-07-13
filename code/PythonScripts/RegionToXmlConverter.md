# Region-to-XML Comment Converter for C# (.cs Files)

## Overview

This Python script scans C# source files and transforms `#region` / `#endregion` directives into XML documentation comments to improve code readability and compatibility with clean-code and documentation standards.

## Features

- **Dry Run Mode**: Preview proposed changes without modifying any files.
- **XML Comment Replacement**: Converts region markers to standardized XML comments.
- **Line Context**: Includes ±4 lines of context in reports for review.
- **File Safety**: Skips system directories (`bin`, `obj`, `.vs`, `.git`) and does not alter nested regions.
- **Preserves Compilation Integrity**: Ensures all changes are syntactically and semantically safe.

## Configuration

Edit the header of the script to configure:

```python
SOURCE_PATH = r"Path\\To\\Your\\Source"
REPORT_PATH = r"Path\\To\\Your\\Report.txt"
DRY_RUN = True  # Set to False to apply changes
```

- `SOURCE_PATH`: Root directory to recursively scan for `.cs` files.
- `REPORT_PATH`: Path to write a detailed report of changes.
- `DRY_RUN`: If `True`, only generates the report; if `False`, applies transformations.

## Report Format

Each proposed change is listed with:
- File name and line range
- `>>> BEFORE <<<` section showing the original region line
- `>>> AFTER <<<` section with the new XML comment

A summary at the end provides:
- Total regions converted
- Number of files affected

## Example Transformation

**Before:**
```csharp
#region Validate Extraction Tool Tests
```

**After:**
```csharp
/// <summary>
/// Begin Tests Validate Extraction Tool
/// </summary>
/// <returns></returns>
```

## Notes

- Only top-level regions are processed.
- Region name patterns with `"Test"` or `"Tool"` are parsed for XML semantics.
- Backup your files before applying transformations with `DRY_RUN = False`.

## License

MIT License — provided as-is without warranty.