import os
import re
import xml.etree.ElementTree as ET
from collections import defaultdict


source_root = r"D:\IndTrace\Dev\IndTraceV2025\Src"
project_packages = defaultdict(set)
used_packages = set()

# Parse .csproj files and collect PackageReferences per project
for root, _, files in os.walk(source_root):
    for file in files:
        if file.endswith(".csproj"):
            project_path = os.path.join(root, file)
            try:
                tree = ET.parse(project_path)
                for elem in tree.iter():
                    if "PackageReference" in elem.tag and 'Include' in elem.attrib:
                        project_packages[project_path].add(elem.attrib['Include'])
            except:
                pass

# Build namespace hints
def extract_namespace_roots(pkg):
    parts = pkg.split('.')
    return ['.'.join(parts[:i+1]) for i in range(len(parts))] + [parts[-1]]

namespace_hints = {}
for pkgs in project_packages.values():
    for pkg in pkgs:
        if pkg not in namespace_hints:
            namespace_hints[pkg] = set(extract_namespace_roots(pkg))

# Scan .cs files for any references
for root, _, files in os.walk(source_root):
    for file in files:
        if file.endswith(".cs"):
            path = os.path.join(root, file)
            with open(path, encoding='utf-8', errors='ignore') as f:
                content = f.read()
                for pkg, hints in namespace_hints.items():
                    for hint in hints:
                        if re.search(r'\b' + re.escape(hint) + r'\b', content):
                            used_packages.add(pkg)
                            break

# Identify unused packages by project
output_path = os.path.join(source_root, "suspected_unused_packages.txt")
with open(output_path, "w", encoding="utf-8") as out_file:
    for project, pkgs in project_packages.items():
        unused = pkgs - used_packages
        if unused:
            out_file.write(f"Project: {project}\n")
            for pkg in sorted(unused):
                out_file.write(f"  - {pkg}\n")
            out_file.write("\n")

print(f"Suspected unused packages written to: {output_path}")

