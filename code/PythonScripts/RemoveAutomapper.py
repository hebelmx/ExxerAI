import os
import re
from pathlib import Path
import webbrowser


log_entries = []

# Configuration
TARGET_DIR = Path(r"D:\IndTrace\Dev\IndTraceV2025\Src")  # Replace with actual path
CS_EXTENSIONS = ['.cs']
CSPROJ_EXTENSIONS = ['.csproj']
AUTO_MAPPER_USING = 'using AutoMapper;'

# Log file
LOG_FILE = TARGET_DIR / "automapper_removal_log.txt"

# Track findings
automapper_usages = []
create_map_usages = []
imapper_injections = []

def log(msg):
    log_entries.append(msg)

def process_file(file_path):
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    modified = False

   # Remove 'using AutoMapper;' line
    new_content = re.sub(r'^\s*using\s+AutoMapper;\s*\n', '', content, flags=re.MULTILINE)
 
    if new_content != content:
        content = new_content
        log(f"Refined: Removed 'using AutoMapper;' in {file_path}")
        modified = True

    new_content = re.sub(r'^\s*@inject\s+AutoMapper;\s*\n', '', content, flags=re.MULTILINE)
    if new_content != content:
        content = new_content
        log(f"Refined: Removed '@using AutoMapper;' in {file_path}")
        modified = True

    new_content = re.sub(r'^\s*@using\s+AutoMapper;\s*\n', '', content, flags=re.MULTILINE)
    if new_content != content:
        content = new_content
        log(f"Refined: Removed '@using AutoMapper;' in {file_path}")

    # Remove IMapper parameters from constructor signatures
    content, count = re.subn(r'\bIMapper\s+\w+,\s*', '', content)
    if count > 0:
        log(f"Refined: Removed 'IMapper' constructor param in {file_path}")
        modified = True

    # Detect and log IMapper injection
    if re.search(r'\bIMapper\b', content):
        imapper_injections.append(str(file_path))

    # Detect and log CreateMap usage
    if re.search(r'\bCreateMap<', content):
        create_map_usages.append(str(file_path))

    # Detect and log AutoMapper keyword
    if 'AutoMapper' in content:
        automapper_usages.append(str(file_path))

    # Remove Profile-based class definitions
    profile_class_pattern = r'class\s+\w+\s*:\s*Profile\s*\{(?:[^{}]|\{[^{}]*\})*\}'
    new_content = re.sub(profile_class_pattern, '', content, flags=re.DOTALL)
    if new_content != content:
        content = new_content
        log(f"Removed Profile-based class in {file_path}")
        modified = True

    # Remove IMapFrom<T> interface definitions
    imapfrom_pattern = r'interface\s+IMapFrom<[^>]+>\s*\{[^}]*\}'
    new_content = re.sub(imapfrom_pattern, '', content, flags=re.DOTALL)
    if new_content != content:
        content = new_content
        log(f"Removed IMapFrom<T> interface from {file_path}")
        modified = True

    # Replace mapper.Map<T>(...) with TMapper.ToDto(...)
    def replace_mapper_call(match):
        return_type, param = match.groups()
        replacement = f'{return_type}Mapper.ToDto({param})'
        log(f"Replaced 'mapper.Map<{return_type}>({param})' with '{replacement}' in {file_path}")
        return replacement

    new_content = re.sub(r'\bmapper\.Map<(\w+)>\(([^)]+)\)', replace_mapper_call, content)
    if new_content != content:
        content = new_content
        modified = True

    # Save changes if any
    if modified:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)

def process_csproj(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    new_content = re.sub(r'\s*<PackageReference\s+Include="AutoMapper"[^>]+/>\s*', '', content)
    if new_content != content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        log(f"Removed AutoMapper PackageReference in {file_path}")

def walk_directory():
    for root, _, files in os.walk(TARGET_DIR):
        for file in files:
            path = Path(root) / file
            if path.suffix in CS_EXTENSIONS:
                process_file(path)
            elif path.suffix in CSPROJ_EXTENSIONS:
                process_csproj(path)

def write_log():
    LOG_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(LOG_FILE, 'w', encoding='utf-8') as logf:
        logf.write("=== AutoMapper Removal Log ===\n\n")
        logf.write("\n".join(log_entries))
        logf.write("\n\n=== AutoMapper Usages ===\n")
        logf.write("\n".join(automapper_usages))
        logf.write("\n\n=== CreateMap Usages ===\n")
        logf.write("\n".join(create_map_usages))
        logf.write("\n\n=== IMapper Injections ===\n")
        logf.write("\n".join(imapper_injections))

walk_directory()
write_log()
webbrowser.open(str(LOG_FILE.resolve()))
"✅ Enhanced script completed. Robust handling and deeper parsing applied."
