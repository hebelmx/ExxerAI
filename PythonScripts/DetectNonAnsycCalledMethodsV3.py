import os
import re

# === CONFIGURATION ===
BASE_DIRECTORY = r"F:\\Dynamic\\ExxerAi\\ExxerAI\\code\\src\\"  # Base directory to search
CONTEXT_REPORT_PATH = r"F:\\Dynamic\\ExxerAi\\ExxerAI\\PythonScripts\\Default_context_report.txt"
INCLUDE_FILES = [".cs", ".razor"]  # File extensions to include
EXCLUDE_DIRS = ["bin", "obj", ".git" "tests","Pages"]  # Directory names to exclude
DRY_RUN = True  # Set to False to perform renaming
EXCLUDED_METHODS = {"Main", "Login", "Register"}  # Methods to ignore for renaming

# NOTE:
# Excluded method names like Login, Register, Main are typically tied to user flows
# or Razor Page semantics, and may intentionally omit the Async suffix. These are
# treated as advisory naming exceptions for test code only—not production code.
# In production, all awaitable methods must be properly suffixed with Async and
# include cancellation support where appropriate. Public/internal async methods
# should declare CancellationToken with default = default.
# ======================

# Regex to match async method signatures not ending in 'Async'
method_pattern = re.compile(
    r'(?P<indent>\s*)(?P<ret>public|private|protected|internal)?\s*(?P<modifiers>static\s+|virtual\s+|async\s+)*'
    r'(?P<type>Task(?:<[^>]+>)?)\s+(?P<name>\w+)(?P<generics><[^>]+>)?\s*\((?P<params>[^)]*)\)',
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
            modified_lines = lines[:]
            changes = []

            for idx, line in enumerate(lines):
                class_match = class_pattern.search(line)
                if class_match:
                    current_class = class_match.group("name")

                match = method_pattern.search(line)
                if match:
                    method_name = match.group("name")
                    return_type = match.group("type")
                    params = match.group("params")
                    access_modifier = match.group("ret")

                    has_ctoken = re.search(r'CancellationToken\s+\w+(\s*=\s*default)?', params)

                    if method_name not in EXCLUDED_METHODS and not method_name.endswith("Async"):
                        new_name = method_name + "Async"
                        updated_line = line.replace(method_name, new_name, 1)
                        report.append(f"{full_path}:{idx+1} | Class: {current_class} | Rename: {method_name} -> {new_name}")
                        modified_lines[idx] = updated_line
                        changes.append((idx + 1, method_name, new_name))

                    if (access_modifier in ["public", "internal"] and
                        return_type.startswith("Task") and
                        (not has_ctoken or not re.search(r'CancellationToken\s+\w+\s*=\s*default', params))):
                        report.append(f"{full_path}:{idx+1} | Class: {current_class} | Incomplete CancellationToken (add '= default'): {method_name}")

            if changes and not DRY_RUN:
                with open(full_path, "w", encoding="utf-8") as f:
                    f.writelines(modified_lines)

with open(CONTEXT_REPORT_PATH, "w", encoding="utf-8") as rep:
    rep.write("\n".join(report))

print(f"Report generated: {CONTEXT_REPORT_PATH}")
