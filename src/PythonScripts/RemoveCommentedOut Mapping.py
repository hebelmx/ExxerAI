# Script to remove commented-out Mapping(Profile) methods
import os
import re
from pathlib import Path
import webbrowser

log_entries = []

# Configuration
TARGET_DIR = Path(r"D:\IndTrace\Dev\IndTraceV2025\Src")
CS_EXTENSIONS = ['.cs']
LOG_FILE = TARGET_DIR / "mapping_method_comment_removal_log.txt"

def log(msg):
    log_entries.append(msg)

def remove_commented_mapping_methods(file_path):
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    modified = False

    # Match blocks of commented-out Mapping(Profile profile) methods
    pattern = r'(//\s*public void Mapping\(Profile profile\)\s*\{\n(?:\s*//.*\n)*?\s*\})'
    new_content, count = re.subn(pattern, '', content, flags=re.MULTILINE)

    if count > 0:
        modified = True
        log(f"Removed {count} commented Mapping method(s) in {file_path}")

    if modified:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)

def walk_and_clean_comments():
    for root, _, files in os.walk(TARGET_DIR):
        for file in files:
            path = Path(root) / file
            if path.suffix in CS_EXTENSIONS:
                remove_commented_mapping_methods(path)

def write_log():
    LOG_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(LOG_FILE, 'w', encoding='utf-8') as logf:
        logf.write("=== Mapping Method Comment Cleanup Log ===\n\n")
        logf.write("\n".join(log_entries))

walk_and_clean_comments()
write_log()
webbrowser.open(str(LOG_FILE.resolve()))

"🧽 Commented Mapping methods removed. Code cleaned and log saved."
