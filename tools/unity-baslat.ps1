# Unity'yi bagimsiz (detached) baslatir ve hemen doner; komut satiri beklemez.
# Uzun islerde (WebGL derlemesi, tam yeniden derleme) sarmalayici surec oldurulse bile
# Unity calismaya devam eder. Durumu tools\unity-durum.ps1 ile izle.
#   .\tools\unity-baslat.ps1 -Metot Ulu.Duzenleyici.YapiCLI.WebGLYap -Oyun Oruntu -Hedef WebGL
param(
    [Parameter(Mandatory = $true)][string]$Metot,
    [string]$Oyun,
    [string]$Hedef,                 # ornek: WebGL (bos ise platform degismez)
    [switch]$Testler                # -runTests modu (Metot yoksayilir)
)

$ErrorActionPreference = "Stop"

$kok   = Split-Path -Parent $PSScriptRoot
$proje = Join-Path $kok "UnityProject"
$unity = "C:\Program Files\unity\6000.5.2f1\Editor\Unity.exe"
$log   = Join-Path $PSScriptRoot "son-log.txt"
$durum = Join-Path $PSScriptRoot "son-durum.txt"
$sonuc = Join-Path $PSScriptRoot "test-sonuc.xml"

if (Get-Process Unity -ErrorAction SilentlyContinue) {
    Write-Host "HATA: Unity zaten calisiyor (Editor ya da onceki batchmode isi)." -ForegroundColor Red
    exit 2
}
$kilit = Join-Path $proje "Temp\UnityLockfile"
if (Test-Path $kilit) { Remove-Item $kilit -Force -ErrorAction SilentlyContinue }
Remove-Item $durum -Force -ErrorAction SilentlyContinue

function Tirnakla([string]$d) { if ($d -match '\s') { return '"' + $d + '"' } return $d }

if ($Testler) {
    $argler = @("-batchmode", "-nographics", "-burst-disable-compilation",
                "-projectPath", (Tirnakla $proje), "-logFile", (Tirnakla $log),
                "-runTests", "-testPlatform", "EditMode", "-testResults", (Tirnakla $sonuc))
} else {
    $argler = @("-batchmode", "-quit", "-nographics", "-burst-disable-compilation",
                "-projectPath", (Tirnakla $proje), "-logFile", (Tirnakla $log),
                "-executeMethod", $Metot)
    if ($Oyun) { $argler += @("-oyun", (Tirnakla $Oyun)) }
    if ($Hedef) { $argler += @("-buildTarget", $Hedef) }
}

$surec = Start-Process -FilePath $unity -ArgumentList $argler -PassThru
"$($surec.Id)|$Metot $Oyun|$(Get-Date -Format s)" | Set-Content $durum -Encoding UTF8

Write-Host "Baslatildi (PID $($surec.Id)): $Metot $Oyun" -ForegroundColor Cyan
Write-Host "Durum: .\tools\unity-durum.ps1"
