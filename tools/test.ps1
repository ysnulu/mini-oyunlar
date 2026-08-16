# EditMode testlerini calistirir (saf mantik testleri: kayit, cizim, zorluk egrisi).
#   .\tools\test.ps1
$ErrorActionPreference = "Stop"

$kok   = Split-Path -Parent $PSScriptRoot
$proje = Join-Path $kok "UnityProject"
$unity = "C:\Program Files\unity\6000.5.2f1\Editor\Unity.exe"
$log     = Join-Path $PSScriptRoot "son-log.txt"
$sonuc   = Join-Path $PSScriptRoot "test-sonuc.xml"

if (Get-Process Unity -ErrorAction SilentlyContinue) {
    Write-Host "HATA: Unity Editor acik. Once kapat." -ForegroundColor Red
    exit 2
}
$kilit = Join-Path $proje "Temp\UnityLockfile"
if (Test-Path $kilit) { Remove-Item $kilit -Force -ErrorAction SilentlyContinue }

$argler = @("-batchmode", "-nographics", "-projectPath", "`"$proje`"", "-runTests",
            "-testPlatform", "EditMode", "-testResults", "`"$sonuc`"", "-logFile", "`"$log`"")
$surec = Start-Process -FilePath $unity -ArgumentList $argler -Wait -PassThru -NoNewWindow
$kod = $surec.ExitCode

if (Test-Path $sonuc) {
    [xml]$x = Get-Content $sonuc
    $k = $x.'test-run'
    Write-Host ("Test: toplam {0}, gecen {1}, kalan {2}, atlanan {3}" -f $k.total, $k.passed, $k.failed, $k.skipped) -ForegroundColor Cyan
    if ([int]$k.failed -gt 0) {
        $x.SelectNodes("//test-case[@result='Failed']") | ForEach-Object {
            Write-Host ("  X " + $_.fullname) -ForegroundColor Red
            Write-Host ("    " + $_.failure.message.'#cdata-section') -ForegroundColor DarkGray
        }
    }
} else {
    Write-Host "Sonuc dosyasi olusmadi - $log dosyasina bak." -ForegroundColor Red
    Select-String -Path $log -Pattern "error CS|Licensing|No valid Unity Editor license" | Select-Object -Last 15 | ForEach-Object { Write-Host ("  " + $_.Line.Trim()) }
}

exit $kod
