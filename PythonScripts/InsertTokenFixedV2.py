import os
import re

# === CONFIGURATION ===
BASE_DIRECTORY = r"F:\Dynamic\ExxerAi\ExxerAI\code\src\tests"  # Set to your target directory
DRY_RUN = True
REPORT_FILE = r"token_final_refined_report.txt"
# ======================

# Regex to detect Async method calls (awaited or assigned)
pattern_add_token = re.compile(
    r"""(?P<full>
        (?P<prefix>\b(?:await|var\s+\w+\s*=\s*)?)             # await or assignment
        (?P<method>[\w.]+Async)                               # method ending with Async
        \s*\(
        (?P<args>(?:[^()]*\([^)]*\))*[^()]*)                 # multiline-safe args
        \)
        (?P<suffix>\s*;)
    )""",
    re.VERBOSE | re.MULTILINE
)

# Regex to fix malformed WaitAsync(, token)
pattern_fix_waitasync = re.compile(
    r'WaitAsync\(\s*,\s*TestContext\.Current\.CancellationToken\s*\)'
)

# Heuristic exclusions
EXCLUDED_CONTEXT_PATTERNS = [
    r'\bReturns\(', r'\bArg\.', r'_mock\.', r'Substitute\.For', r'\.Returns\b'
]

def context_is_mock_or_stub(code: str) -> bool:
    return any(re.search(p, code) for p in EXCLUDED_CONTEXT_PATTERNS)

def already_has_token(args: str) -> bool:
    return re.search(r'\.\s*(Token|CancellationToken)\b', args)

def has_named_cancellation_token(args: str) -> bool:
    return re.search(r'\bcancellationToken\s*:', args)

def has_named_cancellation(args: str) -> bool:
    return re.search(r'\bancellation\s*:', args)

def has_named_token(args: str) -> bool:
    return re.search(r'\btoken\s*:', args)

def needs_named_token(args: str, full_line: str) -> bool:
    return (
        not already_has_token(args)
        and not has_named_cancellation_token(args)
        and not has_named_token(args)
        and not has_named_cancellation(args)
        and not context_is_mock_or_stub(full_line)
        and args.strip() != ''
    )

def insert_named_token(match):
    prefix = match.group('prefix')
    method = match.group('method')
    args = match.group('args').rstrip()
    suffix = match.group('suffix')

    # Prevent trailing commas
    if args.endswith(','):
        return f"{prefix}{method}({args} cancellationToken: TestContext.Current.CancellationToken){suffix}"
    else:
        return f"{prefix}{method}({args}, cancellationToken: TestContext.Current.CancellationToken){suffix}"

def process_file(path, report_lines):
    with open(path, encoding='utf-8') as f:
        original = f.read()

    updated = original
    modified = False

    # === Injection Pass
    for m in reversed(list(pattern_add_token.finditer(updated))):
        if needs_named_token(m.group('args'), m.group('full')):
            replacement = insert_named_token(m)
            updated = updated[:m.start()] + replacement + updated[m.end():]
            report_lines.append(f"\n>> Named token inserted in: {path}")
            report_lines.append("Original:\n" + m.group(0))
            report_lines.append("Modified:\n" + replacement)
            report_lines.append("-" * 60)
            modified = True

    # === WaitAsync fix
    if pattern_fix_waitasync.search(updated):
        updated = pattern_fix_waitasync.sub('WaitAsync(TestContext.Current.CancellationToken)', updated)
        report_lines.append(f"\n>> WaitAsync fix in: {path}")
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
    print(f"✅ Refined report saved to: {REPORT_FILE}")

if __name__ == "__main__":
    main()
