import os
import re

# === CONFIGURATION ===
BASE_DIRECTORY = r"F:\\Dynamic\\ExxerAi\\ExxerAI\\code\\src\\tests"
CONTEXT_REPORT_PATH = r"F:\\Dynamic\\ExxerAi\\ExxerAI\\PythonScripts\\xunit1051_context_report.txt"
INCLUDE_FILES = [".cs", ".razor"]
EXCLUDE_DIRS = ["bin", "obj", ".git"]
DRY_RUN = True
EXCLUDED_METHODS = {"Main", "Login", "Register"}

# NOTE:
# Excluded method names like Login, Register, Main are typically tied to user flows
# or Razor Page semantics, and may intentionally omit the Async suffix. These are
# treated as advisory naming exceptions for test code only—not production code.
# In production, all awaitable methods must be properly suffixed with Async and
# include cancellation support where appropriate. Public/internal async methods
# should declare CancellationToken with default = default.
# ======================

method_start_pattern = re.compile(
    r'(?P<access>public|internal)\s+(?P<modifiers>.*?)\s+Task(?:<[^>]+>)?\s+(?P<name>\w+)\s*\((?P<params>.*)',
    re.IGNORECASE
)

class_pattern = re.compile(r'\b(class|struct)\s+(?P<name>\w+)', re.IGNORECASE)

report = []

for root, dirs, files in os.walk(BASE_DIRECTORY):
    dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]

    for file in files:
        if any(file.endswith(ext) for ext in INCLUDE_FILES):
            full_path = os.path.join(root, file)
            with open(full_path, "r", encoding="utf-8", errors="ignore") as f:
                lines = f.readlines()

            current_class = None
            i = 0
            while i < len(lines):
                line = lines[i]
                class_match = class_pattern.search(line)
                if class_match:
                    current_class = class_match.group("name")

                match = method_start_pattern.search(line)
                if match:
                    method_name = match.group("name")
                    if method_name in EXCLUDED_METHODS or method_name.endswith("Async"):
                        i += 1
                        continue

                    method_lines = [line.rstrip("\n")]
                    while not lines[i].strip().endswith(")") and i + 1 < len(lines):
                        i += 1
                        method_lines.append(lines[i].rstrip("\n"))

                    if any("CancellationToken" in l for l in method_lines):
                        joined = " ".join(method_lines)
                        if not re.search(r'CancellationToken\s+\w+\s*=\s*default', joined):
                            report.append(f"{full_path}:{i+1} | Class: {current_class} | Incomplete CancellationToken default in: {method_name}")
                            snippet = method_lines[:4] if len(method_lines) > 4 else method_lines
                            report.extend([f"    >> {s}" for s in snippet])

                i += 1

with open(CONTEXT_REPORT_PATH, "w", encoding="utf-8") as rep:
    rep.write("\n".join(report))

print(f"Report generated: {CONTEXT_REPORT_PATH}")
