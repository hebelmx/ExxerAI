import os
import re

# === CONFIGURATION ===
BASE_DIRECTORY = r"F:\\Dynamic\\ExxerAi\\ExxerAI\\code\\src"  # Update as needed
CONTEXT_REPORT_PATH = r"F:\\Dynamic\\ExxerAi\\ExxerAI\\PythonScripts\\async_context_report.txt"
# ======================

# Regex to match async method signatures not ending in 'Async'
method_pattern = re.compile(
    r'(?P<ret>public|private|protected|internal)?\s*(?P<modifiers>static\s+|virtual\s+|async\s+)*'
    r'(?P<type>Task(?:<[^>]+>)?)\s+(?P<name>\w+)(?P<generics><[^>]+>)?\s*\(',
    re.IGNORECASE
)

class_pattern = re.compile(r'\b(class|struct)\s+(?P<name>\w+)', re.IGNORECASE)

report = []

for root, _, files in os.walk(BASE_DIRECTORY):
    for file in files:
        if file.endswith(".cs") or file.endswith(".razor"):
            full_path = os.path.join(root, file)
            with open(full_path, "r", encoding="utf-8", errors="ignore") as f:
                lines = f.readlines()

            current_class = None
            for idx, line in enumerate(lines):
                class_match = class_pattern.search(line)
                if class_match:
                    current_class = class_match.group("name")

                method_match = method_pattern.search(line)
                if method_match:
                    method_name = method_match.group("name")
                    return_type = method_match.group("type")

                    if not method_name.endswith("Async"):
                        report.append(f"{full_path}:{idx+1} | Class: {current_class} | Method: {method_name} | Return: {return_type}")

with open(CONTEXT_REPORT_PATH, "w", encoding="utf-8") as rep:
    rep.write("\n".join(report))

print(f"Report generated: {CONTEXT_REPORT_PATH}")
