# Diger scriptlerin dot-source ettigi ortak yardimcilar:  . (Join-Path $PSScriptRoot "ortak.ps1")

function Get-Kok { Split-Path -Parent $PSScriptRoot }

# C# tarafindaki YapiCLI.Slug ile ayni sonucu uretir (klasor adi -> URL parcasi).
function Slugla([string]$ad) {
    $harita = @{ 'ç'='c'; 'Ç'='c'; 'ğ'='g'; 'Ğ'='g'; 'ı'='i'; 'İ'='i'; 'ö'='o'; 'Ö'='o'; 'ş'='s'; 'Ş'='s'; 'ü'='u'; 'Ü'='u' }
    $s = $ad
    foreach ($k in $harita.Keys) { $s = $s.Replace($k, $harita[$k]) }
    $s = $s.ToLowerInvariant()
    $cikti = ""
    foreach ($c in $s.ToCharArray()) {
        if ([char]::IsLetterOrDigit($c)) { $cikti += $c }
        elseif ($cikti.Length -gt 0 -and $cikti[-1] -ne '-') { $cikti += '-' }
    }
    return $cikti.Trim('-')
}

# docs\oyunlar.json icindeki oyun kaydini ekler/gunceller (galeri bunu okur).
function Galeriye-Yaz([string]$kok, [string]$oyun, [string]$slug, [string]$aciklama) {
    $yol = Join-Path $kok "docs\oyunlar.json"
    $liste = @()
    if (Test-Path $yol) {
        $ham = Get-Content $yol -Raw
        if ($ham.Trim()) { $liste = @(ConvertFrom-Json $ham) }
    }

    $kayit = $liste | Where-Object { $_.slug -eq $slug } | Select-Object -First 1
    if ($kayit) {
        $kayit.ad = $oyun
        if ($aciklama) { $kayit.aciklama = $aciklama }
        $kayit.guncelleme = (Get-Date -Format "yyyy-MM-dd")
    } else {
        $liste += [pscustomobject]@{
            slug       = $slug
            ad         = $oyun
            aciklama   = $aciklama
            guncelleme = (Get-Date -Format "yyyy-MM-dd")
        }
    }

    $liste = @($liste | Sort-Object ad)
    # -AsArray: tek oyun kaldiginda bile JSON dizi olarak yazilsin (galeri dizi bekliyor).
    ($liste | ConvertTo-Json -Depth 4 -AsArray) | Set-Content -Path $yol -Encoding UTF8
    return $yol
}
