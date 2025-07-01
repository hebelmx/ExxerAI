import subprocess

# Read the requirements.txt file
with open('requirements.txt', 'r') as file:
    packages = file.readlines()

# Trim whitespace and newlines from each package name
packages = [pkg.strip() for pkg in packages]

# Create a log file to store failed installations
with open('failed_installations.log', 'w') as log_file:
    for package in packages:
        try:
            print(f"Installing {package}...")
            subprocess.run(['pip', 'install', package], check=True)
            print(f"{package} installed successfully.")
        except subprocess.CalledProcessError as e:
            error_message = f"Failed to install {package}: {e}\n"
            print(error_message)
            log_file.write(error_message)

print("Installation process completed. Check failed_installations.log for any errors.")
