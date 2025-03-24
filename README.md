# Tubes1_cool

> Tugas Besar 1 IF2211 Strategi Algoritma
Pemanfaatan Algoritma Greedy dalam pembuatan bot permainan Robocode Tank Royale
Semester II Tahun 2024/2025

## Penjelasan singkat algoritma greedy 
### Main Bot (cool)
- **Strategi**: Target terdekat dan gerakan random
- **Perilaku**:
  - Menembak bot terdekat
  - Menyesuaikan daya tembak berdasarkan jarak dan energi
  - Gerakan acak

### Bot Alternatif 1 (cool1)
- **Strategi**: Target energi terendah  
- **Perilaku**:
  - Serang bot dengan energi paling sedikit
  - Mendekat saat energi sendiri rendah
  - Jaga jarak saat musuh kuat

### Bot Alternatif 2 (cool2)
- **Strategi**: Berputar di tengah
- **Perilaku**:
  - Bergerak ke tengah arena sebagai posisi awal
  - Mempertahankan posisi dalam radius 80px dari tengah
 

### Bot Alternatif 3 (cool3)
- **Strategi**: Target dengan ramming atau tembakan
- **Perilaku**:
  - Bergerak linear dengan kecepatan tinggi
  - Mencari musuh terdekat

## Requirement
1. NET 6.0 SDK atau versi terbaru  

2. Java Runtime Environment (JRE) 11+

## Usage

Cara untuk menjalankan program

1. Download jar dari https://github.com/Ariel-HS/tubes1-if2211-starter-pack
 “robocode-tankroyale-gui-0.30.0.jar”, yang merupakan game engine.

2. Buka terminal atau command prompt.
3. Jalankan ini pada terminal
     ```bash
     java -jar robocode-tankroyale-gui-0.30.0.jar
     ```
4. Setup konfigurasi booter dan pilih "Config" lalu pilih “Bot Root Directories”
5. Klik "Start Battle"
 
6. Masukkan directory yang berisi folder-folder bot.  
   
7. Boot lalu select bot-bot yang ingin dimainkan
   
    
## Cara build dan compile

1. Navigasi ke direktori folder bot yang ingin di build/compile
     ```bash
     cd /path/to/bot
     ```
2. Jalankan perintah berikut
    ```bash
     dotnet run --project Botname.csproj
     ```
## Created by

Sabilul Huda (13523072)  
Henry Filberto Shenelo (13523108)  
Andrew Isra Saputra DB (13523110)  

