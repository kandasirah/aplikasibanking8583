# Aplikasi Banking dengan ISO 8583

Aplikasi banking modern yang dibangun menggunakan ASP.NET Core MVC dengan integrasi layer transaksi finansial ISO 8583. Aplikasi ini mencakup fitur pembukaan akun, setor, tarik, transfer, dan deposit dengan penyimpanan data transaksi ke MySQL di Laragon.

## Daftar Isi

- [Fitur Utama](#fitur-utama)
- [Teknologi yang Digunakan](#teknologi-yang-digunakan)
- [Arsitektur Aplikasi](#arsitektur-aplikasi)
- [Struktur Database](#struktur-database)
- [Metode Transaksi ISO 8583](#metode-transaksi-iso-8583)
- [API Endpoints](#api-endpoints)
- [Cara Menjalankan Aplikasi](#cara-menjalankan-aplikasi)
- [Testing](#testing)
- [Keamanan](#keamanan)
- [Lisensi](#lisensi)

---

## Fitur Utama

### 1. Manajemen Rekening
- **Buka Rekening Baru** — Membuat rekening dengan nomor unik, nama pemilik, dan tipe rekening
- **Detail Rekening** — Melihat informasi lengkap rekening
- **Daftar Rekening** — Menampilkan semua rekening dengan saldo terkini

### 2. Transaksi Keuangan
- **Setor Tunai** — Menambah saldo rekening
- **Tarik Tunai** — Mengurangi saldo dengan validasi saldo cukup
- **Transfer** — Transfer antar rekening dengan referensi unik
- **Cek Saldo** — Inquiry saldo via ISO 8583 tanpa perubahan saldo
- **Riwayat Transaksi** — Melihat semua transaksi dengan detail ISO 8583

### 3. ISO 8583 Financial Transaction Layer
- Setiap operasi perbankan menghasilkan pesan ISO 8583 lengkap (MTI + Bitmap + Data Elements)
- Financial transactions: MTI 0200/0210 (Setor, Tarik, Transfer)
- Non-financial transactions: MTI 0100/0110 (Balance Inquiry, Account Opening, Transaction History)
- Penyimpanan data ISO 8583 di database untuk audit dan compliance
- STAN (System Trace Audit Number) unik untuk setiap transaksi
- RRN (Retrieval Reference Number) untuk tracking
- Response Code sesuai standar ISO 8583
- Kategori transaksi: FINANCIAL, INQUIRY, ACCOUNT_OPENING

### 4. API Simulator
- REST API untuk simulasi semua transaksi ISO 8583
- Swagger UI untuk dokumentasi dan testing API
- Endpoint untuk parsing pesan ISO 8583

---

## Teknologi yang Digunakan

| Teknologi | Versi | Deskripsi |
|-----------|-------|-----------|
| .NET | 8.0 | Framework backend |
| ASP.NET Core MVC | 8.0 | Pattern arsitektur |
| Entity Framework Core | 8.0 | ORM untuk database |
| MySQL | 8.4 | Database (via Laragon) |
| Pomelo.EntityFrameworkCore.MySql | 9.0.0 | MySQL provider untuk EF Core |
| Swashbuckle.AspNetCore | 10.2.3 | Swagger/OpenAPI documentation |
| Bootstrap | 5.x | Frontend UI framework |

---

## Arsitektur Aplikasi

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                         │
│  ┌──────────────────────┐      ┌─────────────────────────┐  │
│  │   MVC Web UI         │      │   REST API (Swagger)    │  │
│  │   - Account Views    │      │   - ISO8583 Simulator   │  │
│  │   - Transaction UI   │      │   - JSON Responses      │  │
│  └──────────────────────┘      └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              Controllers                              │   │
│  │  ┌────────────────┐    ┌─────────────────────────┐   │   │
│  │  │ AccountController│    │ Iso8583SimulatorController│   │   │
│  │  │ - Index         │    │ - SimulateSetor          │   │   │
│  │  │ - Create        │    │ - SimulateTarik          │   │   │
│  │  │ - Deposit       │    │ - SimulateTransfer       │   │   │
│  │  │ - Withdraw      │    │ - SimulateBalanceInquiry │   │   │
│  │  │ - Transfer      │    │ - SimulateAccountOpening │   │   │
│  │  │ - BalanceInquiry│    │ - SimulateTransactionHistory│   │   │
│  │  │ - TransactionHistory│ │ - GetTransactions        │   │   │
│  │  │ - History       │    │ - ParseMessage           │   │   │
│  │  │ - Details       │    └─────────────────────────┘   │   │
│  │  └────────────────┘                                    │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    ISO 8583 Layer                           │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              ISO8583 Models                           │   │
│  │  ┌────────────────┐  ┌──────────────────────────┐   │   │
│  │  │ Iso8583Message │  │ Iso8583TransactionBuilder│   │   │
│  │  │ - MTI          │  │ - BuildSetorMessage      │   │   │
│  │  │ - DataElements │  │ - BuildTarikMessage      │   │   │
│  │  │ - Parse()      │  │ - BuildTransferMessage   │   │   │
│  │  │ - ToString()   │  │ - BuildBalanceInquiryMessage│   │   │
│  │  │ Iso8583Message │  │ - BuildAccountOpeningMessage│   │   │
│  │  │                 │  │ - BuildTransactionHistoryMessage│   │
│  │  │                 │  │ - GenerateSTAN           │   │   │
│  │  └────────────────┘  └──────────────────────────┘   │   │
│  │  ┌──────────────────────────────────────────────┐   │   │
│  │  │      Iso8583ResponseBuilder                   │   │   │
│  │  │ - BuildSetorResponse                          │   │   │
│  │  │ - BuildTarikResponse                          │   │   │
│  │  │ - BuildTransferResponse                       │   │   │
│  │  │ - BuildBalanceInquiryResponse                 │   │   │
│  │  │ - BuildAccountOpeningResponse                 │   │   │
│  │  │ - BuildTransactionHistoryResponse             │   │   │
│  │  └──────────────────────────────────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Data Access Layer                        │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              BankingDbContext                         │   │
│  │  ┌────────────────┐  ┌──────────────────────────┐   │   │
│  │  │   DbSet<Account>│  │   DbSet<Transaction>     │   │   │
│  │  └────────────────┘  └──────────────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Database Layer                           │
│                   MySQL (Laragon)                            │
│  ┌────────────────┐  ┌──────────────────────────┐         │
│  │    accounts    │  │     transactions          │         │
│  └────────────────┘  └──────────────────────────┘         │
└─────────────────────────────────────────────────────────────┘
```

### Design Patterns

- **MVC Pattern** — Separation of concerns between Model, View, Controller
- **Repository Pattern** — EF Core DbContext acts as repository
- **Builder Pattern** — ISO 8583 message builders for constructing complex messages
- **Service Layer** — ISO 8583 processing logic separated from controllers

---

## Struktur Database

### Tabel: accounts

| Field | Type | Null | Key | Default | Description |
|-------|------|------|-----|---------|-------------|
| Id | int | NO | PRI | AUTO_INCREMENT | Primary key |
| AccountNumber | varchar(20) | NO | UNI | - | Nomor rekening unik |
| AccountHolderName | varchar(100) | NO | - | - | Nama pemilik rekening |
| Balance | decimal(18,2) | NO | - | 0.00 | Saldo rekening |
| AccountType | varchar(50) | NO | - | - | Tipe rekening (Tabungan, Giro, Deposito) |
| CreatedAt | datetime | NO | - | CURRENT_TIMESTAMP | Tanggal pembukaan |

### Tabel: transactions

| Field | Type | Null | Key | Default | Description |
|-------|------|------|-----|---------|-------------|
| Id | int | NO | PRI | AUTO_INCREMENT | Primary key |
| AccountId | int | NO | MUL | - | Foreign key ke accounts |
| TransactionType | varchar(20) | NO | - | - | Jenis transaksi (Setor, Tarik, Transfer) |
| Amount | decimal(18,2) | NO | - | - | Jumlah transaksi |
| Description | varchar(200) | NO | - | - | Deskripsi transaksi |
| Reference | varchar(50) | YES | - | NULL | Referensi transaksi |
| Stan | varchar(6) | NO | - | - | System Trace Audit Number |
| Rrn | varchar(12) | NO | - | - | Retrieval Reference Number |
| ResponseCode | varchar(4) | NO | - | 00 | Response code ISO 8583 |
| MTI | varchar(20) | NO | - | - | Message Type Indicator |
| Iso8583MessageRaw | varchar(2000) | NO | - | - | Raw ISO 8583 message |
| TransactionCategory | varchar(20) | NO | - | FINANCIAL | Kategori transaksi (FINANCIAL, INQUIRY, ACCOUNT_OPENING) |
| OperationType | varchar(50) | NO | - | - | Jenis operasi (SETOR, TARIK, TRANSFER, BALANCE_INQUIRY, HISTORY_INQUIRY, ACCOUNT_OPENING) |
| ResponseMessage | varchar(200) | YES | - | NULL | Pesan response human-readable |
| TransactionDate | datetime | NO | - | CURRENT_TIMESTAMP | Waktu transaksi |

### Entity Relationship Diagram

```
┌─────────────────┐         ┌─────────────────┐
│    accounts     │         │   transactions  │
├─────────────────┤         ├─────────────────┤
│ Id (PK)         │◄────────│ AccountId (FK)  │
│ AccountNumber   │   1:N   │ TransactionType │
│ AccountHolder   │         │ Amount          │
│ Balance         │         │ Description     │
│ AccountType     │         │ Reference       │
│ CreatedAt       │         │ Stan            │
└─────────────────┘         │ Rrn             │
                            │ ResponseCode    │
                            │ MTI             │
                            │ Iso8583MessageRaw│
                            │ TransactionCategory│
                            │ OperationType   │
                            │ ResponseMessage │
                            │ TransactionDate │
                            └─────────────────┘
```

---

## Metode Transaksi ISO 8583

### 1. Setor Tunai (Credit Transaction)

**MTI:** `0200` (Request) → `0210` (Response)

**Data Elements:**
| Element | Name | Value | Description |
|---------|------|-------|-------------|
| 2 | Primary Account Number | AccountNumber | Nomor rekening |
| 3 | Processing Code | `310000` | Setor tunai |
| 4 | Amount, Transaction | Amount * 100 | Jumlah dalam cents |
| 7 | Transmission Date & Time | MMDDhhmmss | Waktu transmisi |
| 11 | System Trace Audit Number | STAN | Nomor urut transaksi |
| 12 | Time, Local Transaction | hhmmss | Waktu transaksi |
| 13 | Date, Local Transaction | MMDD | Tanggal transaksi |
| 17 | Date, Capture | MMDD | Tanggal capture |
| 22 | Point of Service Entry Mode | `012` | Entry mode |
| 32 | Acquiring Institution ID | `1234567890` | ID institusi |
| 35 | Track 2 Data | AccountNumber | Track 2 data |
| 37 | Retrieval Reference Number | RRN | Nomor referensi |
| 41 | Card Acceptor Terminal ID | TerminalId | ID terminal |
| 42 | Card Acceptor Identification Code | `BANKINGAPPSYS` | Kode sistem |
| 43 | Card Acceptor Name/Location | `BANKING APPLICATION TERMINAL` | Nama terminal |
| 49 | Currency Code, Transaction | `360` | Kode mata uang (IDR) |
| 58 | From Account | Description | Deskripsi |
| 60 | Transaction Type | `SETOR` | Tipe transaksi |
| 61 | To Account | AccountId | ID rekening tujuan |

**Response Code:**
- `00` — Berhasil
- `05` — Saldo tidak cukup

### 2. Tarik Tunai (Debit Transaction)

**MTI:** `0200` (Request) → `0210` (Response)

**Data Elements:**
| Element | Name | Value | Description |
|---------|------|-------|-------------|
| 2 | Primary Account Number | AccountNumber | Nomor rekening |
| 3 | Processing Code | `010000` | Tarik tunai |
| 4 | Amount, Transaction | Amount * 100 | Jumlah dalam cents |
| 7 | Transmission Date & Time | MMDDhhmmss | Waktu transmisi |
| 11 | System Trace Audit Number | STAN | Nomor urut transaksi |
| 12 | Time, Local Transaction | hhmmss | Waktu transaksi |
| 13 | Date, Local Transaction | MMDD | Tanggal transaksi |
| 17 | Date, Capture | MMDD | Tanggal capture |
| 22 | Point of Service Entry Mode | `012` | Entry mode |
| 32 | Acquiring Institution ID | `1234567890` | ID institusi |
| 35 | Track 2 Data | AccountNumber | Track 2 data |
| 37 | Retrieval Reference Number | RRN | Nomor referensi |
| 41 | Card Acceptor Terminal ID | TerminalId | ID terminal |
| 42 | Card Acceptor Identification Code | `BANKINGAPPSYS` | Kode sistem |
| 43 | Card Acceptor Name/Location | `BANKING APPLICATION TERMINAL` | Nama terminal |
| 49 | Currency Code, Transaction | `360` | Kode mata uang (IDR) |
| 54 | Additional Amounts | `BAL:{newBalance}` | Saldo baru |
| 58 | From Account | Description | Deskripsi |
| 60 | Transaction Type | `TARIK` | Tipe transaksi |
| 61 | To Account | AccountId | ID rekening |

**Response Code:**
- `00` — Berhasil
- `05` — Saldo tidak cukup
- `51` — Transaksi ditolak

### 3. Transfer (Transfer Transaction)

**MTI:** `0200` (Request) → `0210` (Response)

**Data Elements:**
| Element | Name | Value | Description |
|---------|------|-------|-------------|
| 2 | Primary Account Number | FromAccountNumber | Nomor rekening pengirim |
| 3 | Processing Code | `400000` | Transfer |
| 4 | Amount, Transaction | Amount * 100 | Jumlah dalam cents |
| 5 | Amount, Settlement | Amount * 100 | Jumlah settlement |
| 6 | Amount, Cardholder Billing | `000000000000` | Nol untuk transfer |
| 7 | Transmission Date & Time | MMDDhhmmss | Waktu transmisi |
| 11 | System Trace Audit Number | STAN | Nomor urut transaksi |
| 12 | Time, Local Transaction | hhmmss | Waktu transaksi |
| 13 | Date, Local Transaction | MMDD | Tanggal transaksi |
| 17 | Date, Capture | MMDD | Tanggal capture |
| 22 | Point of Service Entry Mode | `012` | Entry mode |
| 32 | Acquiring Institution ID | `1234567890` | ID institusi |
| 33 | Forwarding Institution ID | `1234567890` | ID institusi penerus |
| 35 | Track 2 Data | FromAccountNumber | Track 2 data pengirim |
| 36 | Track 3 Data | ToAccountNumber | Track 3 data penerima |
| 37 | Retrieval Reference Number | RRN | Nomor referensi |
| 41 | Card Acceptor Terminal ID | TerminalId | ID terminal |
| 42 | Card Acceptor Identification Code | `BANKINGAPPSYS` | Kode sistem |
| 43 | Card Acceptor Name/Location | `BANKING APPLICATION TERMINAL` | Nama terminal |
| 49 | Currency Code, Transaction | `360` | Kode mata uang (IDR) |
| 54 | Additional Amounts | `FROM_BAL:{fromBalance}|TO_BAL:{toBalance}` | Saldo kedua rekening |
| 58 | From Account | Description | Deskripsi |
| 60 | Transaction Type | `TRANSFER` | Tipe transaksi |
| 61 | To Account | `{fromId}-{toId}` | ID rekening |
| 62 | Additional Data | ToAccountNumber | Data tambahan |

**Response Code:**
- `00` — Berhasil
- `05` — Saldo tidak cukup
- `12` — Transaksi invalid

### 4. Reversal

**MTI:** `0420` (Reversal Request) → `0430` (Reversal Response)

Digunakan untuk membatalkan transaksi sebelumnya.

| Element | Name | Value | Description |
|---------|------|-------|-------------|
| 3 | Processing Code | `000000` | Reversal |
| 7 | Transmission Date & Time | MMDDhhmmss | Waktu transmisi |
| 11 | System Trace Audit Number | New STAN | STAN baru untuk reversal |
| 12 | Time, Local Transaction | hhmmss | Waktu transaksi |
| 13 | Date, Local Transaction | MMDD | Tanggal transaksi |
| 17 | Date, Capture | MMDD | Tanggal capture |
| 37 | Retrieval Reference Number | New RRN | RRN baru untuk reversal |
| 38 | Approval Code | Original STAN | STAN transaksi asli |
| 41 | Card Acceptor Terminal ID | TerminalId | ID terminal |
| 42 | Card Acceptor Identification Code | `BANKINGAPPSYS` | Kode sistem |
| 90 | Original Data Elements | Original RRN | RRN transaksi asli |

### 5. Balance Inquiry (Non-Financial Transaction)

**MTI:** `0100` (Request) → `0110` (Response)

**Kategori:** Non-Financial / INQUIRY  
**OperationType:** BALANCE_INQUIRY  
**Balance Impact:** None (hanya inquiry)

| Element | Name | Request | Response |
|---------|------|---------|----------|
| 2 | Primary Account Number | AccountNumber | AccountNumber |
| 3 | Processing Code | `310000` | `310000` |
| 7 | Transmission Date & Time | MMDDhhmmss | MMDDhhmmss |
| 11 | System Trace Audit Number | STAN | STAN |
| 12 | Time, Local Transaction | hhmmss | hhmmss |
| 13 | Date, Local Transaction | MMDD | MMDD |
| 17 | Date, Capture | MMDD | MMDD |
| 22 | Point of Service Entry Mode | `012` | `012` |
| 32 | Acquiring Institution ID | `1234567890` | `1234567890` |
| 35 | Track 2 Data | AccountNumber | AccountNumber |
| 37 | Retrieval Reference Number | RRN | RRN |
| 41 | Card Acceptor Terminal ID | TerminalId | TerminalId |
| 42 | Card Acceptor Identification Code | `BANKINGAPPSYS` | `BANKINGAPPSYS` |
| 43 | Card Acceptor Name/Location | `BANKING APPLICATION TERMINAL` | `BANKING APPLICATION TERMINAL` |
| 49 | Currency Code, Transaction | `360` | `360` |
| 54 | Additional Amounts | - | `BAL:{balance}` |
| 60 | Transaction Type | `INQUIRY` | `INQUIRY` |
| 61 | Account Reference | AccountId | AccountId |
| 63 | Transaction Status | - | `SUCCESS` / `ERROR` |

**Response Code:**
- `00` — Approved/Found
- `40` — Request in progress
- `51` — Account not found

### 6. Account Opening (Non-Financial Transaction)

**MTI:** `0100` (Request) → `0110` (Response)

**Kategori:** ACCOUNT_OPENING  
**OperationType:** ACCOUNT_OPENING  
**Balance Impact:** 0 (initial balance)

| Element | Name | Request | Response |
|---------|------|---------|----------|
| 2 | Primary Account Number | AccountNumber | AccountNumber |
| 3 | Processing Code | `000000` | `000000` |
| 7 | Transmission Date & Time | MMDDhhmmss | MMDDhhmmss |
| 11 | System Trace Audit Number | STAN | STAN |
| 12 | Time, Local Transaction | hhmmss | hhmmss |
| 13 | Date, Local Transaction | MMDD | MMDD |
| 17 | Date, Capture | MMDD | MMDD |
| 22 | Point of Service Entry Mode | `012` | `012` |
| 32 | Acquiring Institution ID | `1234567890` | `1234567890` |
| 35 | Track 2 Data | AccountNumber | AccountNumber |
| 37 | Retrieval Reference Number | RRN | RRN |
| 41 | Card Acceptor Terminal ID | TerminalId | TerminalId |
| 42 | Card Acceptor Identification Code | `BANKINGAPPSYS` | `BANKINGAPPSYS` |
| 43 | Card Acceptor Name/Location | `BANKING APPLICATION TERMINAL` | `BANKING APPLICATION TERMINAL` |
| 49 | Currency Code, Transaction | `360` | `360` |
| 60 | Transaction Type | `ACCOUNT_OPENING` | `ACCOUNT_OPENING` |
| 61 | Account Reference | AccountId | AccountId |
| 63 | Transaction Status | `NAME:{name}|TYPE:{type}` | `SUCCESS` / `ERROR` |

**Response Code:**
- `00` — Approved/Success
- `05` — Do not honor
- `12` — Invalid transaction

### 7. Transaction History Inquiry (Non-Financial Transaction)

**MTI:** `0100` (Request) → `0110` (Response)

**Kategori:** INQUIRY  
**OperationType:** HISTORY_INQUIRY  
**Balance Impact:** None

| Element | Name | Request | Response |
|---------|------|---------|----------|
| 2 | Primary Account Number | AccountNumber | AccountNumber |
| 3 | Processing Code | `000000` | `000000` |
| 7 | Transmission Date & Time | MMDDhhmmss | MMDDhhmmss |
| 11 | System Trace Audit Number | STAN | STAN |
| 12 | Time, Local Transaction | hhmmss | hhmmss |
| 13 | Date, Local Transaction | MMDD | MMDD |
| 17 | Date, Capture | MMDD | MMDD |
| 22 | Point of Service Entry Mode | `012` | `012` |
| 32 | Acquiring Institution ID | `1234567890` | `1234567890` |
| 35 | Track 2 Data | AccountNumber | AccountNumber |
| 37 | Retrieval Reference Number | RRN | RRN |
| 41 | Card Acceptor Terminal ID | TerminalId | TerminalId |
| 42 | Card Acceptor Identification Code | `BANKINGAPPSYS` | `BANKINGAPPSYS` |
| 43 | Card Acceptor Name/Location | `BANKING APPLICATION TERMINAL` | `BANKING APPLICATION TERMINAL` |
| 49 | Currency Code, Transaction | `360` | `360` |
| 60 | Transaction Type | `HISTORY_INQUIRY` | `HISTORY_INQUIRY` |
| 61 | Account Reference | AccountId | AccountId |
| 62 | Additional Data | `FROM:{fromDate}|TO:{toDate}` | Transaction count |
| 63 | Transaction Status | - | `SUCCESS` / `ERROR` |

**Response Code:**
- `00` — Approved/Found
- `40` — Request in progress
- `51` — Account not found

---

## API Endpoints

### Base URL
```
http://localhost:5015
```

### 1. Simulate Setor (Deposit)

```
POST /api/iso8583/simulate-setor
```

**Request Body:**
```json
{
  "accountNumber": "1234567890",
  "amount": 50000,
  "description": "Setor Tunai"
}
```

**Response:**
```json
{
  "message": "Setor tunai berhasil",
  "accountNumber": "1234567890",
  "amount": 50000,
  "newBalance": 150000,
  "iso8583Request": "0200...",
  "iso8583Response": "0210...",
  "stan": "D9D246",
  "rrn": "F39DF0A1E437",
  "responseCode": "00",
  "timestamp": "2026-09-03T00:29:18.1041169+09:00"
}
```

### 2. Simulate Tarik (Withdraw)

```
POST /api/iso8583/simulate-tarik
```

**Request Body:**
```json
{
  "accountNumber": "1234567890",
  "amount": 20000,
  "description": "Tarik Tunai"
}
```

**Response:**
```json
{
  "message": "Tarik tunai berhasil",
  "accountNumber": "1234567890",
  "amount": 20000,
  "newBalance": 1130000,
  "iso8583Request": "0200...",
  "iso8583Response": "0210...",
  "stan": "AEFBD4",
  "rrn": "9A97687655A0",
  "responseCode": "00",
  "timestamp": "2026-09-03T00:29:40.987906+09:00"
}
```

### 3. Simulate Transfer

```
POST /api/iso8583/simulate-transfer
```

**Request Body:**
```json
{
  "fromAccountNumber": "1234567890",
  "toAccountNumber": "9876543210",
  "amount": 15000,
  "description": "Transfer ke Jane"
}
```

**Response:**
```json
{
  "message": "Transfer berhasil",
  "fromAccountNumber": "1234567890",
  "toAccountNumber": "9876543210",
  "amount": 15000,
  "fromNewBalance": 1115000,
  "toNewBalance": 65000,
  "iso8583Request": "0200...",
  "iso8583Response": "0210...",
  "stan": "763059",
  "rrn": "EE6BB4D3E5E4",
  "responseCode": "00",
  "timestamp": "2026-09-03T00:32:34.0345435+09:00"
}
```

### 4. Get Transactions

```
GET /api/iso8583/transactions?accountNumber=1234567890&page=1&pageSize=50
```

**Query Parameters:**
- `accountNumber` (optional) — Filter by account number
- `page` (optional, default: 1) — Page number
- `pageSize` (optional, default: 50) — Items per page

**Response:**
```json
{
  "total": 3,
  "page": 1,
  "pageSize": 50,
  "data": [
    {
      "id": 3,
      "accountNumber": "1234567890",
      "accountHolder": "John Doe",
      "type": "Tarik",
      "amount": 20000,
      "description": "Tarik Tunai",
      "reference": null,
      "stan": "AEFBD4",
      "rrn": "9A97687655A0",
      "responseCode": "00",
      "mti": "0200/0210",
      "iso8583Raw": "0210...",
      "date": "2026-09-03T00:29:41"
    }
  ]
}
```

### 5. Get Accounts

```
GET /api/iso8583/accounts
```

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "accountNumber": "1234567890",
      "accountHolder": "John Doe",
      "type": "Tab Savings",
      "balance": 1130000,
      "createdAt": "2026-09-03T00:28:14"
    }
  ]
}
```

### 6. Parse ISO 8583 Message

```
GET /api/iso8583/parse-message?message=0200...
```

**Response:**
```json
{
  "mti": "0200",
  "raw": "0200...",
  "elementCount": 20,
  "elements": {
    "2": "1234567890          ",
    "3": "310000",
    "4": "000005000000",
    ...
  }
}
```

### 7. Simulate Balance Inquiry

```
POST /api/iso8583/simulate-balance-inquiry
```

**Request Body:**
```json
{
  "accountNumber": "1234567890"
}
```

**Response:**
```json
{
  "message": "Balance inquiry successful",
  "accountNumber": "1234567890",
  "accountHolder": "John Doe",
  "balance": 150000,
  "currency": "IDR",
  "iso8583Request": "0100...",
  "iso8583Response": "0110...",
  "stan": "D7B1C8",
  "rrn": "106309E36809",
  "responseCode": "00",
  "timestamp": "2026-09-03T11:27:22.2289517+09:00"
}
```

### 8. Simulate Account Opening

```
POST /api/iso8583/simulate-account-opening
```

**Request Body:**
```json
{
  "accountNumber": "1122334455",
  "accountHolderName": "Alice Johnson",
  "accountType": "Giro"
}
```

**Response:**
```json
{
  "message": "Account opening successful",
  "accountNumber": "1122334455",
  "accountHolder": "Alice Johnson",
  "accountType": "Giro",
  "accountId": 2,
  "iso8583Request": "0100...",
  "iso8583Response": "0110...",
  "stan": "82FF3A",
  "rrn": "D0A048F7DD64",
  "responseCode": "00",
  "timestamp": "2026-09-03T11:27:51.7903868+09:00"
}
```

### 9. Simulate Transaction History

```
POST /api/iso8583/simulate-transaction-history
```

**Request Body:**
```json
{
  "accountNumber": "1234567890",
  "fromDate": "2026-08-01",
  "toDate": "2026-09-03",
  "pageSize": 50
}
```

**Response:**
```json
{
  "message": "Transaction history inquiry successful",
  "accountNumber": "1234567890",
  "accountHolder": "John Doe",
  "fromDate": "2026-08-04T00:00:00Z",
  "toDate": "2026-09-03T11:28:21Z",
  "totalTransactions": 1,
  "transactions": [
    {
      "id": 1,
      "type": "Cek Saldo",
      "amount": 0,
      "description": "Inquiry saldo via API",
      "date": "2026-09-03T11:27:22",
      "stan": "D7B1C8",
      "rrn": "106309E36809",
      "responseCode": "00",
      "mti": "0100/0110"
    }
  ],
  "iso8583Request": "0100...",
  "iso8583Response": "0110...",
  "stan": "960443",
  "rrn": "0D91E9754F13",
  "responseCode": "00",
  "timestamp": "2026-09-03T11:28:21.5097093+09:00"
}
```

### MVC Endpoints

| URL | Method | Deskripsi |
|-----|--------|-----------|
| `/` | GET | Daftar semua rekening |
| `/Account/Create` | GET | Form buka rekening |
| `/Account/Create` | POST | Proses buka rekening (ISO 8583 0100/0110) |
| `/Account/Deposit/{id}` | GET | Form setor tunai |
| `/Account/Deposit/{id}` | POST | Proses setor tunai (ISO 8583 0200/0210) |
| `/Account/Withdraw/{id}` | GET | Form tarik tunai |
| `/Account/Withdraw/{id}` | POST | Proses tarik tunai (ISO 8583 0200/0210) |
| `/Account/Transfer/{id}` | GET | Form transfer |
| `/Account/Transfer/{id}` | POST | Proses transfer (ISO 8583 0200/0210) |
| `/Account/BalanceInquiry/{id}` | GET | Form cek saldo |
| `/Account/BalanceInquiry/{id}` | POST | Proses cek saldo (ISO 8583 0100/0110) |
| `/Account/TransactionHistory/{id}` | GET | Form riwayat transaksi |
| `/Account/TransactionHistory/{id}` | POST | Proses riwayat transaksi (ISO 8583 0100/0110) |
| `/Account/History/{id}` | GET | Riwayat transaksi (legacy) |
| `/Account/Details/{id}` | GET | Detail rekening |

---

## Cara Menjalankan Aplikasi

### Prerequisites

1. **.NET 8.0 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **MySQL 8.4** (via Laragon) — Pastikan MySQL service berjalan
3. **Git** (opsional) — Untuk clone repository

### Langkah-langkah

#### 1. Clone atau Extract Project

```bash
git clone <repository-url>
cd aplikasibanking8583
```

Atau extract file project ke folder `C:\Users\naraya\Desktop\aplikasibanking8583`

#### 2. Pastikan MySQL Laragon Berjalan

1. Buka Laragon
2. Klik **Start** untuk menjalankan MySQL
3. Verifikasi MySQL berjalan di port 3306

#### 3. Konfigurasi Database

Edit file `appsettings.json` jika diperlukan:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=banking_db;User=root;Password=;"
  }
}
```

#### 4. Restore Dependencies

```bash
dotnet restore
```

#### 5. Build Project

```bash
dotnet build
```

#### 6. Jalankan Aplikasi

```bash
dotnet run
```

Aplikasi akan berjalan di: **http://localhost:5015**

#### 7. Akses Aplikasi

- **MVC UI:** http://localhost:5015
- **Swagger UI:** http://localhost:5015/swagger
- **API Base:** http://localhost:5015/api/iso8583

### Database Creation

Database dan tabel akan dibuat otomatis saat aplikasi pertama kali dijalankan melalui `Database.EnsureCreated()`.

Untuk membuat database manual:

```sql
CREATE DATABASE IF NOT EXISTS banking_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

---

## Testing

### Manual Testing via curl

#### 1. Buka Rekening Baru

```bash
curl -X POST -d "AccountNumber=1234567890&AccountHolderName=John Doe&AccountType=Tabungan" http://localhost:5015/Account/Create
```

#### 2. Setor Tunai via API

```bash
curl -X POST -H "Content-Type: application/json" -d "{\"accountNumber\":\"1234567890\",\"amount\":50000,\"description\":\"Setor Tunai\"}" http://localhost:5015/api/iso8583/simulate-setor
```

#### 3. Tarik Tunai via API

```bash
curl -X POST -H "Content-Type: application/json" -d "{\"accountNumber\":\"1234567890\",\"amount\":20000}" http://localhost:5015/api/iso8583/simulate-tarik
```

#### 4. Transfer via API

```bash
curl -X POST -H "Content-Type: application/json" -d "{\"fromAccountNumber\":\"1234567890\",\"toAccountNumber\":\"9876543210\",\"amount\":15000}" http://localhost:5015/api/iso8583/simulate-transfer
```

#### 5. Lihat Riwayat Transaksi

```bash
curl http://localhost:5015/Account/History/1
```

#### 6. Lihat Semua Transaksi via API

```bash
curl "http://localhost:5015/api/iso8583/transactions?accountNumber=1234567890"
```

#### 7. Cek Saldo via API (ISO 8583 0100/0110)

```bash
curl -X POST -H "Content-Type: application/json" -d "{\"accountNumber\":\"1234567890\"}" http://localhost:5015/api/iso8583/simulate-balance-inquiry
```

#### 8. Buka Rekening via API (ISO 8583 0100/0110)

```bash
curl -X POST -H "Content-Type: application/json" -d "{\"accountNumber\":\"1122334455\",\"accountHolderName\":\"Alice Johnson\",\"accountType\":\"Giro\"}" http://localhost:5015/api/iso8583/simulate-account-opening
```

#### 9. Riwayat Transaksi via API (ISO 8583 0100/0110)

```bash
curl -X POST -H "Content-Type: application/json" -d "{\"accountNumber\":\"1234567890\"}" http://localhost:5015/api/iso8583/simulate-transaction-history
```

### Testing via Swagger UI

1. Buka http://localhost:5015/swagger
2. Pilih endpoint yang ingin diuji
3. Klik **Try it out**
4. Isi parameter sesuai kebutuhan
5. Klik **Execute**

---

## Keamanan

### ISO 8583 Security Features

1. **STAN (System Trace Audit Number)**
   - Nomor urut unik untuk setiap transaksi
   - Digenerated menggunakan GUID untuk memastikan keunikan
   - Format: 6 karakter hexadecimal

2. **RRN (Retrieval Reference Number)**
   - Nomor referensi unik untuk setiap transaksi
   - Format: 12 karakter hexadecimal
   - Digunakan untuk tracking dan reversal

3. **Response Codes**
   - Standar ISO 8583 response codes
   - `00` — Approved/Completed
   - `05` — Do not honor/Saldo tidak cukup
   - `51` — Insufficient funds
   - `12` — Invalid transaction

4. **MTI (Message Type Indicator)**
   - Identifikasi tipe pesan ISO 8583
   - `0200` — Financial transaction request
   - `0210` — Financial transaction response
   - `0100` — Non-financial transaction request (Balance Inquiry, Account Opening, Transaction History)
   - `0110` — Non-financial transaction response
   - `0420` — Reversal request
   - `0430` — Reversal response

5. **Bitmap**
   - 128-bit bitmap untuk menunjukkan data elements yang aktif
   - Setiap bit menunjukkan presence/absence of data element

### Application Security

1. **Anti-Forgery Token** — Dinonaktifkan untuk API endpoints (untuk testing)
2. **Input Validation** — Semua input divalidasi sebelum diproses
3. **SQL Injection Protection** — Menggunakan EF Core parameterized queries
4. **Balance Validation** — Validasi saldo sebelum tarik/transfer

---

## Troubleshooting

### MySQL Tidak Bisa Connect

**Problem:** `Can't connect to MySQL server on 'localhost:3306'`

**Solution:**
1. Buka Laragon
2. Pastikan MySQL service berjalan (status: Running)
3. Cek port MySQL di Laragon Settings → Port: 3306

### Build Error: File Locked

**Problem:** `Could not copy ... The process cannot access the file because it is being used by another process`

**Solution:**
1. Stop aplikasi yang sedang running (Ctrl+C atau stop di IDE)
2. Tutup semua instance dotnet process di Task Manager
3. Build ulang: `dotnet build`

### Database Table Error

**Problem:** `Data too long for column 'MTI'`

**Solution:**
```bash
mysql -u root -e "ALTER TABLE banking_db.transactions MODIFY COLUMN MTI varchar(20) NOT NULL;"
```

### Port 5015 Sudah Digunakan

**Problem:** `Address already in use`

**Solution:**
1. Cek proses yang menggunakan port: `netstat -ano | findstr :5015`
2. Kill proses: `taskkill /PID <pid> /F`
3. Atau ubah port di `launchSettings.json`

---

## Kategori dan Jenis Operasi Transaksi

### TransactionCategory

| Kategori | Deskripsi | Contoh |
|----------|-----------|--------|
| `FINANCIAL` | Transaksi finansial yang mengubah saldo | Setor, Tarik, Transfer |
| `INQUIRY` | Operasi inquiry tanpa perubahan saldo | Cek Saldo, Riwayat Transaksi |
| `ACCOUNT_OPENING` | Pembukaan akun baru | Buka Rekening |

### OperationType

| OperationType | TransactionType | MTI | Kategori |
|---------------|-----------------|-----|----------|
| `SETOR` | Setor | 0200/0210 | FINANCIAL |
| `TARIK` | Tarik | 0200/0210 | FINANCIAL |
| `TRANSFER` | Transfer Keluar/Masuk | 0200/0210 | FINANCIAL |
| `BALANCE_INQUIRY` | Cek Saldo | 0100/0110 | INQUIRY |
| `HISTORY_INQUIRY` | Riwayat Transaksi | 0100/0110 | INQUIRY |
| `ACCOUNT_OPENING` | Pembukaan Akun | 0100/0110 | ACCOUNT_OPENING |

---

## Developer

Dikembangkan menggunakan ASP.NET Core 8.0 dengan arsitektur MVC dan integrasi ISO 8583 financial transaction messaging standard.

---

## Changelog

### Version 2.0 (2026-09-03)
- Menambahkan ISO 8583 untuk semua operasi perbankan
- Operasi non-financial sekarang menggunakan MTI 0100/0110
- Field database baru: TransactionCategory, OperationType, ResponseMessage
- API simulator baru: simulate-balance-inquiry, simulate-account-opening, simulate-transaction-history
- MVC views baru: BalanceInquiry, TransactionHistory

### Version 1.0 (2026-09-02)
- Aplikasi banking dasar dengan ISO 8583 untuk transaksi finansial
- Fitur: Setor, Tarik, Transfer, Buka Rekening
- MTI: 0200/0210 untuk financial transactions

---

## Lisensi

MIT License
