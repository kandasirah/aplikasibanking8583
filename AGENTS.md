# AGENTS.md - AplikasiBanking8583 Development Guide

## Overview
ASP.NET Core 8.0 MVC banking application with ISO 8583 financial transaction messaging standard.
Supports: Setor (Deposit), Tarik (Withdraw), Transfer, Balance Inquiry, Account Opening, Transaction History.

## Build Commands
```bash
dotnet build        # Build the project
dotnet run          # Run locally (http://localhost:5015)
dotnet test         # Run tests (no test project yet)
```

## Project Structure
```
/Controllers/             # MVC + API controllers
  /AccountController.cs   # Banking operations (Create, Deposit, Withdraw, Transfer, BalanceInquiry, etc.)
  /HomeController.cs
  /ISO8583/Iso8583SimulatorController.cs  # REST API for ISO 8583 simulation
/Models/
  Account.cs              # Account entity (Id, AccountNumber, Holder, Balance, Type)
  Transaction.cs          # Transaction entity (with STAN, RRN, MTI, ISO8583MessageRaw, etc.)
  BankingDbContext.cs     # EF Core DbContext (MySQL)
  /ISO8583/
    Iso8583Message.cs     # Core ISO 8583 message class (MTI, Bitmap, Data Elements)
    Iso8583TransactionBuilder.cs  # Build 0100/0200 requests
    Iso8583ResponseBuilder.cs     # Build 0110/0210 responses
/Views/                   # Razor views (MVC UI)
/wwwroot/                 # Static assets
/docs/                    # Architecture/design docs
```

## ISO 8583 Conventions
- **Financial transactions**: MTI 0200 (request) → 0210 (response) — Setor, Tarik, Transfer
- **Non-financial transactions**: MTI 0100 (request) → 0110 (response) — Balance Inquiry, Account Opening, Transaction History
- **Reversal**: MTI 0420 (request) → 0430 (response)
- STAN: 6-char hex (from `Iso8583TransactionBuilder.GenerateSTAN()`)
- RRN: 12-char hex (from `Guid.NewGuid()`)
- Response codes: `00` (Approved), `05` (Do not honor), `51` (Insufficient funds), `12` (Invalid transaction)

## Coding Standards
- C# with nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Implicit usings enabled
- Controller methods return `Task<IActionResult>` for async
- Transaction records always include full ISO 8583 raw message in `Iso8583MessageRaw` field
- Anti-forgery token disabled on POST API endpoints (`[IgnoreAntiforgeryToken]`)
- Database uses `Database.EnsureCreated()` (no EF migrations yet)

## Git Conventions
- Branch: `main`
- Remote: `https://github.com/kandasirah/aplikasibanking8583.git`
- User: kandasirah / kandasirojulahmad@outlook.com
- Commit messages: short descriptive format (e.g., "iso builder", "rilis")

## Daily Update Routine
1. Check build passes: `dotnet build`
2. Run daily automation script: `bash scripts/daily-update.sh` (if modified)
3. Push to GitHub: `git add . && git commit -m "daily update" && git push origin main`
4. GitHub Actions CI runs automatically on push

## Prerequisites
- .NET 8.0 SDK
- MySQL 8.x (via Laragon recommended, port 3306)
- Git
