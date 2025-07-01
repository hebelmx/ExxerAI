import os
import re

# Hardcoded path to your EF Core configuration files
TARGET_DIRECTORY = r'D:\IndTrace\Dev\IndTraceV2025\Src\Infrastructure\IndTrace.Persistence\Configurations'

def update_column_names(file_path):
    with open(file_path, 'r', encoding='utf-8') as file:
        lines = file.readlines()

    # Try to extract the entity class name from the file (assumes naming convention)
    class_name_match = re.search(r'class\s+(\w+)Configuration\s*:\s*IEntityTypeConfiguration<(\w+)>', ''.join(lines))
    if not class_name_match:
        print(f"Skipped (class not found): {file_path}")
        return

    entity_name = class_name_match.group(2)
    updated_lines = []
    pattern = re.compile(r'\.HasColumnName\("(\w+)"\)')

    for line in lines:
        match = pattern.search(line)
        if match:
            prop = match.group(1)
            updated_line = pattern.sub(f'.HasColumnName(nameof({entity_name}.{prop}))', line)
            updated_lines.append(updated_line)
        else:
            updated_lines.append(line)

    with open(file_path, 'w', encoding='utf-8') as file:
        file.writelines(updated_lines)
    print(f"Updated: {file_path}")

def main():
    for root, _, files in os.walk(TARGET_DIRECTORY):
        for file in files:
            if file.endswith('.cs'):
                full_path = os.path.join(root, file)
                update_column_names(full_path)

if __name__ == "__main__":
    main()
