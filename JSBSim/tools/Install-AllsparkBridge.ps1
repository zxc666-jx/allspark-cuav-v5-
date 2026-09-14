param(
    [string]$TargetRoot = 'E:\PlaneGroup_Releases\PlaneGroup_GCS_HITL_v0.1.1_20260905',
    [switch]$Apply
)
$ErrorActionPreference = 'Stop'
$manifestPath = Join-Path $PSScriptRoot 'PATCH_MANIFEST.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$target = [IO.Path]::GetFullPath($TargetRoot).TrimEnd('\')
if (-not (Test-Path -LiteralPath (Join-Path $target 'JSBSim\hil_supervisor.py'))) {
    throw 'Select the release root containing JSBSim and MissionPlanner, not the source directory.'
}
function Resolve-Child([string]$Root, [string]$Relative) {
    $full = [IO.Path]::GetFullPath((Join-Path $Root $Relative))
    if (-not $full.StartsWith($Root.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path escapes the selected root: $Relative"
    }
    $cursor = $full
    while ($cursor -and $cursor.Length -ge $Root.Length) {
        if (Test-Path -LiteralPath $cursor) {
            if ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw "Refusing a reparse-point path: $cursor"
            }
        }
        $cursor = [IO.Path]::GetDirectoryName($cursor)
    }
    return $full
}
$payloadRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'payload'))
$changes = @()
foreach ($entry in $manifest.files) {
    $source = Resolve-Child $payloadRoot $entry.path
    $destination = Resolve-Child $target $entry.path
    if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $entry.sha256) {
        throw "Package checksum mismatch: $($entry.path)"
    }
    $current = if (Test-Path -LiteralPath $destination) {
        (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash
    } else { $null }
    if ($current -eq $entry.sha256) { continue }
    if ($current -ne $entry.previous_sha256) {
        throw "Local file differs from the verified baseline. Preserve/review it first: $destination"
    }
    $changes += [pscustomobject]@{ Entry = $entry; Source = $source; Destination = $destination }
}
# Fail closed if process enumeration fails. Never kill a simulation or hot-swap it.
$running = @(Get-CimInstance Win32_Process -ErrorAction Stop | Where-Object {
    $_.Name -match '^(python(w)?|MissionPlanner)\.exe$' -and
    (($_.CommandLine -and $_.CommandLine.IndexOf($target, [StringComparison]::OrdinalIgnoreCase) -ge 0) -or
     ($_.ExecutablePath -and $_.ExecutablePath.StartsWith($target + '\', [StringComparison]::OrdinalIgnoreCase)))
})
Write-Output "Patch version: $($manifest.version)"
Write-Output "Target: $target"
Write-Output "Verified files: $(@($manifest.files).Count); pending replacements: $($changes.Count)"
if ($running.Count) {
    $running | Select-Object Name, ProcessId, CommandLine | Format-List | Out-Host
    if ($Apply) { throw 'Close MissionPlanner and its HITL backends before applying. Nothing was overwritten.' }
    Write-Output 'CHECK ONLY: running processes detected; stop them before applying.'
}
if (-not $Apply) {
    Write-Output 'CHECK ONLY: no files changed. Run again with -Apply after stopping HITL safely.'
    exit 0
}
if (-not $changes.Count) {
    Write-Output 'All payload files already match this patch.'
    exit 0
}
$backup = Join-Path $target ('Allspark_Backup_' + (Get-Date -Format 'yyyyMMdd_HHmmss') + '_' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $backup | Out-Null
# Back up every existing target before writing any replacement. Configs, logs,
# dependencies and MissionPlanner.exe are deliberately absent from the manifest.
foreach ($change in $changes) {
    if (Test-Path -LiteralPath $change.Destination) {
        $oldCopy = Resolve-Child $backup $change.Entry.path
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($oldCopy)) -Force | Out-Null
        Copy-Item -LiteralPath $change.Destination -Destination $oldCopy
    }
}
Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $backup 'PATCH_MANIFEST.json')
foreach ($change in $changes) {
    New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($change.Destination)) -Force | Out-Null
    Copy-Item -LiteralPath $change.Source -Destination $change.Destination -Force
    if ((Get-FileHash -LiteralPath $change.Destination -Algorithm SHA256).Hash -ne $change.Entry.sha256) {
        throw "Installed checksum mismatch. Do not start HITL; restore from $backup"
    }
}
Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $target 'ALLSPARK_PATCH_MANIFEST.json') -Force
Write-Output "Installed and verified. Backup: $backup"
Write-Output 'Firmware files were copied only; no board was flashed. Restart HITL only with the matching firmware.'
