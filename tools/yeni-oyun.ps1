# Yeni mini oyun iskeleti: klasor + Bootstrap kodu + sahne.
# Iki asamali calisir; ilk asamada yazilan kodu Unity ikinci asamada derlenmis olarak gorur.
#   .\tools\yeni-oyun.ps1 -Oyun Kacis
param(
    [Parameter(Mandatory = $true)][string]$Oyun
)

$ErrorActionPreference = "Stop"
$unityPs = Join-Path $PSScriptRoot "unity.ps1"

Write-Host "[1/2] Dosyalar uretiliyor..." -ForegroundColor Yellow
& $unityPs -Metot "Ulu.Duzenleyici.OyunIskelesi.YeniOyunDosyalari" -Oyun $Oyun
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[2/2] Sahne uretiliyor..." -ForegroundColor Yellow
& $unityPs -Metot "Ulu.Duzenleyici.OyunIskelesi.SahneUret" -Oyun $Oyun
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Hazir: UnityProject\Assets\Oyunlar\$Oyun" -ForegroundColor Green
