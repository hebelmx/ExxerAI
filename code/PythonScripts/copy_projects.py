import os
import shutil
from pathlib import Path

# ==== CONFIGURATION ====
SRC_ROOT = r"F:\Dynamic\ExxerAi\ExxerAI\src"
DEST_ROOT = r"F:\Dynamic\ExxerAi\ExxerAI\code"
LOG_FILE = r"F:\Dynamic\ExxerAi\ExxerAI\copy_log.txt"

copied_items = []
ignored_slns = []

def log_copy(path):
    print(f"Copied: {path}")
    copied_items.append(f"Copied: {path}")

def log_ignore(path):
    print(f"Ignored: {path}")
    ignored_slns.append(f"Ignored: {path}")

# ==== Ensure Destination Exists ====
os.makedirs(DEST_ROOT, exist_ok=True)

# ==== Copy the Most Recently Modified .sln File ====
sln_files = list(Path(SRC_ROOT).glob("*.sln"))
if sln_files:
    sln_files.sort(key=lambda f: f.stat().st_mtime, reverse=True)
    most_recent_sln = sln_files[0]
    dest_file = Path(DEST_ROOT) / most_recent_sln.name
    if not os.path.exists(dest_file):
        shutil.copy2(most_recent_sln, dest_file)
        log_copy(dest_file)

    for sln in sln_files[1:]:
        log_ignore(sln)

# ==== Copy Directory.Packages.props ====
props_file = Path(SRC_ROOT) / "Directory.Packages.props"
if props_file.exists():
    dest_file = Path(DEST_ROOT) / props_file.name
    if not os.path.exists(dest_file) and not os.path.islink(props_file):
        shutil.copy2(props_file, dest_file)
        log_copy(dest_file)

# ==== Copy .cursor Folder ====
cursor_src = Path(SRC_ROOT) / ".cursor"
cursor_dest = Path(DEST_ROOT) / ".cursor"
if cursor_src.exists() and not os.path.islink(cursor_src):
    if not cursor_dest.exists():
        shutil.copytree(cursor_src, cursor_dest, symlinks=False, dirs_exist_ok=True)
        log_copy(cursor_dest)

# ==== Copy Projects ====
for root, dirs, files in os.walk(SRC_ROOT):
    if any(f.endswith(".csproj") for f in files):
        rel_path = os.path.relpath(root, SRC_ROOT)
        dest_dir = os.path.join(DEST_ROOT, rel_path)
        if not os.path.exists(dest_dir) and not os.path.islink(root):
            shutil.copytree(root, dest_dir, symlinks=False, dirs_exist_ok=True)
            log_copy(dest_dir)

# ==== Write Log to File ====
with open(LOG_FILE, "w") as log_file:
    log_file.write("=== Copied Items ===\n")
    log_file.write("\n".join(copied_items))
    log_file.write("\n\n=== Ignored .sln Files ===\n")
    log_file.write("\n".join(ignored_slns))
