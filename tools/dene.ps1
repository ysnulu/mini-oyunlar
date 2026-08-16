# Derlenmis oyunu yerelde tarayicida acar (yayinlamadan once son kontrol).
#   .\tools\dene.ps1 -Oyun Kacis           (mevcut derlemeyi ac)
#   .\tools\dene.ps1 -Oyun Kacis -Derle    (once WebGL'e derle, sonra ac)
param(
    [Parameter(Mandatory = $true)][string]$Oyun,
    [int]$Port = 8080,
    [switch]$Derle
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "ortak.ps1")

$kok  = Get-Kok
$slug = Slugla $Oyun
$dizin = Join-Path $kok "docs"

if ($Derle) {
    & (Join-Path $PSScriptRoot "web-derle.ps1") -Oyun $Oyun
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

if (-not (Test-Path (Join-Path $dizin "$slug\index.html"))) {
    Write-Host "HATA: docs\$slug\index.html yok. Once: .\tools\web-derle.ps1 -Oyun $Oyun" -ForegroundColor Red
    exit 1
}

# Zaten calisan sunucu varsa yenisini acma.
$mevcut = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
if (-not $mevcut) {
    $python = if (Get-Command py -ErrorAction SilentlyContinue) { "py" } else { "python" }
    # Yolda bosluk var; Start-Process argumanlari tirnaklamadan birlestirdigi icin tirnagi biz koyuyoruz.
    Start-Process -FilePath $python -ArgumentList @("-m", "http.server", "$Port", "--directory", "`"$dizin`"") -WindowStyle Minimized
    Start-Sleep -Seconds 2

    if (-not (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)) {
        Write-Host "HATA: yerel sunucu $Port portunda acilmadi." -ForegroundColor Red
        exit 1
    }
}

$adres = "http://localhost:$Port/$slug/"
Write-Host "Acilan adres: $adres" -ForegroundColor Green
Start-Process $adres
