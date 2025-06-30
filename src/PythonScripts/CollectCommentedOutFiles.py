import os
import re

# Set your root path here (or leave as os.getcwd() to use the current directory)
ROOT_DIR = os.getcwd()
OUTPUT_FILE = "commented_cs_files.txt"

def is_fully_commented_out(file_path):
    inside_block_comment = False
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        for line in f:
            stripped = line.strip()
            if not stripped:
                continue  # Blank line

            if inside_block_comment:
                if '*/' in stripped:
                    inside_block_comment = False
                continue

            if stripped.startswith('//'):
                continue

            if '/*' in stripped:
                if '*/' not in stripped:
                    inside_block_comment = True
                continue

            # Not blank or a comment
            return False
    return True

def find_commented_cs_files(root_dir):
    commented_files = []
    for dirpath, _, filenames in os.walk(root_dir):
        for filename in filenames:
            if filename.endswith('.cs'):
                full_path = os.path.join(dirpath, filename)
                if is_fully_commented_out(full_path):
                    commented_files.append(full_path)
    return commented_files

if __name__ == '__main__':
    files = find_commented_cs_files(ROOT_DIR)
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as out:
        if files:
            out.write("Fully commented-out or blank-only .cs files:\n")
            for file in files:
                out.write(f"{file}\n")
            print(f"Found {len(files)} files. Results written to {OUTPUT_FILE}.")
        else:
            out.write("No such .cs files found.\n")
            print("No such .cs files found.")
