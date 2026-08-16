# Kodun derlendigini Editor acmadan dogrular. Kod yazdiktan sonra ilk calistirilan sey budur.
#   .\tools\derle.ps1
& (Join-Path $PSScriptRoot "unity.ps1") -Metot "Ulu.Duzenleyici.YapiCLI.DerlemeKontrol"
exit $LASTEXITCODE
