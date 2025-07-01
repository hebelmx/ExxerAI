import os
import re
from pathlib import Path
import webbrowser

log_entries = []

# Configuration
TARGET_DIR = Path(r"D:\IndTrace\Dev\IndTraceV2025\Src")
CS_EXTENSIONS = ['.cs']
LOG_FILE = TARGET_DIR / "imapfrom_interface_removal_log.txt"

def log(msg):
    log_entries.append(msg)

def remove_imapfrom_inheritance(file_path):
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    modified = False

    # Pattern to remove ": IMapFrom<SomeType>" from class definitions
    new_content, count = re.subn(r'\s*:\s*IMapFrom<[^>]+>', '', content)
    if count > 0:
        modified = True
        log(f"Removed IMapFrom interface from {file_path}")

    if modified:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)

def walk_and_clean_imapfrom():
    for root, _, files in os.walk(TARGET_DIR):
        for file in files:
            path = Path(root) / file
            if path.suffix in CS_EXTENSIONS:
                remove_imapfrom_inheritance(path)

def write_log():
    LOG_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(LOG_FILE, 'w', encoding='utf-8') as logf:
        logf.write("=== IMapFrom Interface Removal Log ===\n\n")
        logf.write("\n".join(log_entries))

walk_and_clean_imapfrom()
write_log()
webbrowser.open(str(LOG_FILE.resolve()))

"🧹 IMapFrom interface cleanup completed. Residual AutoMapper traces removed."
