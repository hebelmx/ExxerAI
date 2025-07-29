import os
import filecmp
import shutil

# Set source (original/corrupted) and destination (new cloned) directories
source_dir = r'F:\Dynamic\ExxerAi\ExxerAI'
dest_dir = r'F:\Dynamic\ExxerAIT\ExxerAI'

# Track copied files
copied_files = []

# Walk through source directory
for root, _, files in os.walk(source_dir):
    for file in files:
        if file.endswith('.cs') or file.endswith('.csproj'):
            rel_path = os.path.relpath(os.path.join(root, file), source_dir)
            source_file = os.path.join(source_dir, rel_path)
            dest_file = os.path.join(dest_dir, rel_path)

            if not os.path.exists(dest_file) or not filecmp.cmp(source_file, dest_file, shallow=False):
                os.makedirs(os.path.dirname(dest_file), exist_ok=True)
                shutil.copy2(source_file, dest_file)
                copied_files.append(rel_path)

# Report result
if copied_files:
    print("Copied the following updated or missing files:")
    for f in copied_files:
        print("  ", f)
else:
    print("All files are up to date. No files were copied.")
