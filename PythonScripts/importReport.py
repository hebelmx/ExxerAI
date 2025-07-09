import re
from pathlib import Path

# === CONFIGURATION ===
BUILD_REPORT_PATH = r"F:\Dynamic\ExxerAi\ExxerAI\BuildReport.txt"
OUTPUT_REPORT_PATH = r"xunit1051_context_report.txt"
LINES_BEFORE_AFTER = 2
# ======================


pattern = re.compile(r"^(?P<file>.*?\.cs)\((?P<line>\d+),\d+\): warning xUnit1051")
skipped, processed = 0, 0

def extract_context(file_path: Path, line_number: int, before: int, after: int):
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            lines = f.readlines()
        start = max(0, line_number - 1 - before)
        end = min(len(lines), line_number + after)
        snippet = "".join(lines[start:end])
        return snippet.strip()
    except Exception as e:
        return f"ERROR: Cannot read file '{file_path}': {str(e)}"
def extract_file_and_line(line: str):
    # Look for .cs file path
    file_match = re.search(r"(F:.*?\.cs)", line)
    line_match = re.search(r"\.cs\((\d+)", line)
    if file_match and line_match:
        return Path(file_match.group(1)), int(line_match.group(1))
    return None, None

def main():
    report_lines = []
    with open(BUILD_REPORT_PATH, "r", encoding="utf-8") as report:
        for line in report:
            if "xUnit1051" not in line:
                continue
            file_path, line_number = extract_file_and_line(line)
            if file_path and file_path.exists():
                context = extract_context(file_path, line_number, LINES_BEFORE_AFTER, LINES_BEFORE_AFTER)
                report_lines.append(f">>> {file_path} (Line {line_number})\n{context}\n{'-'*80}")
            else:
                report_lines.append(f"SKIPPED or NOT FOUND: {line.strip()}")

    with open(OUTPUT_REPORT_PATH, "w", encoding="utf-8") as out:
        out.write("\n".join(report_lines))

    print(f"✅ Saved report: {OUTPUT_REPORT_PATH}")


if __name__ == "__main__":
    main()
