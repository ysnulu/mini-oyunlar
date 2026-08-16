# Unity'yi batchmode'da çalıştıran ortak sarmalayıcı.
# Diğer tüm tools/*.ps1 bunu çağırır; tek başına da kullanılabilir:
#   .\tools\unity.ps1 -Metot Ulu.Duzenleyici.YapiCLI.DerlemeKontrol
param(
    [Parameter(Mandatory = $true)][string]$Metot,
    [string]$Oyun,
    [string[]]$UnityArg = @(),
    [string]$Log,
    [switch]$Sessiz
)

$ErrorActionPreference = "Stop"

$kok    = Split-Path -Parent $PSScriptRoot
$proje  = Join-Path $kok "UnityProject"
$unity  = "C:\Program Files\unity\6000.5.2f1\Editor\Unity.exe"
if (-not $Log) { $Log = Join-Path $PSScriptRoot "son-log.txt" }

if (-not (Test-Path $unity)) { Write-Host "HATA: Unity bulunamadi: $unity" -ForegroundColor Red; exit 3 }
if (-not (Test-Path $proje)) { Write-Host "HATA: Unity projesi yok: $proje" -ForegroundColor Red; exit 3 }

# Editor acikken batchmode calismaz (proje kilidi).
if (Get-Process Unity -ErrorAction SilentlyContinue) {
    Write-Host "HATA: Unity Editor acik. Batchmode islerinden once Editor'u kapat." -ForegroundColor Red
    exit 2
}
# Editor kapali ama kilit dosyasi kalmissa (cokme/yarim kapanma) temizle.
$kilit = Join-Path $proje "Temp\UnityLockfile"
if (Test-Path $kilit) { Remove-Item $kilit -Force -ErrorAction SilentlyContinue }

# Start-Process arguman dizisini bosluklara gore birlestirir, tirnaklamaz.
# Yollarda bosluk var ("Claude Projeleri"), bu yuzden tirnagi biz koyuyoruz.
function Tirnakla([string]$deger) {
    if ($deger -match '\s') { return '"' + $deger + '"' }
    return $deger
}

$argler = @(
    "-batchmode", "-quit", "-nographics",
    "-projectPath", (Tirnakla $proje),
    "-logFile", (Tirnakla $Log),
    "-executeMethod", $Metot
)
if ($Oyun) { $argler += @("-oyun", (Tirnakla $Oyun)) }
if ($UnityArg.Count -gt 0) { $argler += ($UnityArg | ForEach-Object { Tirnakla $_ }) }

if (-not $Sessiz) { Write-Host ">> $Metot $Oyun" -ForegroundColor Cyan }
$baslangic = Get-Date
# Unity.exe Windows'ta GUI uygulamasidir: "&" ile cagrilirsa beklemez ve cikis kodu alinmaz.
# Start-Process -Wait -PassThru ile hem bekleriz hem gercek cikis kodunu okuruz.
$surec = Start-Process -FilePath $unity -ArgumentList $argler -Wait -PassThru -NoNewWindow
$kod = $surec.ExitCode
$sure = [int]((Get-Date) - $baslangic).TotalSeconds

if (Test-Path $Log) {
    $onemli = Select-String -Path $Log -Pattern "error CS|\[ULU\]|Build Failed|BuildFailedException|Unhandled Exception|No valid Unity Editor license|Licensing" |
              Select-Object -Last 25
    foreach ($satir in $onemli) { Write-Host ("  " + $satir.Line.Trim()) }
}

if ($kod -eq 0) { Write-Host "TAMAM ($sure sn)" -ForegroundColor Green }
else { Write-Host "BASARISIZ (cikis $kod, $sure sn) - tam kayit: $Log" -ForegroundColor Red }
exit $kod
