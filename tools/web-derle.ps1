# Bir oyunu WebGL'e derler; cikti docs\<slug>\ altina yazilir.
# Ilk derleme uzun surer (IL2CPP + Emscripten, 5-15 dk), sonrakiler hizlanir.
#   .\tools\web-derle.ps1 -Oyun Kacis
param(
    [Parameter(Mandatory = $true)][string]$Oyun
)

$ErrorActionPreference = "Stop"
& (Join-Path $PSScriptRoot "unity.ps1") -Metot "Ulu.Duzenleyici.YapiCLI.WebGLYap" -Oyun $Oyun -UnityArg @("-buildTarget", "WebGL")
exit $LASTEXITCODE
