param(
    [string]$Configuration = "Release",
    [string]$Version = "0.2.2.0",
    [string[]]$Platforms = @("x64")
)

$ErrorActionPreference = "Stop"

$ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ProjectDir "CmdPalBrowserBookmarks.csproj"

Write-Host "Building CmdPalBrowserBookmarks $Version" -ForegroundColor Green

dotnet restore $ProjectFile

foreach ($Platform in $Platforms) {
    $Runtime = "win-$Platform"
    $PublishDir = Join-Path $ProjectDir "bin\$Configuration\$Runtime\publish"

    Write-Host "Publishing $Runtime..." -ForegroundColor Cyan
    dotnet publish $ProjectFile `
        --configuration $Configuration `
        --runtime $Runtime `
        --self-contained true `
        -p:Version=$Version `
        -p:AssemblyVersion=$Version `
        -p:FileVersion=$Version `
        --output $PublishDir

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $Runtime"
    }

    $SetupTemplate = Get-Content (Join-Path $ProjectDir "setup-template.iss") -Raw
    $SetupScript = $SetupTemplate -replace '#define AppVersion ".*"', "#define AppVersion `"$Version`""
    $SetupScript = $SetupScript -replace 'OutputBaseFilename=CmdPalBrowserBookmarks-Setup-\{#AppVersion\}', "OutputBaseFilename=CmdPalBrowserBookmarks-Setup-{#AppVersion}-$Platform"
    $SetupScript = $SetupScript -replace 'Source: "bin\\Release\\win-x64\\publish\\\*"', "Source: `"bin\$Configuration\$Runtime\publish\*`""

    if ($Platform -eq "arm64") {
        $SetupScript = $SetupScript -replace '(MinVersion=10\.0\.22000)', "ArchitecturesAllowed=arm64`r`nArchitecturesInstallIn64BitMode=arm64`r`n`$1"
    }
    else {
        $SetupScript = $SetupScript -replace '(MinVersion=10\.0\.22000)', "ArchitecturesAllowed=x64compatible`r`nArchitecturesInstallIn64BitMode=x64compatible`r`n`$1"
    }

    $SetupPath = Join-Path $ProjectDir "setup-$Platform.iss"
    Set-Content -Path $SetupPath -Value $SetupScript -Encoding UTF8

    $InnoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6\iscc.exe"
    if (-not (Test-Path $InnoSetupPath)) {
    $InnoSetupPath = "${env:ProgramFiles}\Inno Setup 6\iscc.exe"
}

if (-not (Test-Path $InnoSetupPath)) {
    $InnoSetupPath = "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
}


    if (Test-Path $InnoSetupPath) {
        & $InnoSetupPath $SetupPath
        if ($LASTEXITCODE -ne 0) {
            throw "Inno Setup failed for $Platform"
        }
    }
    else {
        Write-Warning "Inno Setup was not found. Published files are available at $PublishDir."
    }
}
