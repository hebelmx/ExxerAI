import subprocess
import os
import sys

def verify_git_repo():
    result = subprocess.run(["git", "rev-parse", "--is-inside-work-tree"], capture_output=True, text=True)
    if result.returncode != 0 or "true" not in result.stdout.strip():
        print("❌ Not inside a Git repository.")
        sys.exit(1)

def run_git_filter_repo():
    target_files = [
        "Samples/local-ai-packaged/neo4j/data/transactions/system/neostore.transaction.db.0",
        "Samples/local-ai-packaged/neo4j/data/transactions/neo4j/neostore.transaction.db.0"
    ]

    cmd = [
        "git", "filter-repo", "--invert-paths"
    ]

    for path in target_files:
        cmd.extend(["--path", path])

    print("⚙️ Running git-filter-repo...")
    result = subprocess.run(cmd, text=True)
    if result.returncode != 0:
        print("❌ git-filter-repo failed. Ensure it's installed and Python is working.")
        sys.exit(1)

def main():
    verify_git_repo()
    run_git_filter_repo()
    print("✅ Git history cleaned. You can now run:")
    print("   git push origin your-branch-name --force")

if __name__ == "__main__":
    main()
