# 🛠️ Project Recovery & Restore Instructions (.NET 9.0.5)

This guide standardizes cleanup, restore, build, and test procedures across all projects using the `.NET 9.0.5` SDK.

---

## 📌 SDK Pinning

Ensure a `global.json` exists at the solution root:

```json
{
  "sdk": {
    "version": "9.0.5",
    "rollForward": "disable"
  }
}
```

This enforces SDK compatibility and prevents fallback to .NET 10 previews.

---

## 📦 Full NuGet Cleanup & Restore

### Run the following from PowerShell in the solution root:

```powershell
# 1. Clear NuGet cache
dotnet nuget locals all --clear

# 2. Delete all intermediate folders
Remove-Item -Recurse -Force .vs, bin, obj
Remove-Item -Recurse -Force "$env:USERPROFILE\.nuget\packages\*"

# 3. Restore the solution with a clean state
dotnet restore --force-evaluate --no-cache
```

---

## ⚙️ Required .csproj Directives (Per Project or Globally)

Each `.csproj` must contain:

```xml
<PropertyGroup>
  <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  <RestoreLockedMode>false</RestoreLockedMode>
  <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  <DisablePackageRestore>true</DisablePackageRestore>
</PropertyGroup>
```

Place this either directly in each project file or in a shared `Directory.Build.props` at the solution root.

---

## 🔃 Building and Testing One Project at a Time

For each project:

```bash
dotnet clean
dotnet restore --force-evaluate --no-cache
dotnet build ProjectName.csproj --no-incremental
dotnet test ProjectName.csproj --no-build
```

Replace `ProjectName.csproj` with the actual path to each `.csproj`.

---

## 🧠 Notes

- This applies to all projects within the solution.
- Avoid relying on Visual Studio until CLI restores and builds are stable.
- Validate that no project unintentionally depends on transitive types without referencing the source project explicitly.