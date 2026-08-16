# Mini Oyunlar

Unity ile yapılmış küçük 2D oyunlar. Hepsi tarayıcıda açılır, kurulum gerektirmez,
telefonda da oynanır.

**Oyna:** https://ysnulu.github.io/mini-oyunlar/

## Oyunlar

| Oyun | Nasıl oynanır |
|---|---|
| [Kaçış](https://ysnulu.github.io/mini-oyunlar/kacis/) | 3 şeritli sonsuz kaçış — A/D veya ekranın soluna/sağına dokun |

## Nasıl yapıldı

Her oyunun tamamı koddan kurulur: sahnede tek bir nesne vardır, kamera-oyuncu-arayüz-ses
o nesnenin üstündeki tek script tarafından üretilir. Görseller ve sesler de kodla üretilir,
projede hiç görsel/ses dosyası yoktur.

- `UnityProject/Assets/Ortak/` — çizim, ses, girdi, arayüz, kayıt, oyun omurgası
- `UnityProject/Assets/Oyunlar/` — oyun başına bir klasör
- `tools/` — derleme, test ve yayın komutları (PowerShell)
- `docs/` — GitHub Pages: galeri ve WebGL çıktıları

Unity 6000.5.2f1 · 2D URP · Yasin Ulu
