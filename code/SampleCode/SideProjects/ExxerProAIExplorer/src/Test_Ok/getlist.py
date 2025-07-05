import subprocess

# Get the list of installed packages
result = subprocess.run(['pip', 'list', '--format=freeze'], stdout=subprocess.PIPE, text=True)

# Split the output into lines
packages = result.stdout.splitlines()

# Remove the version numbers
packages_no_versions = [pkg.split('==')[0] for pkg in packages]

# Write to requirements.txt
with open('requirements.txt', 'w') as f:
    for pkg in packages_no_versions:
        f.write(f"{pkg}\n")


#pip list --format=freeze | cut -d '=' -f 1 > requirements.txt


#pip install -r requirements.txt
