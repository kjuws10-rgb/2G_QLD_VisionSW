[CmdletBinding()]
param(
    [string]$InstallPath = (Join-Path $PSScriptRoot '..\BIN'),

    [datetime]$IncidentTime = (Get-Date),

    [ValidateRange(1, 1440)]
    [int]$WindowMinutes = 30,

    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'CollectedDiagnostics')
)

$resolvedInstallPath = (Resolve-Path -LiteralPath $InstallPath -ErrorAction Stop).Path
$windowStart = $IncidentTime.AddMinutes(-$WindowMinutes)
$windowEnd = $IncidentTime.AddMinutes($WindowMinutes)
$bundleName = 'VisionAlign_Diagnostics_{0}' -f $IncidentTime.ToString('yyyyMMdd_HHmmss')
$stagingRoot = Join-Path ([IO.Path]::GetTempPath()) ($bundleName + '_' + [guid]::NewGuid().ToString('N'))
$zipPath = Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) ($bundleName + '.zip')
$logDateTokens = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$logDate = $windowStart.Date
while ($logDate -le $windowEnd.Date) {
    [void]$logDateTokens.Add($logDate.ToString('yyyyMMdd'))
    $logDate = $logDate.AddDays(1)
}

function Copy-FilePreservingRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceRoot,

        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo]$File,

        [Parameter(Mandatory = $true)]
        [string]$DestinationRoot
    )

    $rootWithSeparator = $SourceRoot.TrimEnd('\') + '\'
    $relativePath = $File.FullName.Substring($rootWithSeparator.Length)
    $destinationPath = Join-Path $DestinationRoot $relativePath
    $destinationParent = Split-Path -Parent $destinationPath
    New-Item -ItemType Directory -Path $destinationParent -Force | Out-Null
    Copy-Item -LiteralPath $File.FullName -Destination $destinationPath -Force
}

try {
    New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

    $manifest = [System.Collections.Generic.List[string]]::new()
    $manifest.Add('Vision_Align diagnostic collection')
    $manifest.Add('==================================')
    $manifest.Add('CollectedLocal={0:O}' -f (Get-Date))
    $manifest.Add('IncidentLocal={0:O}' -f $IncidentTime)
    $manifest.Add('WindowStart={0:O}' -f $windowStart)
    $manifest.Add('WindowEnd={0:O}' -f $windowEnd)
    $manifest.Add('InstallPath=' + $resolvedInstallPath)
    $manifest.Add('ComputerName=' + $env:COMPUTERNAME)
    $manifest.Add('UserName=' + $env:USERNAME)

    $executablePath = Join-Path $resolvedInstallPath 'Vision_Align.exe'
    if (Test-Path -LiteralPath $executablePath) {
        $executable = Get-Item -LiteralPath $executablePath
        $manifest.Add('ExecutableVersion=' + $executable.VersionInfo.FileVersion)
        $manifest.Add('ExecutableModifiedLocal={0:O}' -f $executable.LastWriteTime)
        $manifest.Add('ExecutableSHA256=' + (Get-FileHash -LiteralPath $executablePath -Algorithm SHA256).Hash)

        Copy-Item -LiteralPath $executablePath -Destination $stagingRoot -Force
        $pdbPath = Join-Path $resolvedInstallPath 'Vision_Align.pdb'
        if (Test-Path -LiteralPath $pdbPath) {
            Copy-Item -LiteralPath $pdbPath -Destination $stagingRoot -Force
        }
    }

    $diagnosticPath = Join-Path $resolvedInstallPath 'LOG\DIAGNOSTIC'
    if (Test-Path -LiteralPath $diagnosticPath) {
        $diagnosticDestination = Join-Path $stagingRoot 'LOG\DIAGNOSTIC'
        New-Item -ItemType Directory -Path $diagnosticDestination -Force | Out-Null
        Copy-Item -Path (Join-Path $diagnosticPath '*') -Destination $diagnosticDestination -Recurse -Force
    }

    $logRoot = Join-Path $resolvedInstallPath 'LOG'
    if (Test-Path -LiteralPath $logRoot) {
        $logDestination = Join-Path $stagingRoot 'LOG'
        Get-ChildItem -LiteralPath $logRoot -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object {
                $_.FullName -notlike ($diagnosticPath.TrimEnd('\') + '\*') -and
                $logDateTokens.Contains($_.BaseName)
            } |
            ForEach-Object {
                Copy-FilePreservingRelativePath -SourceRoot $logRoot -File $_ -DestinationRoot $logDestination
            }
    }

    $resultRoot = Join-Path $resolvedInstallPath 'RESULT'
    if (Test-Path -LiteralPath $resultRoot) {
        $resultDestination = Join-Path $stagingRoot 'RESULT'
        Get-ChildItem -LiteralPath $resultRoot -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object {
                $_.LastWriteTime -ge $windowStart -and $_.LastWriteTime -le $windowEnd
            } |
            ForEach-Object {
                Copy-FilePreservingRelativePath -SourceRoot $resultRoot -File $_ -DestinationRoot $resultDestination
            }
    }

    $configRoot = Join-Path $resolvedInstallPath 'CONFIG'
    if (Test-Path -LiteralPath $configRoot) {
        $configDestination = Join-Path $stagingRoot 'CONFIG'
        New-Item -ItemType Directory -Path $configDestination -Force | Out-Null
        Get-ChildItem -LiteralPath $configRoot -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Extension -in @('.json', '.ini', '.mot') } |
            Copy-Item -Destination $configDestination -Force
    }

    try {
        Get-WinEvent -FilterHashtable @{
            LogName = 'Application'
            StartTime = $windowStart
            EndTime = $windowEnd
        } -ErrorAction Stop |
            Where-Object {
                $_.ProviderName -in @('.NET Runtime', 'Application Error', 'Windows Error Reporting') -or
                $_.Message -match 'Vision_Align|halcon|uEye|AXL'
            } |
            Select-Object TimeCreated, ProviderName, Id, LevelDisplayName, Message |
            Export-Csv -LiteralPath (Join-Path $stagingRoot 'Windows_Application_Events.csv') -NoTypeInformation -Encoding UTF8
    }
    catch {
        $manifest.Add('WindowsEventCollectionError=' + $_.Exception.Message)
    }

    $dumpRoot = Join-Path $env:ProgramData 'Vision_Align\CrashDumps'
    if (Test-Path -LiteralPath $dumpRoot) {
        $dumpDestination = Join-Path $stagingRoot 'CrashDumps'
        New-Item -ItemType Directory -Path $dumpDestination -Force | Out-Null
        Get-ChildItem -LiteralPath $dumpRoot -File -Filter '*.dmp' -ErrorAction SilentlyContinue |
            Where-Object { $_.LastWriteTime -ge $windowStart -and $_.LastWriteTime -le $windowEnd } |
            Copy-Item -Destination $dumpDestination -Force
    }

    $manifest | Set-Content -LiteralPath (Join-Path $stagingRoot 'collection-manifest.txt') -Encoding UTF8

    if (Test-Path -LiteralPath $zipPath) {
        throw "Output already exists: $zipPath"
    }

    Compress-Archive -Path (Join-Path $stagingRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Output $zipPath
}
finally {
    $resolvedStagingRoot = [IO.Path]::GetFullPath($stagingRoot)
    $resolvedTempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if ($resolvedStagingRoot.StartsWith($resolvedTempRoot, [StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path -LiteralPath $resolvedStagingRoot)) {
        Remove-Item -LiteralPath $resolvedStagingRoot -Recurse -Force
    }
}
