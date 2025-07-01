Get-ChildItem -Recurse -Include *.cs,*.csproj,*.sln,*.xml,*.json,*.yml,*.yaml,*.md,*.txt,*.html,*.js,*.ts,*.jsx,*.tsx,*.css,*.scss,*.razor,*.xaml,*.xsd,*.config,*.ps1,*.sh,*.cmd,*.bat |
  ForEach-Object {
    $path = $_.FullName
    (Get-Content $path -Raw) -replace "`r`n", "`n" | Set-Content -NoNewline -Encoding UTF8 $path
  }
