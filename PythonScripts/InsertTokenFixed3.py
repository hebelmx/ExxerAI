import re
from pathlib import Path

# === CONFIGURATION ===
CONTEXT_REPORT_PATH = r"F:\Dynamic\ExxerAi\ExxerAI\PythonScripts\xunit1051_context_report.txt"
BACKUP_SUFFIX = ".bak"  # Optional backup before editing
INJECTION_STRING = "cancellationToken: TestContext.Current.CancellationToken"
# ======================
INCLUDE_IF_CONTAINS = ["await"]
SKIP_IF_CONTAINS = [
    "ShouldBe", ".Returns", "Assert", "Task.WhenAll", "TestContext.Current.CancellationToken",
    "cancellationToken", "nameof(", "foreach", "=> await", "if (", "else if", "switch (", "mock" ,
    "AgentTask" , "throw new"
]

def parse_context_report(file_path: Path):
    with open(file_path, encoding="utf-8") as f:
        lines = f.read().split("\n")

    entries = []
    current_file = None
    for line in lines:
        if line.startswith(">>> "):
            current_file = re.sub(r"\s*\(Line \d+\)", "", line.replace(">>>", "").strip())
        elif re.match(r"^\s*\d+:\s", line):
            match = re.match(r"^\s*(\d+): (.*)", line)
            if match and current_file:
                line_number = int(match.group(1))
                code = match.group(2)
                entries.append((Path(current_file), line_number, code))
    return entries


def should_inject(original_line: str) -> bool:
    line = original_line.strip()
    return (
        any(kw in line for kw in INCLUDE_IF_CONTAINS)
        and not any(kw in line for kw in SKIP_IF_CONTAINS)
        and line.endswith(";")
    )

def should_inject(original_line: str) -> bool:
    line = original_line.strip()

    # Must contain "await"
    if not any(token in line for token in INCLUDE_IF_CONTAINS):
        return False

    # Skip problematic, misleading, or already-handled lines
    if any(skip in line for skip in SKIP_IF_CONTAINS):
        return False

    # Ensure this is a complete expression line
    if not line.endswith(";"):
        return False

    return True

def inject_token_call_dryrun(file_path: Path, line_number: int, line_content: str) -> bool:
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            lines = f.readlines()

        target_idx = line_number - 1
        original_line = lines[target_idx]
    
        if not should_inject(original_line):
            return False
        
        if INJECTION_STRING in original_line:
            return False  # Already injected
        
        
        if INJECTION_STRING in original_line:
            return False  # Already injected

        paren_index = original_line.rfind(")")
        if paren_index != -1:
            before = original_line[:paren_index].rstrip()
            after = original_line[paren_index:]
            if before.endswith("("):
                new_line = f"{before}{INJECTION_STRING}{after}"
            else:
                new_line = f"{before}, {INJECTION_STRING}{after}"
            print(f"\n🔎 File: {file_path} (Line {line_number})")
            print(f"OLD: {original_line.strip()}")
            print(f"NEW: {new_line.strip()}")
            return True
        return False
    except Exception as e:
        print(f"❌ Failed to read {file_path}:{line_number} — {e}")
        return False

def main():
    injected = 0
    entries = parse_context_report(Path(CONTEXT_REPORT_PATH))
    print(f"🔍 Dry-run: Processing {len(entries)} entries...")
    for file_path, line_num, code in entries:
        if inject_token_call_dryrun(file_path, line_num, code):
            injected += 1
    print(f"\nℹ️ Dry-run complete: {injected} modifications proposed.")
if __name__ == "__main__":
    main()
