import re
import os

# === CONFIGURATION HEADER ===
BASE_DIRECTORY = r"F:\Dynamic\ExxerAi\ExxerAI\code\src\tests"  # <<< SET YOUR BASE DIRECTORY HERE
DRY_RUN = True  # Set to False to apply changes
REPORT_FILE = r"fix_waitasync_report.txt"
# =============================

# Regex to match: await something.WaitAsync(, TestContext.Current.CancellationToken);
pattern = re.compile(
    r"""(?P<full>
        await\s+
        (?P<obj>\w+)\.WaitAsync\(
        \s*,\s*
        (?P<token>TestContext\.Current\.CancellationToken)
        \s*\);
    )""",
    re.VERBOSE
)

def fix_waitasync(match):
    obj = match.group('obj')
    token = match.group('token')
    return f"await {obj}.WaitAsync({token});"

def process_file(path, report_lines):
    with open(path, encoding='utf-8') as f:
        content = f.read()

    matches = list(pattern.finditer(content))
    if matches:
        report_lines.append(f"\n>> {len(matches)} WaitAsync fix(es) in: {path}")
        for m in matches:
            original = m.group(0)
            fixed = fix_waitasync(m)
            report_lines.append("Original:\n" + original)
            report_lines.append("Fixed:\n" + fixed)
            report_lines.append("-" * 60)

        if not DRY_RUN:
            updated = pattern.sub(fix_waitasync, content)
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

print(f"Fix report saved to: {REPORT_FILE}")
