# Rancangan Skenario: AI Belajar Repositori & Rutinitas Update Harian

## 1. Ringkasan Proyek

**AplikasiBanking8583** adalah aplikasi perbankan berbasis **ASP.NET Core 8.0 MVC** dengan integrasi standar **ISO 8583** financial transaction messaging.

### Teknologi
| Komponen | Versi | Keterangan |
|----------|-------|------------|
| .NET | 8.0 | Framework backend |
| ASP.NET Core MVC | 8.0 | Pattern arsitektur |
| EF Core | 8.0 | ORM |
| MySQL | 8.x | Database (via Laragon, port 3306) |
| Swashbuckle | 10.2.3 | Swagger/OpenAPI |

### Fitur Utama
1. **Manajemen Rekening**: Buka rekening, detail, daftar rekening
2. **Transaksi Keuangan** (MTI 0200/0210): Setor, Tarik, Transfer
3. **Inquiry Non-Financial** (MTI 0100/0110): Cek Saldo, Riwayat Transaksi, Buka Rekening
4. **Reversal** (MTI 0420/0430)
5. **REST API Simulator** dengan Swagger UI
6. **Audit Trail**: Setiap transaksi menyimpan pesan ISO 8583 lengkap di kolom `Iso8583MessageRaw`

### Struktur Kode
```
/Controllers/
  AccountController.cs        → MVC banking operations (CRUD + transaksi)
  HomeController.cs           → Landing page
  ISO8583/Iso8583SimulatorController.cs → REST API simulator
/Models/
  Account.cs                  → Entitas rekening
  Transaction.cs              → Entitas transaksi (STAN, RRN, MTI, raw message)
  BankingDbContext.cs         → EF Core DbContext (MySQL)
  ISO8583/
    Iso8583Message.cs         → Core: MTI, Bitmap, Data Elements, Parse/ToString
    Iso8583TransactionBuilder.cs → Build request messages (0100/0200/0420)
    Iso8583ResponseBuilder.cs   → Build response messages (0110/0210/0430)
/Views/                      → Razor views (MVC UI)
/wwwroot/                    → Static assets (Bootstrap 5)
/docs/                       → Dokumen arsitektur
/Properties/launchSettings.json → Konfigurasi server
```

---

## 2. Status Implementasi vs. Rancangan

| Fitur | Status | Keterangan |
|-------|--------|------------|
| Setor (0200/0210) | ✅ Selesai | Controller + API + Builder |
| Tarik (0200/0210) | ✅ Selesai | Controller + API + Builder |
| Transfer (0200/0210) | ✅ Selesai | Controller + API + Builder |
| Balance Inquiry (0100/0110) | ✅ Selesai | Controller + API + Builder |
| Account Opening (0100/0110) | ✅ Selesai | Controller + API + Builder |
| Transaction History (0100/0110) | ✅ Selesai | Controller + API + Builder |
| Reversal (0420/0430) | ⚠️ Parsial | Builder ada, belum ada controller/API |
| iso8583_audit_log table | ❌ Belum | Didokumenkan di design, belum implement |
| Parse pesan ISO 8583 | ✅ Selesai | API endpoint `/api/iso8583/parse-message` |
| EF Migrations | ❌ Belum | Pakai `Database.EnsureCreated()` |

### Rintis Pengembangan yang Terindentifikasi
1. **Repository ini sudah jauh berkembang** — arsitektur design doc (docs/) merepresentasikan proposal, dan sebagian besar sudah diimplementasikan.
2. **Belum ada test project** — hanya manual testing via curl dan Swagger.
3. **Security**: Anti-forgery dinonaktifkan untuk semua POST endpoints.
4. **Error handling**: Semua response code hardcoded "00" (success) — belum ada error path echtief.
5. **TransactionCategory/OperationType/ResponseMessage** ada di model dan DB context, tapi **belum ada filter API** untuk `GetTransactions`.
6. **Reversal endpoint** tidak ada di controller.

---

## 3. Rancangan Rutinitas Update Harian ke GitHub

### 3.1 Arsitektur Rutinitas

