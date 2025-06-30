import os
import string

# --- Configuration ---
TARGET_EXTENSIONS = [
    #'.vb',
   '.cs',  '.csproj', '.sln',
    #'.cs', '.razor', '.razor.cs',
   # '.cshtml', '.cshtml.cs',
    '.sql', 
    #'.json', '.xml',
   # '.csproj', '.sln',
   # '.html', '.css', '.js',
   # '.md', '.yml', '.yaml',
       
]
SKIP_DIRS = ['\\bin', '\\obj', '\\.git']
VERBOSE = True
LOG_FILE = "diagnostic.log"

def has_valid_extension(filename):
        return any(filename.endswith(ext) for ext in TARGET_EXTENSIONS)

def is_skipped_dir(path):
    return any(skip in path for skip in SKIP_DIRS)

def log(message):
    with open(LOG_FILE, 'a', encoding='utf-8') as logf:
        logf.write(message + '\n')

def is_mostly_printable(text, threshold=0.6):
    printable_chars = set(string.printable)
    ratio = sum(c in printable_chars for c in text) / max(len(text), 1)
    return ratio >= threshold

def find_target_files(directory):
    collected_files = []

    if not os.path.exists(directory):
        print(f"❌ ERROR: Directory does not exist: {directory}")
        log(f"ERROR: Directory not found -> {directory}")
        return []

    if not os.path.isdir(directory):
        print(f"❌ ERROR: Path is not a directory: {directory}")
        log(f"ERROR: Path is not a directory -> {directory}")
        return []

    log(f"Starting directory scan: {directory}\n")

    for root, _, files in os.walk(directory):
        if is_skipped_dir(root):
            log(f"SKIP DIR: {root}")
            continue

        log(f"DIR: {root}")
        for file in files:
            full_path = os.path.join(root, file)
            ext = os.path.splitext(file)[1]

            if has_valid_extension(file):
                log(f"  [MATCH] {file}")
                collected_files.append(full_path)
            else:
                log(f"  [SEEN]  {file} (ext: {ext})")

    return sorted(collected_files)

def generate_ascii_tree(directory):
    tree_lines = []

    def recurse(path, prefix=""):
        try:
            entries = sorted(os.listdir(path))
        except PermissionError:
            tree_lines.append(f"{prefix}└── <Permission Denied>")
            return

        for index, entry in enumerate(entries):
            full_path = os.path.join(path, entry)
            connector = "└── " if index == len(entries) - 1 else "├── "
            tree_lines.append(f"{prefix}{connector}{entry}")
            if os.path.isdir(full_path) and not is_skipped_dir(full_path):
                extension = "    " if index == len(entries) - 1 else "│   "
                recurse(full_path, prefix + extension)

    tree_lines.append(directory)
    recurse(directory)
    return "\n".join(tree_lines)

def merge_files(file_paths, output_file, root_dir):
    ascii_tree = generate_ascii_tree(root_dir)

    try:
        with open(output_file, 'w', encoding='utf-8') as outfile:
            outfile.write("// === Source Tree Overview ===\n")
            outfile.write("// This ASCII tree shows the folder structure scanned.\n")
            outfile.write("//\n")
            outfile.write("// " + ascii_tree.replace("\n", "\n// ") + "\n\n")
            outfile.write("// === Merged Source Files ===\n\n")

            for file_path in file_paths:
                content = ""
                try:
                    # First try UTF-8
                    with open(file_path, 'r', encoding='utf-8') as infile:
                        content = infile.read()
                except UnicodeDecodeError:
                    try:
                        # Fallback to Windows-1252
                        with open(file_path, 'r', encoding='windows-1252') as infile:
                            content = infile.read()
                    except Exception as e:
                        error_msg = f"ERROR reading {file_path} (even with fallback): {e}"
                        print(error_msg)
                        log(error_msg)
                        continue  # Skip to the next file

                except Exception as e:
                    error_msg = f"ERROR reading {file_path}: {e}"
                    print(error_msg)
                    log(error_msg)
                    continue  # Skip to next file on other exceptions

                # Check for binary-like or non-readable content
                if not is_mostly_printable(content):
                    log(f"SKIPPED NON-PRINTABLE: {file_path}")
                    continue

                # Add headers for specific file types
                file_comment = f"// File: {file_path}\n"
                if file_path.endswith('.csproj'):
                    file_comment = f"// === C# Project File ===\n// {file_path}\n"
                elif file_path.endswith('.sln'):
                    file_comment = f"// === Solution File ===\n// {file_path}\n"

                # Write to output file
                outfile.write(file_comment)
                outfile.write(content + "\n\n")
       
    except Exception as e:
        error_msg = f"ERROR reading {file_path}: {e}"
        print(error_msg)
        log(error_msg)

def main():
    directory_to_search = r'E:\Dynamic\ExxerAi\ExxerAI\src'
    output_file_path = r'E:\Dynamic\ExxerAi\ExxerAI.cs'

    # Reset log
    with open(LOG_FILE, 'w', encoding='utf-8') as logf:
        logf.write("=== DIAGNOSTIC LOG START ===\n\n")

    print(f"🔍 Scanning for source files under: {directory_to_search}")
    collected_files = find_target_files(directory_to_search)

    print(f"\n📄 Found {len(collected_files)} matching source files. Log written to {LOG_FILE}")
    if collected_files:
        merge_files(collected_files, output_file_path, directory_to_search)
        print(f"✅ Successfully merged files into: {output_file_path}")
    else:
        print("⚠️ No matching source files found. Check diagnostic.log for details.")

if __name__ == "__main__":
    main()
