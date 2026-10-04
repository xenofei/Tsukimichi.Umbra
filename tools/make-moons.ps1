# Regenerates assets/moons/*.svg and their 64 px PNGs (the PNGs are embedded in the add-on).
# Needs node and resvg on PATH.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
node (Join-Path $PSScriptRoot 'make-moons.mjs')
Get-ChildItem (Join-Path $root 'assets/moons') -Filter *.svg | ForEach-Object {
    $png = [IO.Path]::ChangeExtension($_.FullName, '.png')
    resvg -w 64 -h 64 $_.FullName $png
    Write-Host $png
}
