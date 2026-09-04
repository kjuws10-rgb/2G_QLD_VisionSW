[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [ValidateSet('Enable', 'Disable', 'Status')]
    [string]$Action = 'Enable',

    [string]$DumpFolder = "$env:ProgramData\Vision_Align\CrashDumps",

    [ValidateRange(1, 100)]
    [int]$DumpCount = 10,

    [ValidateSet('Mini', 'Full')]
    [string]$DumpType = 'Mini'
)

$registryPath = 'HKLM:\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\Vision_Align.exe'

function Test-IsAdministrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if ($Action -eq 'Status') {
    if (-not (Test-Path -LiteralPath $registryPath)) {
        Write-Output 'Vision_Align WER LocalDumps: disabled'
        return
    }

    Get-ItemProperty -LiteralPath $registryPath |
        Select-Object DumpFolder, DumpCount, DumpType
    return
}

if (-not (Test-IsAdministrator)) {
    throw 'Run PowerShell as Administrator to change Windows Error Reporting LocalDumps.'
}

if ($Action -eq 'Disable') {
    if ((Test-Path -LiteralPath $registryPath) -and
        $PSCmdlet.ShouldProcess($registryPath, 'Disable Vision_Align crash dumps')) {
        Remove-Item -LiteralPath $registryPath -Recurse
    }

    Write-Output 'Vision_Align WER LocalDumps: disabled'
    return
}

$dumpTypeValue = if ($DumpType -eq 'Full') { 2 } else { 1 }

if ($PSCmdlet.ShouldProcess($registryPath, "Enable $DumpType crash dumps in $DumpFolder")) {
    New-Item -ItemType Directory -Path $DumpFolder -Force | Out-Null
    New-Item -Path $registryPath -Force | Out-Null
    New-ItemProperty -LiteralPath $registryPath -Name DumpFolder -PropertyType ExpandString -Value $DumpFolder -Force | Out-Null
    New-ItemProperty -LiteralPath $registryPath -Name DumpCount -PropertyType DWord -Value $DumpCount -Force | Out-Null
    New-ItemProperty -LiteralPath $registryPath -Name DumpType -PropertyType DWord -Value $dumpTypeValue -Force | Out-Null
}

Write-Output "Vision_Align WER LocalDumps: enabled ($DumpType, keep $DumpCount, $DumpFolder)"
