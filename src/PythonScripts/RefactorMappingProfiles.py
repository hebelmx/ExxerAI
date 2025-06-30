# Re-import and re-initialize after code execution state reset
import os
import re
from pathlib import Path
from datetime import date



log_entries = []

# Configuration
TARGET_DIR = Path(r"D:\IndTrace\Dev\IndTraceV2025\Src")
CS_EXTENSIONS = ['.cs']
LOG_FILE = TARGET_DIR / "mapping_method_transform_log.txt"
TODAY = date.today().strftime("%d %b %Y").upper()

def log(msg):
    log_entries.append(msg)

def transform_mapping_method(file_path):
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    pattern = r'public void Mapping\(Profile profile\)\s*\{([\s\S]*?)\}'
    matches = re.finditer(pattern, content)

    inserts = []
    modified = False

    for match in matches:
        body = match.group(1)
        if 'CreateMap<' not in body:
            continue

        lines = body.splitlines()
        for line in lines:
            if 'CreateMap<' in line:
                type_match = re.search(r'CreateMap<([^,>]+),\s*([^>]+)>', line)
                if not type_match:
                    continue
                src_type, dest_type = type_match.group(1).strip(), type_match.group(2).strip()

                # Comment the original Mapping method
                full_method = f'public void Mapping(Profile profile){{{body}}}'
                commented = '\n'.join(f'//{line}' for line in full_method.splitlines())
                inserts.append(commented)

                # Generate ToDto method
                to_dto = f"""
public TDest ToDto<TDest>({src_type} src, TDest dest) where TDest : new()
{{
    //TODO finish this method
    // {TODAY}
    // CRITICAL
    if(src == null)
    {{
        throw new ArgumentNullException(nameof(src));
    }}

    if (dest == null)
    {{
        dest = new TDest();

        // Map properties from src.Command to dest as needed
        // Example: Assuming {dest_type} has properties to map
        // dest.SomeProperty = src.Command.SomeProperty;

        return dest;
    }}
    // Map properties from src.Command to dest as needed
    // Example: Assuming {dest_type} has properties to map
    // dest.SomeProperty = src.Command.SomeProperty;
    return dest;
}}
""".strip()

                # Generate ToEntity method
                to_entity = f"""
public {src_type} ToEntity<T>(T src, {src_type} dest)
{{
    //TODO finish this method
    // {TODAY}
    // CRITICAL
    if (src == null)
    {{
        throw new ArgumentNullException(nameof(src));
    }}

    if (dest == null)
    {{
        dest = new {src_type}();

        // Map properties from src.Command to dest as needed
        // Example: Assuming {src_type} has properties to map
        // dest.SomeProperty = src.Command.SomeProperty;

        return dest;
    }}
    // Map properties from src.Command to dest as needed
    // Example: Assuming {src_type} has properties to map
    // dest.SomeProperty = src.Command.SomeProperty;
    return dest;
}}
""".strip()

                inserts.append(to_dto)
                inserts.append(to_entity)
                modified = True
                log(f"Processed Mapping method for {src_type} → {dest_type} in {file_path}")

    if modified:
        new_content = re.sub(pattern, '\n\n'.join(inserts), content)
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)

def walk_and_transform():
    for root, _, files in os.walk(TARGET_DIR):
        for file in files:
            path = Path(root) / file
            if path.suffix in CS_EXTENSIONS:
                transform_mapping_method(path)

def write_log():
    LOG_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(LOG_FILE, 'w', encoding='utf-8') as logf:
        logf.write("=== Mapping Method Transformation Log ===\n\n")
        logf.write("\n".join(log_entries))

walk_and_transform()
write_log()
webbrowser.open(str(LOG_FILE.resolve()))
"🚀 Transformation completed: Mapping methods converted with placeholders. Log updated."