```
┌─────────────────────────────────────────────────────┐
│           Rutinitas Update Harian                    │
├─────────────────────────────────────────────────────┤
│                                                      │
│  ┌──────────────┐     ┌──────────────┐    ┌─────────┐ │
│  │ AI / Kilo    │────▶│ Script       │────│ GitHub  │ │
│  │ (agent)      │     │ daily-update │    │ Actions │ │
│  │              │     │ .sh          │    │ CI      │ │
│  └──────────────┘     └──────────────┘    └─────────┘ │
│         │                │                   │       │
│         │ 1. Develop     │                   │       │
│         │    code        │                   │       │
│         │                │ 2. Build check    │       │
│         │                │    (dotnet build) │       │
│         │                │                   │       │
│         │                │ 3. git add -A     │       │
│         │                │ 4. git commit     │ 5. Push│
│         │                │ 5. git push       │    &  │
│         │                │                   │ Verify│
│         └────────────────┴───────────────────┴───────┘ │
└─────────────────────────────────────────────────────┘
```

### 3.2 Komponen yang Disiapkan

| File | Kegunaan |
|------|----------|
| `AGENTS.md` | Panduan Kilo: build commands, konvensi kode, pola ISO 8583 |
| `.gitignore` | Mengecualikan `bin/`, `obj/`, `*.user`, logs, dll |
| `.github/workflows/ci.yml` | CI pipeline: restore → build → verify pada setiap push/PR |
| `.github/workflows/daily-update.yml` | Scheduled workflow: build + auto-commit "daily update" tiap hari pukul 01:00 UTC |
| `scripts/daily-update.sh` | Script lokal: build → stage → commit → push |
| `docs/daily-update-routine.md` | Dokumen ini (rancangan skenario) |

### 3.3 Alur Harian (Manual)

```bash
# 1. Kilo / AI mengembangkan kode
# ... edits to Controllers/, Models/ISO8583/, Views/, dll ...

# 2. Jalankan rutinitas harian
bash scripts/daily-update.sh
```

Script akan:
1. Cek git status
2. Build project (`dotnet build`)
3. Stage semua perubahan (`git add -A`)
4. Commit dengan pesan `"daily update"`
5. Push ke `origin main`

### 3.4 Alur CI/CD Otomatis

**`ci.yml`** — Trigger pada setiap `push` dan `pull_request` ke `main`:
- Checkout kode
- Setup .NET 8.0
- `dotnet restore`
- `dotnet build --no-restore`
- Verifikasi build berhasil

**`daily-update.yml`** — Trigger terjadwal tiap hari:
- Checkout kode
- Setup .NET 8.0
- `dotnet restore` → `dotnet build`
- Jika ada perubahan yang belum di-commit → commit `"daily update"` → push

### 3.5 Git Conventions

- **Branch**: `main` (primary)
- **Remote**: `https://github.com/kandasirah/aplikasibanking8583.git`
- **User**: `kandasirah` / `kandasirojulahmad@outlook.com`
- **Commit messages**: format pendek, deskriptif
  - `"daily update"` — untuk commit otomatis harian
  - `"iso builder"` — untuk perubahan kode spesifik
  - `"rilis"` — untuk rilis

### 3.6 Komando Sehari-hari AI/Kilo

```bash
# Build
dotnet build

# Run lokal (butuh MySQL/Laragon running)
dotnet run

# Commit manual
git add -A
git commit -m "deskripsi singkat"
git push origin main

# Daily routine
bash scripts/daily-update.sh
```

---

## 4. Rekomendasi Pengembangan Lanjutan

1. **Buat test project** (xUnit) untuk unit test ISO 8583 builders
2. **Tambahkan EF Migrations** untuk database schema management
3. **Implementasi Reversal controller endpoint** (sekarang hanya builder ada)
4. **Tambahkan filter di API** `/api/iso8583/transactions` untuk `TransactionCategory` dan `OperationType`
5. **Perbaiki error handling** — response codes dinamis berdasarkan hasil transaksi
6. **Aktifkan antiforgery protection** untuk production (matikan hanya di dev/testing)
7. **Buat iso8583_audit_log table** untuk audit trail terperinci

---

## 5. Timeline

| Fase | Aktivitas | Estimasi |
|------|-----------|----------|
| Setup | AGENTS.md, .gitignore, CI workflow, daily script | 1 hari |
| CI/CD | GitHub Actions pipelines | 1 hari |
| Rutinitas | Script daily update + otomatisasi | 1 hari |
| (Opsional) | Test project, migrations, error handling | 2-3 minggu |

**Status**: ✅ Setup rutinitas update harian — siap pakai
