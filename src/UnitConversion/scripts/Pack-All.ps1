[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+([-.][0-9A-Za-z.-]+)?$')]
    [string]$Version = '3.4.1',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [ValidateSet('Debug', 'Release')]
    [string]$GeneratorConfiguration = 'Debug',
    [string]$OutputDirectory = 'artifacts/packages',
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$packageOutput = Join-Path $repoRoot $OutputDirectory
$localProjectProperty = '-p:UseLocalUnitConversionProjects=true'

function Invoke-DotNet {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

Push-Location $repoRoot
try {
    New-Item -ItemType Directory -Force -Path $packageOutput | Out-Null

    Invoke-DotNet restore '.\UnitConversion.sln' $localProjectProperty
    # Enumeration files are configuration-independent source artifacts. Keep the
    # generator in Debug; the solution and NuGet packages are still built in Release.
    Invoke-DotNet run --project '.\GenerateEnumerations\GenerateEnumerations.csproj' --configuration $GeneratorConfiguration --no-restore $localProjectProperty
    Invoke-DotNet build '.\UnitConversion.sln' --configuration $Configuration --no-restore $localProjectProperty '-p:GeneratePackageOnBuild=false'
    if (-not $SkipTests) {
        Invoke-DotNet test '.\UnitConversion.sln' --configuration $Configuration --no-build $localProjectProperty
    }

    $projects = @(
        '.\Conversion\Conversion.csproj',
        '.\Conversion.DrillingEngineering\Conversion.DrillingEngineering.csproj',
        '.\ConversionUnitSystem\ConversionUnitSystem.csproj',
        '.\ConversionUnitSystem.DrillingEngineering\ConversionUnitSystem.DrillingEngineering.csproj',
        '.\DrillingRazorMudComponents\DrillingRazorMudComponents.csproj',
        '.\WebPages\WebPages.csproj'
    )
    foreach ($project in $projects) {
        Invoke-DotNet pack $project --configuration $Configuration --no-build $localProjectProperty "-p:PackageVersion=$Version" --output $packageOutput
    }
}
finally {
    Pop-Location
}
