import os
import string
import sys


# === Default Working Directory and Output File ===
directory_to_search = r'E:\Dynamic\IndTrace\IndTraceV2025\Src'
output_file_path = r'E:\Dynamic\IndTrace\IndTraceV2025\Src\IndTrace.cs'

# --- Configuration ---
TARGET_EXTENSIONS = ['.cs','.razor.cs'] , ##'.razor', '.razor.cs', '.sql', '.csproj', '.sln']
SKIP_DIRS = ['\\bin', '\\obj', '\\.git']
VERBOSE = True
LOG_FILE = "diagnostic.log"
MAX_FILE_SIZE = int(1.8 * 1024 * 1024)  # 1.8 MB

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

def merge_files(file_paths, base_output_file, root_dir, split=True,
                header_directory="Directory Structure Overview",
                header_content="Aggregated Source Files",
                header_csproj="Project File (.csproj)",
                header_sln="Solution File (.sln)"):

    ascii_tree = generate_ascii_tree(root_dir)
    version = 1
    current_output = base_output_file
    bytes_written = 0

    def get_output_file(version):
        if split:
            base, ext = os.path.splitext(base_output_file)
            return f"{base}_v{version}{ext}"
        return base_output_file

    def open_new_file():
        nonlocal version, current_output, bytes_written
        current_output = get_output_file(version)
        version += 1
        bytes_written = 0
        print(f"📂 Creating output file: {current_output}")
        return open(current_output, 'w', encoding='utf-8')

    try:
        outfile = open_new_file()
        outfile.write(f"// === {header_directory} ===\n")
        outfile.write("// This file shows the scanned folder layout.\n")
        outfile.write("//\n")
        tree_comment = "// " + ascii_tree.replace("\n", "\n// ") + "\n\n"
        outfile.write(tree_comment)
        outfile.write(f"// === {header_content} ===\n\n")
        bytes_written += sum(len(line.encode('utf-8')) for line in [tree_comment])

        for i, file_path in enumerate(file_paths):
            sys.stdout.write(f"\r⏳ Processing {i + 1}/{len(file_paths)}: {file_path[-50:]}")
            sys.stdout.flush()

            content = ""
            try:
                with open(file_path, 'r', encoding='utf-8') as infile:
                    content = infile.read()
            except UnicodeDecodeError:
                try:
                    with open(file_path, 'r', encoding='windows-1252') as infile:
                        content = infile.read()
                except Exception as e:
                    log(f"ERROR reading {file_path} (even with fallback): {e}")
                    continue
            except Exception as e:
                log(f"ERROR reading {file_path}: {e}")
                continue

            if not is_mostly_printable(content):
                log(f"SKIPPED NON-PRINTABLE: {file_path}")
                continue

            header = f"// File Path: {file_path}\n"
            if file_path.endswith('.csproj'):
                header = f"// === {header_csproj} ===\n// Path: {file_path}\n"
            elif file_path.endswith('.sln'):
                header = f"// === {header_sln} ===\n// Path: {file_path}\n"

            block = header + content + "\n\n"
            block_bytes = len(block.encode('utf-8'))

            if split and bytes_written + block_bytes > MAX_FILE_SIZE:
                outfile.close()
                outfile = open_new_file()

            outfile.write(block)
            bytes_written += block_bytes

        outfile.close()
        print(f"\n✅ Processing complete. Final output: {current_output}")

    except Exception as e:
        log(f"Unexpected error during merging: {e}")


def main(directory_to_search=None, split=True, **headers):
    if directory_to_search is None:
        directory_to_search = os.getcwd()

    base_name = os.path.basename(os.path.normpath(directory_to_search))
    output_file_path = os.path.join(directory_to_search, f"{base_name}.cs")

    with open(LOG_FILE, 'w', encoding='utf-8') as logf:
        logf.write("=== DIAGNOSTIC LOG START ===\n\n")

    print(f"🔍 Scanning source files in: {directory_to_search}")
    collected_files = find_target_files(directory_to_search)

    print(f"\n📄 Found {len(collected_files)} source files. Logs saved to {LOG_FILE}")
    if collected_files:
        merge_files(collected_files, output_file_path, directory_to_search, split, **headers)
    else:
        print("⚠️ No source files found. Check the log for details.")

if __name__ == "__main__":
    main()
