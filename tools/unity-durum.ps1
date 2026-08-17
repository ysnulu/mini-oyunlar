# unity-baslat.ps1 ile baslatilan isin durumunu gosterir: calisiyor mu, log ne diyor, cikis kodu ne.
#   .\tools\unity-durum.ps1
$ErrorActionPreference = "SilentlyContinue"

$log   = Join-Path $PSScriptRoot "son-log.txt"
$durum = Join-Path $PSScriptRoot "son-durum.txt"

if (-not (Test-Path $durum)) { Write-Host "Baslatilmis is kaydi yok."; exit 0 }

$parcalar = (Get-Content $durum -Raw).Trim() -split "\|"
$pid_ = [int]$parcalar[0]
$is   = $parcalar[1]
$bas  = $parcalar[2]

$surec = Get-Process -Id $pid_ -ErrorAction SilentlyContinue
$gecen = [int]((New-TimeSpan -Start ([datetime]$bas) -End (Get-Date)).TotalSeconds)

if ($surec) {
    Write-Host "CALISIYOR  $is   ($gecen sn)" -ForegroundColor Yellow
} else {
    Write-Host "BITTI  $is   ($gecen sn icinde)" -ForegroundColor Green
}

if (Test-Path $log) {
    Write-Host "--- log ---"
    Select-String -Path $log -Pattern "error CS|\[ULU\]|Build Failed|BuildFailedException|Unhandled Exception|No valid Unity Editor license|Test run completed|Exiting with|A crash has been" |
        Select-Object -Last 12 | ForEach-Object {
            $s = $_.Line.Trim()
            Write-Host ("  " + $s.Substring(0, [Math]::Min(160, $s.Length)))
        }
    $son = (Get-Item $log).LastWriteTime.ToString("HH:mm:ss")
    Write-Host "  (log son yazim: $son)"
}
