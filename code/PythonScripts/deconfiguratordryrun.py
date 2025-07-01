import os
import re

# Hardcoded path to your target directory
TARGET_DIRECTORY = r'D:\IndTrace\Dev\IndTraceV2025\Src\Infrastructure\IndTrace.Persistence\Configurations'
OUTPUT_FILE = 'column_name_preview.txt'

def preview_column_name_updates(file_path, output_lines):
    with open(file_path, 'r', encoding='utf-8') as file:
        lines = file.readlines()

    class_name_match = re.search(r'class\s+(\w+)Configuration\s*:\s*IEntityTypeConfiguration<(\w+)>', ''.join(lines))
    if not class_name_match:
        return

    entity_name = class_name_match.group(2)
    pattern = re.compile(r'\.HasColumnName\("(\w+)"\)')

    for line_number, line in enumerate(lines, start=1):
        match = pattern.search(line)
        if match:
            prop = match.group(1)
            proposed = f'.HasColumnName(nameof({entity_name}.{prop}))'
            output_lines.append(f"{file_path}:{line_number}:\n  {line.strip()}  ➜  {proposed}\n")

def main():
    output_lines = []
    for root, _, files in os.walk(TARGET_DIRECTORY):
        for file in files:
            if file.endswith('.cs'):
                full_path = os.path.join(root, file)
                preview_column_name_updates(full_path, output_lines)

    with open(OUTPUT_FILE, 'w', encoding='utf-8') as out_file:
        out_file.writelines(output_lines)
    print(f"Preview saved to {OUTPUT_FILE}")

if __name__ == "__main__":
    main()
