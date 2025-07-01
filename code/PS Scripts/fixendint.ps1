Get-ChildItem -Recurse -Include *.cs,*.csproj | ForEach-Object {
    (Get-Content $_.FullName) | Set-Content -NoNewline $_.FullName
}
