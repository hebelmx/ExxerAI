import re
import os

# === CONFIGURATION HEADER ===
BASE_DIRECTORY = r"F:\Dynamic\ExxerAi\ExxerAI\code\src\tests"  # <<< SET YOUR BASE DIRECTORY HERE
DRY_RUN = False  # Set to False to apply changes
# =============================
REPORT_FILE = r"dry_run_report.txt"  # <<< Path to your report file
REPORT_FILE = r"dry_run_report.txt"  # <<< Path to your report file
# =============================

pattern = re.compile(
    r"""(?P<full>
        await\s+                       # starts with await
        (?P<call>[\w.]+)               # e.g., _agentService.Method
        \(
        (?P<args>[^)]*                 # match multiline arguments
            (?:\n[^)]*)*
        )
        \)
        (?![^)]*(?i:cancellationtoken|ct|cts|token))  # exclude if token-related
        \s*;
    )""",
    re.VERBOSE | re.MULTILINE | re.IGNORECASE
)

def insert_token(match):
    call = match.group('call')
    args = match.group('args')
    return f"await {call}({args}, TestContext.Current.CancellationToken);"

def process_file(path, report_lines):
    with open(path, encoding='utf-8') as f:
        content = f.read()

    matches = list(pattern.finditer(content))
    if matches:
        report_lines.append(f"\n>> {len(matches)} match(es) in: {path}")
        for m in matches:
            original = m.group(0)
            modified = insert_token(m)
            report_lines.append("Original:\n" + original)
            report_lines.append("Modified:\n" + modified)
            report_lines.append("-" * 60)

        if not DRY_RUN:
            updated = pattern.sub(insert_token, content)
            with open(path, "w", encoding='utf-8') as f:
                f.write(updated)
            report_lines.append(">> File updated.")

report_output = []
for root, _, files in os.walk(BASE_DIRECTORY):
    for filename in files:
        if filename.lower().endswith(".cs"):
            process_file(os.path.join(root, filename), report_output)

with open(REPORT_FILE, "w", encoding="utf-8") as report:
    report.write("\n".join(report_output))

print(f"Report saved to: {REPORT_FILE}")
