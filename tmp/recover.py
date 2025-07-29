import os
import filecmp

# Set your directories
source_dir = r'F:\Dynamic\ExxerAIT\ExxerAI'
corrupted_dir = r'F:\Dynamic\ExxerAi\ExxerAI'

# Collect potential updates
updated_files = []

for root, dirs, files in os.walk(source_dir):
    for file in files:
        if file.endswith('.cs') or file.endswith('.csproj'):
            rel_path = os.path.relpath(os.path.join(root, file), source_dir)
            source_file = os.path.join(source_dir, rel_path)
            corrupted_file = os.path.join(corrupted_dir, rel_path)

            if not os.path.exists(corrupted_file) or not filecmp.cmp(source_file, corrupted_file, shallow=False):
                updated_files.append(rel_path)

# Print dry-run result
if updated_files:
    print("Dry Run: The following files would be copied:")
    for file in updated_files:
        print("  ", file)
else:
    print("Dry Run: No updated .cs or .csproj files found.")
