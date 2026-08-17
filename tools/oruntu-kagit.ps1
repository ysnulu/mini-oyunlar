# Oruntu'nun kural motorundan yazdirilabilir calisma kagidi uretir (HTML + SVG).
# Tarayicida acilir, Ctrl+P ile A4 kagida ya da PDF'e basilir.
#   .\tools\oruntu-kagit.ps1                        (bolum 3, 12 soru)
#   .\tools\oruntu-kagit.ps1 -Bolum 7 -Soru 16
#   .\tools\oruntu-kagit.ps1 -Renksiz               (siyah-beyaz baski: renk ozelligi kullanilmaz)
#   .\tools\oruntu-kagit.ps1 -Tohum 42              (ayni tohum ayni kagidi uretir)
param(
    [int]$Bolum = 3,
    [int]$Soru = 12,
    [int]$Tohum = 0,
    [switch]$Renksiz,
    [string]$Cikti = "",
    [switch]$Acma
)

$ErrorActionPreference = "Stop"

$kok   = Split-Path -Parent $PSScriptRoot
$proje = Join-Path $kok "UnityProject"
$unity = "C:\Program Files\unity\6000.5.2f1\Editor\Unity.exe"
$log   = Join-Path $PSScriptRoot "son-log.txt"

if (Get-Process Unity -ErrorAction SilentlyContinue) {
    Write-Host "HATA: Unity Editor acik. Once kapat (ya da Editor icinden Ulu > Oruntu menusunu kullan)." -ForegroundColor Red
    exit 2
}
$kilit = Join-Path $proje "Temp\UnityLockfile"
if (Test-Path $kilit) { Remove-Item $kilit -Force -ErrorAction SilentlyContinue }

if ($Tohum -eq 0) { $Tohum = Get-Random -Minimum 1 -Maximum 999999 }
if (-not $Cikti) {
    $ek = if ($Renksiz) { "-sb" } else { "" }
    $Cikti = Join-Path $kok "kagitlar\oruntu-b$Bolum$ek-$Tohum.html"
}

$argler = @("-batchmode", "-nographics", "-quit", "-projectPath", "`"$proje`"",
            "-executeMethod", "Ulu.Duzenleyici.OruntuKagit.Uret",
            "-bolum", $Bolum, "-soru", $Soru, "-tohum", $Tohum, "-cikti", "`"$Cikti`"",
            "-logFile", "`"$log`"", "-burst-disable-compilation")
if ($Renksiz) { $argler += "-renksiz" }

# Unity.exe bir GUI uygulamasi: & ile cagrilirsa beklemez ve cikis kodu vermez.
$surec = Start-Process -FilePath $unity -ArgumentList $argler -Wait -PassThru -NoNewWindow

if ($surec.ExitCode -ne 0 -or -not (Test-Path $Cikti)) {
    Write-Host "Calisma kagidi uretilemedi (cikis $($surec.ExitCode)) - $log dosyasina bak." -ForegroundColor Red
    Select-String -Path $log -Pattern "error CS|\[ULU\]|Licensing" | Select-Object -Last 15 |
        ForEach-Object { Write-Host ("  " + $_.Line.Trim()) -ForegroundColor DarkGray }
    exit 1
}

Write-Host "Calisma kagidi: $Cikti" -ForegroundColor Green
Write-Host "Yazdirmak icin tarayicida Ctrl+P (cevap anahtari son sayfada)." -ForegroundColor DarkGray
if (-not $Acma) { Start-Process $Cikti }
