import os
import re

# === CONFIGURATION ===
BASE_DIRECTORY = r"F:\Dynamic\ExxerAi\ExxerAI\code\src\tests"
DRY_RUN = True  # False to apply changes
REPORT_FILE = r"final_token_fix_report.txt"
# ======================

def needs_token(args: str) -> bool:
    return not re.search(r'\.\s*(Token|CancellationToken)\b', args)

# Insert TestContext.Current.CancellationToken if missing
pattern_add_token = re.compile(
    r"""(?P<full>
        (?P<prefix>\b(?:await|var\s+\w+\s*=\s*)?)       # await or assignment
        (?P<method>[\w.]+Async)                         # method ending in Async
        \s*\(
        (?P<args>(?:[^()]*\([^)]*\))*[^()]*)           # multiline arguments
        \)
        (?P<suffix>\s*;)
    )""",
    re.VERBOSE | re.MULTILINE
)

# Fix malformed WaitAsync(, token)
pattern_fix_waitasync = re.compile(
    r'WaitAsync\(\s*,\s*TestContext\.Current\.CancellationToken\s*\)'
)

# Wrap test-specific manual token usage
pattern_custom_token_use = re.compile(
    r'(?P<indent>\s*)(?P<call>await\s+[\w.]+\([^)]*\b(CancellationTokenSource|cts\.Token|new\s+CancellationToken)\b[^)]*\);)',
    re.IGNORECASE
)

def insert_token(match):
    prefix = match.group('prefix')
    method = match.group('method')
    args = match.group('args').rstrip()
    suffix = match.group('suffix')
    return f"{prefix}{method}({args}, TestContext.Current.CancellationToken){suffix}"

def wrap_with_pragma(match):
    indent = match.group('indent')
    call = match.group('call')
    return f"{indent}#pragma warning disable xUnit1051\n{indent}{call}\n{indent}#pragma warning restore xUnit1051"

def process_file(path, report_lines):
    with open(path, encoding='utf-8') as f:
        original = f.read()

    updated = original
    modified = False

    # === Token injection
    for m in reversed(list(pattern_add_token.finditer(updated))):
        if needs_token(m.group('args')):
            replacement = insert_token(m)
            updated = updated[:m.start()] + replacement + updated[m.end():]
            report_lines.append(f"\n>> Token inserted in: {path}")
            report_lines.append("Original:\n" + m.group(0))
            report_lines.append("Modified:\n" + replacement)
            report_lines.append("-" * 60)
            modified = True

    # === WaitAsync fix
    if pattern_fix_waitasync.search(updated):
        updated = pattern_fix_waitasync.sub('WaitAsync(TestContext.Current.CancellationToken)', updated)
        report_lines.append(f"\n>> WaitAsync fixed in: {path}")
        modified = True

    # === Wrap custom token uses
    for m in reversed(list(pattern_custom_token_use.finditer(updated))):
        replacement = wrap_with_pragma(m)
        updated = updated[:m.start()] + replacement + updated[m.end():]
        report_lines.append(f"\n>> Wrapped custom token use in: {path}")
        report_lines.append("Original:\n" + m.group(0))
        report_lines.append("Wrapped with pragma:\n" + replacement)
        report_lines.append("-" * 60)
        modified = True

    if modified and not DRY_RUN:
        with open(path, "w", encoding='utf-8') as f:
            f.write(updated)

def main():
    report_output = []
    for root, _, files in os.walk(BASE_DIRECTORY):
        for file in files:
            if file.endswith('.cs'):
                process_file(os.path.join(root, file), report_output)

    with open(REPORT_FILE, "w", encoding="utf-8") as report:
        report.write("\n".join(report_output))
    print(f"✅ Report saved to: {REPORT_FILE}")

if __name__ == "__main__":
    main()
