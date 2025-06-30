import xml.etree.ElementTree as ET
from xml.dom import minidom
import shutil
import argparse
import os

def reorder_packages(file_path: str, dry_run: bool = False):
    # Backup original file
    backup_path = file_path + ".bak"
    if not dry_run:
        shutil.copyfile(file_path, backup_path)
        print(f"Backup created at: {backup_path}")

    # Load and parse the XML
    tree = ET.parse(file_path)
    root = tree.getroot()

    changed = False
    for item_group in root.findall(".//ItemGroup"):
        package_versions = item_group.findall("PackageVersion")
        if package_versions:
            sorted_versions = sorted(package_versions, key=lambda x: x.attrib.get("Include", "").lower())
            if [ET.tostring(p) for p in package_versions] != [ET.tostring(p) for p in sorted_versions]:
                changed = True
                if not dry_run:
                    for pkg in package_versions:
                        item_group.remove(pkg)
                    for pkg in sorted_versions:
                        item_group.append(pkg)

    # Pretty print
    xml_str = minidom.parseString(ET.tostring(root)).toprettyxml(indent="  ")

    if dry_run:
        print("Dry Run Output:\n")
        print(xml_str)
    elif changed:
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(xml_str)
        print(f"Updated file saved: {file_path}")
    else:
        print("No changes needed; packages are already sorted.")

if __name__ == "__main__":
    reorder_packages(
        file_path=r"D:\IndTrace\Dev\IndTraceV2025\Src\Directory.Packages.props",
        dry_run=False  # Set to True if you just want to preview changes
    )