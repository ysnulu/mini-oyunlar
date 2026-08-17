# Oyunu WebGL'e derler, galeriye ekler, commit'ler ve GitHub'a gonderir.
#   .\tools\yayinla.ps1 -Oyun Kacis -Aciklama "3 seritli sonsuz kacis" -Mesaj "Kacis ilk surum"
#   .\tools\yayinla.ps1 -Oyun Kacis -DerlemeyiAtla     (mevcut derlemeyi yayinla)
param(
    [Parameter(Mandatory = $true)][string]$Oyun,
    [string]$Ad = "",          # galeride gorunen ad (Turkce); bos ise klasor adi kullanilir
    [string]$Aciklama = "",
    [string]$Mesaj = "",
    [switch]$DerlemeyiAtla,
    [switch]$GonderMe
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "ortak.ps1")

$kok  = Get-Kok
$slug = Slugla $Oyun

if (-not $DerlemeyiAtla) {
    & (Join-Path $PSScriptRoot "web-derle.ps1") -Oyun $Oyun
    if ($LASTEXITCODE -ne 0) { Write-Host "Derleme basarisiz, yayin durduruldu." -ForegroundColor Red; exit $LASTEXITCODE }
}

$ciktiDizin = Join-Path $kok "docs\$slug"
if (-not (Test-Path (Join-Path $ciktiDizin "index.html"))) {
    Write-Host "HATA: $ciktiDizin icinde index.html yok." -ForegroundColor Red
    exit 1
}

# GitHub Pages Jekyll'i atlasin (alt cizgiyle baslayan dosyalar yok sayilmasin).
$nojekyll = Join-Path $kok "docs\.nojekyll"
if (-not (Test-Path $nojekyll)) { New-Item -ItemType File -Path $nojekyll | Out-Null }

if (-not $Ad) { $Ad = $Oyun }
$galeri = Galeriye-Yaz -kok $kok -oyun $Ad -slug $slug -aciklama $Aciklama
Write-Host "Galeri guncellendi: $galeri" -ForegroundColor Cyan

if (-not $Mesaj) { $Mesaj = "$Oyun yayinlandi" }

Push-Location $kok
try {
    # Yeni derleme dosyalarini virus tarayicisi bir an kilitleyebiliyor; bir kez daha dene.
    git add -A
    if ($LASTEXITCODE -ne 0) {
        Write-Host "git add takildi (dosya kilidi olabilir), tekrar deneniyor..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
        git add -A
        if ($LASTEXITCODE -ne 0) { throw "git add basarisiz" }
    }
    $degisiklik = git status --porcelain
    if (-not $degisiklik) {
        Write-Host "Degisiklik yok, commit atlandi." -ForegroundColor Yellow
    } else {
        git commit -m $Mesaj
        if ($LASTEXITCODE -ne 0) { throw "commit basarisiz" }
    }

    if (-not $GonderMe) {
        git push
        if ($LASTEXITCODE -ne 0) { throw "push basarisiz" }
        $uzak = (git remote get-url origin) -replace "\.git$", ""
        $kullanici = ($uzak -split "/")[-2]
        $depo = ($uzak -split "/")[-1]
        Write-Host "Yayinlandi: https://$kullanici.github.io/$depo/$slug/" -ForegroundColor Green
        Write-Host "(Pages ilk derlemeyi bitirene kadar birkac dakika 404 gorebilirsin.)" -ForegroundColor DarkGray
    }
}
finally {
    Pop-Location
}
