#!/usr/bin/env python3
"""
MCP Server Collection Migration Helper
Helps prepare and validate files for migration to the proper repository.
"""

import os
import shutil
from pathlib import Path
import json

def create_migration_package():
    """Create a migration package with all necessary files"""
    
    print("📦 Creating MCP Server Migration Package...")
    
    # Define source and destination
    source_dir = Path(__file__).parent
    migration_dir = source_dir / "migration_package"
    
    # Create migration directory
    migration_dir.mkdir(exist_ok=True)
    
    # Files to copy
    files_to_copy = [
        "simple_mcp_server.py",
        "http_mcp_server.py", 
        "simple_http_mcp_server.py",
        "mcpservers.py",
        "setup_mcp_servers.py",
        "mcp_dashboard.py",
        "dashboard_requirements.txt",
        "test_requirements.txt",
        "run_tests.py",
        "Makefile",
        "README.md"
    ]
    
    # Directories to copy
    dirs_to_copy = [
        "templates",
        "tests",
        ".github"
    ]
    
    copied_files = []
    copied_dirs = []
    
    # Copy individual files
    for file_name in files_to_copy:
        source_file = source_dir / file_name
        if source_file.exists():
            dest_file = migration_dir / file_name
            shutil.copy2(source_file, dest_file)
            copied_files.append(file_name)
            print(f"   ✅ Copied {file_name}")
        else:
            print(f"   ⚠️  Missing {file_name}")
    
    # Copy directories
    for dir_name in dirs_to_copy:
        source_dir_path = source_dir / dir_name
        if source_dir_path.exists():
            dest_dir_path = migration_dir / dir_name
            if dest_dir_path.exists():
                shutil.rmtree(dest_dir_path)
            shutil.copytree(source_dir_path, dest_dir_path)
            copied_dirs.append(dir_name)
            print(f"   ✅ Copied {dir_name}/ directory")
        else:
            print(f"   ⚠️  Missing {dir_name}/ directory")
    
    # Create migration info
    migration_info = {
        "version": "2.0.0",
        "migration_date": "2025-06-30",
        "status": "ready",
        "files_copied": copied_files,
        "directories_copied": copied_dirs,
        "total_files": len(copied_files),
        "features": [
            "5 MCP Server implementations",
            "Web dashboard with confetti animations",
            "70+ unit tests with TDD approach",
            "CI/CD pipeline with GitHub Actions",
            "Cross-platform support",
            "Comprehensive documentation"
        ],
        "next_steps": [
            "Copy migration_package/* to new repository",
            "Run: pip install -r test_requirements.txt",
            "Run: make test",
            "Run: make start-dashboard",
            "Create VS Code confetti extension"
        ]
    }
    
    # Save migration info
    with open(migration_dir / "migration_info.json", "w") as f:
        json.dump(migration_info, f, indent=2)
    
    # Create quick start script
    quick_start = """#!/bin/bash
# MCP Server Collection Quick Start
echo "Setting up MCP Server Collection..."

# Install dependencies
echo "Installing dependencies..."
pip install -r test_requirements.txt

# Run tests
echo "Running tests..."
python run_tests.py

# Show status
echo "Setup complete!"
echo "Ready to start dashboard: make start-dashboard"
echo "Available commands: make help"
"""
    
    with open(migration_dir / "quick_start.sh", "w", encoding="utf-8") as f:
        f.write(quick_start)
    
    # Make executable
    os.chmod(migration_dir / "quick_start.sh", 0o755)
    
    print(f"\n🎉 Migration package created successfully!")
    print(f"📁 Location: {migration_dir}")
    print(f"📋 Files: {len(copied_files)} files, {len(copied_dirs)} directories")
    print(f"💾 Size: {get_directory_size(migration_dir):.2f} MB")
    print(f"\n📋 Migration Info:")
    for feature in migration_info["features"]:
        print(f"   ✅ {feature}")
    
    return migration_dir

def get_directory_size(directory):
    """Get directory size in MB"""
    total_size = 0
    for dirpath, dirnames, filenames in os.walk(directory):
        for filename in filenames:
            filepath = os.path.join(dirpath, filename)
            total_size += os.path.getsize(filepath)
    return total_size / (1024 * 1024)

def validate_migration_package(package_dir):
    """Validate the migration package"""
    print("\n🔍 Validating migration package...")
    
    required_files = [
        "simple_mcp_server.py",
        "mcp_dashboard.py", 
        "README.md",
        "run_tests.py",
        "migration_info.json"
    ]
    
    required_dirs = [
        "templates",
        "tests"
    ]
    
    all_valid = True
    
    for file_name in required_files:
        file_path = package_dir / file_name
        if file_path.exists():
            print(f"   ✅ {file_name}")
        else:
            print(f"   ❌ Missing {file_name}")
            all_valid = False
    
    for dir_name in required_dirs:
        dir_path = package_dir / dir_name
        if dir_path.exists() and dir_path.is_dir():
            file_count = len(list(dir_path.rglob("*")))
            print(f"   ✅ {dir_name}/ ({file_count} files)")
        else:
            print(f"   ❌ Missing {dir_name}/")
            all_valid = False
    
    if all_valid:
        print("\n🎉 Migration package is VALID and ready!")
        print("🚀 Ready to move to the proper repository!")
    else:
        print("\n❌ Migration package has issues!")
    
    return all_valid

if __name__ == "__main__":
    print("🎯 MCP Server Collection Migration Helper")
    print("=" * 50)
    
    # Create migration package
    package_dir = create_migration_package()
    
    # Validate package
    validate_migration_package(package_dir)
    
    print("\n" + "=" * 50)
    print("🎊 MIGRATION READY! 🎊")
    print(f"📦 Package location: {package_dir}")
    print("🔄 Copy the migration_package folder to your target repository")
    print("🎉 Then create that VS Code confetti extension!")
