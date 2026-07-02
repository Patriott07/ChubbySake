# 🌀 BlastBlade

**BlastBlade** adalah game pertarungan gasing 3D bertema arena bertempo cepat. Pemain mengontrol gasing dalam mode _Best of 3_ melawan musuh AI dengan sistem tabrakan (_collision-based combat_), energi serangan khusus, item ramuan (_power-up_), dan mini-game QTE.

Dikembangkan menggunakan **Unity 6 (6000.1.7f1)** dengan **Universal Render Pipeline (URP) 17**.

---

## Daftar Isi

- [Fitur-Fitur Utama](#fitur-fitur-utama)
- [Cara Bermain](#cara-bermain)
- [Kontrol](#kontrol)
- [Sistem & Mekanik](#sistem--mekanik)
  - [Sistem RPM](#sistem-rpm)
  - [Sistem Energi](#sistem-energi)
  - [Sistem Bagian Tubuh & Defense](#sistem-bagian-tubuh--defense)
  - [Item Ramuan (Potion)](#item-ramuan-potion)
  - [Sistem Ronde (Best of 3)](#sistem-ronde-best-of-3)
  - [Sistem Kombo](#sistem-kombo)
  - [Sistem Save & Load](#sistem-save--load)
  - [Sistem Progres & Leveling](#sistem-progres--leveling)
- [Musuh & AI](#musuh--ai)
- [Action Attack & QTE](#action-attack--qte)
- [Kamera](#kamera)
- [Post-Processing & Efek Visual](#post-processing--efek-visual)
- [Struktur Proyek](#struktur-proyek)
- [Teknologi & Dependensi](#teknologi--dependensi)
- [Cara Menjalankan](#cara-menjalankan)
- [Pengembangan](#pengembangan)

---

## Fitur-Fitur Utama

| Fitur | Deskripsi |
|---|---|
| ⚔️ **Pertarungan Gasing** | Tabrakan fisik antar gasing dengan knockback & damage |
| 🎯 **Action Attack + QTE** | Serangan spesial slow-mo dengan mini-game lingkaran |
| 🧪 **6 Jenis Ramuan** | Health, Speed, Damage, Invincible, Energy Attack, Energy Ult |
| 🤖 **AI 4 Tingkat Kesulitan** | Easy / Medium / Hard / Impossible |
| 🏆 **Best of 3 Rounds** | Sistem pertandingan 3 ronde dengan skor & reward |
| 🔄 **Sistem RPM** | Rotasi memengaruhi damage & visual spin |
| 📸 **5 Mode Kamera** | Isometric, Hard Lock, Top-Down, Cinematic Orbit, Close-Up |
| ✨ **Post-Processing Dinamis** | Vignette, Bloom, Chromatic Aberration, Lens Distortion |
| 💾 **Save Terenkripsi** | AES + HMAC untuk rilis; JSON debug untuk development |
| 📈 **Progres & Leveling** | Gold, XP, level, dan unlockables |

---

## Cara Bermain

1. **Pilih Pengaturan** atur jumlah musuh (1/2/4), tingkat kesulitan (Normal/Hard/Expert), dan peta (Bedroom/Kitchen/Living Room).
2. **Bertarung** kendalikan gasing Anda untuk menabrak musuh. Setiap tabrakan mengurangi HP dan RPM.
3. **Kumpulkan Ramuan** yang muncul secara acak di arena untuk mendapatkan keuntungan.
4. **Isi Energi Attack** dengan menabrak musuh, lalu tekan **Spasi** untuk melancarkan Action Attack.
5. **Menangkan Ronde** dengan menghabisi HP semua musuh. Rebut 2 dari 3 ronde untuk memenangkan pertandingan.
6. **Dapatkan Reward** gold dan XP untuk naik level dan membuka konten baru.

---

## Kontrol

| Tombol | Aksi |
|---|---|
| **W/A/S/D** atau **Arrow Keys** | Dorong gasing (arah gerakan) |
| **Spasi** | Action Attack (saat energi penuh) |
| **Mouse** | Bidik & klik lingkaran QTE saat Action Attack |
| **ESC** | (direncanakan) Pause menu |

> Gasing memiliki gerakan otomatis bawaan. Input pemain berfungsi sebagai dorongan tambahan (_force multiplier_) untuk mengubah arah.

---

## Sistem & Mekanik

### Sistem RPM

_Rotations Per Minute_ adalah stat inti yang memengaruhi:

- **Kecepatan rotasi visual** gasing
- **Damage multiplier saat tabrakan**
- RPM turun saat bertabrakan dan **regenerasi otomatis** seiring waktu
- Rentang: **400 (min)** – **1200 (max)**

### Sistem Energi

Dua jenis energi yang terisi secara terpisah:

| Energi | Cara Isi | Digunakan Untuk |
|---|---|---|
| **Energy Attack** | Bertabrakan dengan musuh (+100–150 per tabrakan) | Action Attack (Spasi) |
| **Energy Ultimate** | Ramuan Energy Ult | (direncanakan) Serangan ultimate |

### Sistem Bagian Tubuh & Defense

Gasing memiliki 4 bagian tubuh dengan multiplier defense berbeda:

| Bagian | Defense Multiplier |
|---|---|
| Head | 1.0× (normal) |
| Body | 0.8× (lebih tahan) |
| Hand | 1.2× (lebih rentan) |
| Leg | 0.9× |

Damage akhir = kekuatan benturan × multiplier defense bagian yang terkena.

### Item Ramuan (Potion)

Ramuan muncul secara acak di arena (maks 2 simultan) dengan jeda waktu tertentu:

| Ramuan | Efek |
|---|---|
| ❤️ **Health** | Pulihkan HP sebesar persentase max HP |
| ⚡ **Energy Attack** | Isi energi attack |
| 💜 **Energy Ultimate** | Isi energi ultimate |
| 🏃 **Speed Move** | Tingkatkan kecepatan gerak untuk 5 detik |
| ⚔️ **Damage** | Tingkatkan damage untuk 10 detik |
| 🛡️ **Kebal** | Invincible selama 3 detik |
| 🎁 **Box** | Efek acak (belum diimplementasikan penuh) |

### Sistem Ronde (Best of 3)

- Setiap ronde, kedua gasing di-respawn ke posisi awal
- Hitung mundur **READY → SET → GO!** sebelum pertarungan dimulai
- Pemain yang mencapai **2 kemenangan ronde** memenangkan pertandingan
- Pemenang mendapat gold & XP lebih besar (15 gold / 25 XP vs 5 gold / 10 XP)

### Sistem Kombo

- Hitungan kombo bertambah setiap kali pemain mengenai musuh dalam waktu tertentu
- Jika jeda terlalu lama, kombo _reset_ ke 0
- Menampilkan teks "Combo X" di layar

### Sistem Save & Load

- **Mode Debug** menyimpan JSON mentah untuk kemudahan development
- **Mode Release** menggunakan enkripsi **AES** + integritas **HMAC-SHA256** untuk mencegah kecurangan
- File disimpan di `Application.persistentDataPath`

### Sistem Progres & Leveling

- Gold dan XP dikumpulkan dari hasil pertandingan
- Level naik secara otomatis saat XP mencapai threshold
- Data disimpan via `PlayerPrefs` dengan struktur `ProgressInGame`

---

## Musuh & AI

AI musuh memiliki **4 tingkat kesulitan** yang memengaruhi berbagai parameter:

| Parameter | Easy | Medium | Hard | Impossible |
|---|---|---|---|---|
| **Interval keputusan** | 1.5s | 0.8s | 0.3s | 0.05s |
| **Peluang mengejar** | 30% | 60% | 90% | 100% |
| **Auto Speed** | 3 | 5 | 6.5 | 8 |
| **Damage multiplier** | 0.8× | 1.05× | 1.2× | 1.5× |
| **HP multiplier** | 0.8× | 1.05× | 2.0× | 3.0× |
| **Bonus damage serangan** | 5–15 | 15–30 | 30–50 | 50–200 |

AI secara otomatis mengisi energi serangan dan melancarkan **Action Attack** sendiri saat energi penuh, dengan efek slow-mo yang sama seperti pemain.

---

## Action Attack & QTE

Saat **Energy Attack penuh**, tekan **Spasi** untuk memicu:

1. **Slow-mo ekstrem** (timeScale = 0.05) selama 5 detik
2. Lingkaran QTE muncul secara acak di layar
3. Arahkan kursor ke lingkaran dan klik untuk mengumpulkan bonus damage (+10 s/d +70 per lingkaran)
4. Setelah 5 detik, gasing **melesat homing** ke arah musuh
5. Damage normal + seluruh bonus QTE diberikan dalam satu serangan
6. Gasing menjadi **invincible** selama dash

---

## Kamera

Terdapat **5 mode kamera** yang dapat berubah secara dinamis:

| Mode | Deskripsi |
|---|---|
| **IsometricAction** | Kamera isometrik dengan auto-zoom berdasarkan jarak |
| **HardLockPlayer** | Kamera mengunci pemain |
| **TopDown** | Tampilan atas |
| **CinematicOrbit** | Orbit sinematik (aktif saat kematian) |
| **CloseUpAction** | Close-up dramatis |

Kamera juga mendukung **screen shake** yang dipicu oleh tabrakan.

---

## Post-Processing & Efek Visual

Memanfaatkan **URP Volume** dengan efek:

| Efek | Trigger |
|---|---|
| **Vignette** | Menguat saat HP rendah (warna merah) |
| **Bloom** | Berdenyut (_pulse_) saat energi penuh |
| **Chromatic Aberration** | Spike saat attack / kena damage / speed boost |
| **Lens Distortion** | Efek distorsi pada attack & speed boost |
| **Color Adjustments** | Perubahan exposure saat reward |

Efek dikategorikan sebagai:
- **Continuous** (berjalan terus: vignette HP, pulse bloom)
- **One-Shot** (dipicu sekali: attack, heal, reward, damage, speed boost)

---

## Struktur Proyek

```
BlastBlade/
├── Assets/
│   ├── 3d/                      # Model, material, tekstur 3D
│   │   ├── Materials/           # Material berbasis tekstur
│   │   ├── Mats/                # Material tambahan
│   │   ├── Models/              # Model FBX & Blender
│   │   └── Textures/            # Tekstur JPG/PNG
│   ├── Fonts/                   # Font Luckiest Guy
│   ├── GUI/                     # Gambar GUI
│   ├── Prefabs/                 # Prefab pemain, musuh, ramuan, damage text
│   ├── Resources/               # DOTween settings
│   ├── Scenes/                  # Scene permainan
│   ├── Script/
│   │   ├── Camera/
│   │   │   └── MultiModeActionCamera.cs
│   │   ├── SO/                  # Scriptable Objects
│   │   │   ├── PotionData.cs
│   │   │   └── *.asset          # Data ramuan
│   │   ├── System/              # Sistem inti game
│   │   │   ├── _struct.cs           # Struktur data global
│   │   │   ├── GameEvents.cs        # Event statis UnityAction
│   │   │   ├── GasingMovement.cs    # Fisik & gerakan gasing
│   │   │   ├── GasingStat.cs        # Stat & damage system
│   │   │   ├── GasingPartCollider.cs# Deteksi per bagian tubuh
│   │   │   ├── GasingActionAttack.cs# Action Attack & QTE
│   │   │   ├── GasingAI.cs         # Kecerdasan buatan musuh
│   │   │   ├── PotionItem.cs       # Logika ramuan
│   │   │   ├── PotionSpawner.cs    # Spawn ramuan periodik
│   │   │   ├── RoundManager.cs     # Manajemen ronde & skor
│   │   │   ├── SaveSystem.cs       # Save/load terenkripsi
│   │   │   ├── ProgressManager.cs  # Gold, XP, leveling
│   │   │   ├── PostProcessingFX.cs # Efek visual URP
│   │   │   ├── DamageTextUI.cs     # Floating damage number
│   │   │   ├── MovementGuide.cs    # Indikator arah
│   │   │   ├── LineUI.cs          # Garis UI
│   │   │   ├── LantaiScript.cs    # Batas arena (instant kill)
│   │   │   └── DestroyTrigger.cs  # Utility destroy
│   │   └── UI/
│   │       ├── MainMenuController.cs
│   │       ├── MapSelectUI.cs
│   │       ├── HUDUIHandler.cs
│   │       ├── MatchResultUI.cs
│   │       ├── ComboManager.cs
│   │       ├── QTESpawner.cs
│   │       ├── QTECircle.cs
│   │       ├── SettingsManager.cs
│   │       ├── TutorialManager.cs
│   │       └── ObjectHighlight.cs
│   ├── Plugins/
│   │   └── Demigiant/DOTween/   # Animasi DOTween
│   ├── QuickOutline/            # Outline object (Chris Nolet)
│   ├── Settings/                # URP Render Pipeline assets
│   └── TextMesh Pro/            # Asset TMP
├── Packages/
│   └── manifest.json
├── ProjectSettings/
└── BlastBlade.sln
```

---

## Teknologi & Dependensi

| Teknologi | Versi |
|---|---|
| **Unity** | 6000.1.7f1 (Unity 6) |
| **Render Pipeline** | Universal RP 17.1.0 |
| **Input System** | 1.14.0 |
| **UI** | uGUI + TextMesh Pro |
| **Animasi** | DOTween (Demigiant) |
| **Outline** | QuickOutline (Chris Nolet) |
| **AI Navigation** | 2.0.8 |
| **Visual Scripting** | 1.9.7 |

**Paket Unity resmi:**
- `com.unity.inputsystem`
- `com.unity.render-pipelines.universal`
- `com.unity.ai.navigation`
- `com.unity.ugui`
- `com.unity.textmeshpro`
- `com.unity.timeline`
- `com.unity.visualscripting`
- `com.unity.test-framework`

---

## Cara Menjalankan

1. **Clone repositori:**
   ```bash
   git clone https://github.com/username/BlastBlade.git
   ```

2. **Buka di Unity 6000.1.7f1** atau lebih baru.

3. Biarkan Unity mengimpor asset dan menyelesaikan dependensi paket.

4. Buka scene **`Assets/Scenes/SampleScene.unity`** (atau scene utama lainnya).

5. Tekan tombol **Play** di Unity Editor.

> **Catatan:** Scene `MainMenu`, `StoreScene`, dan `Store` dirujuk dalam kode tetapi belum ada. Gunakan `SampleScene` sebagai entry point.

---

## Pengembangan

### Git Commit Convention

Kode ini menggunakan campuran bahasa Inggris dan Indonesia. Disarankan untuk konsisten dengan gaya yang sudah ada.

### Mode Debug vs Release

Di `SaveSystem.cs`, ubah `isDebugMode = false` untuk mengaktifkan enkripsi AES + HMAC.

### Scene yang Tersedia

| Scene | Status |
|---|---|
| `SampleScene` | Scene pertarungan utama (aktif) |
| `RasyidMemek` | Scene tambahan (placeholder) |
| `Abmek` | Scene tambahan (placeholder) |
| `Abdiscene` | Scene dengan post-processing profile terpisah |

### Catatan Pengembangan

- Belum ada animasi `.anim` (folder Animation masih kosong)
- Scene `MainMenu`, `Store` masih perlu dibuat
- Beberapa nama scene bersifat placeholder/development
- Sistem kombo sudah ada backend (`ComboManager.cs`) tetapi trigger dari collision masih perlu integrasi penuh

---

## Lisensi

Hak cipta © 2026 DefaultCompany. Seluruh aset, kode, dan konten dalam repositori ini adalah milik pengembang masing-masing.

---

_Dibuat dengan ❤️ menggunakan Unity 6._
