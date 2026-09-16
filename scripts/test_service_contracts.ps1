$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$root = Split-Path $PSScriptRoot -Parent
$cases = @(
    @('CartographicProjection\ServiceMcpTest\ServiceMcpTest.csproj', ''),
    @('Cluster\ServiceTest\ServiceTest.csproj', ''),
    @('EarthGravity\ServiceTest\ServiceTest.csproj', ''),
    @('EarthMagneticField\ServiceTest\ServiceTest.csproj', ''),
    @('EarthVerticalDatum\ServiceTest\ServiceTest.csproj', ''),
    @('Field\ServiceMcpTest\ServiceMcpTest.csproj', ''),
    @('Field\ServiceUnitTest\ServiceUnitTest.csproj', ''),
    @('GeodeticDatum\ServiceTest\ServiceTest.csproj', 'FullyQualifiedName~McpToolsMetadataTests|FullyQualifiedName~McpToolBehaviorTests'),
    @('Rig\ServiceTest\ServiceTest.csproj', 'FullyQualifiedName~ServiceTest.Tests.|FullyQualifiedName~McpToolRegistrationTests|FullyQualifiedName~RigBatchTransferTests|FullyQualifiedName~GeneratedClientContractTests'),
    @('SurveyInstrument\ServiceTest\ServiceTest.csproj', 'FullyQualifiedName~McpToolRegistrationTests'),
    @('UnitConversion\ServiceTest\ServiceTest.csproj', 'FullyQualifiedName~McpContractTests|FullyQualifiedName~McpNameNormalizerTests|FullyQualifiedName~UnitSystemValidationTests'),
    @('Well\ServiceTest\ServiceTest.csproj', 'FullyQualifiedName~McpToolRegistrationTests|FullyQualifiedName~WellControllerTests')
)
$previousConnection = $env:ConnectionStrings__Sqlite
$failures = @()
try {
    $env:ConnectionStrings__Sqlite = 'Data Source=:memory:'
    foreach ($case in $cases) {
        $project = Join-Path $root "src\$($case[0])"
        $arguments = @('test', $project, '-c', 'Release', '--nologo', '--verbosity', 'quiet')
        if ($case[1]) { $arguments += @('--filter', $case[1]) }
        Write-Host "`nService contracts: $($case[0])"
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) { $failures += $case[0] }
    }
}
finally {
    $env:ConnectionStrings__Sqlite = $previousConnection
}
if ($failures.Count -gt 0) {
    throw "Service contract failures: $($failures -join ', ')"
}
