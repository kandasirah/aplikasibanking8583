# ISO 8583 Architecture Design Document

## 1. Executive Summary

Dokumen ini menjelaskan rancangan integrasi ISO 8583 financial transaction messaging standard ke seluruh operasi banking application. Saat ini, hanya transaksi keuangan (setor, tarik, transfer) yang menghasilkan pesan ISO 8583, sementara operasi lain seperti **balance inquiry** tidak melalui mekanisme ISO 8583.

Tujuan redesign ini adalah memastikan **setiap operasi perbankan** menghasilkan pesan ISO 8583 lengkap untuk compliance, audit trail, dan keamanan.

---

## 2. Current State Analysis

### 2.1 Operasi yang Sudah ISO 8583
- Setor Tunai (Deposit) — MTI 0200/0210
- Tarik Tunai (Withdraw) — MTI 0200/0210
- Transfer — MTI 0200/0210

### 2.2 Operasi yang Belum ISO 8583
- **Balance Inquiry (Cek Saldo)** — Sekarang hanya query database tanpa ISO 8583 message
- **Account Opening (Buka Rekening)** — Hanya insert database
- **Account Detail View** — Hanya query database
- **Transaction History View** — Hanya query database

### 2.3 Issues Identified
1. Balance inquiry tidak memiliki audit trail ISO 8583
2. Tidak ada response code untuk operasi non-financial
3. Tidak ada STAN/RRN untuk balance inquiry
4. Tidak ada financial transaction record untuk cek saldo

---

## 3. Proposed ISO 8583 Message Flow

### 3.1 Financial Transactions (Existing)

```
Client Request → Controller → ISO8583 Builder → Database Update → ISO8583 Response → Database Record
```

**Operasi:**
- Setor Tunai
- Tarik Tunai
- Transfer

**Message Flow:**
```
0200 (Request) → Processing → 0210 (Response)
```

### 3.2 Non-Financial Transactions (New)

#### Balance Inquiry (Cek Saldo)

**MTI:** `0100` (Request) → `0110` (Response)

**Message Flow:**
```
Client Request → Controller → ISO8583 Builder → Database Query → ISO8583 Response → Database Record
0100 (Request) → Processing → 0110 (Response)
```

**Key Difference:** Tidak ada update balance, hanya inquiry.

---

## 4. ISO 8583 Message Specifications

### 4.1 Financial Transaction Messages

#### 4.1.1 Setor Tunai (Deposit)

| Aspect | Value |
|--------|-------|
| **MTI Request** | `0200` |
| **MTI Response** | `0210` |
| **Processing Code** | `310000` |
| **Transaction Type** | Credit to account |
| **Balance Impact** | +Amount |

**Data Elements:**

| Bit | Name | Request | Response |
|-----|------|---------|----------|
| 2 | Primary Account Number | AccountNumber | AccountNumber |
| 3 | Processing Code | `310000` | `310000` |
| 4 | Amount, Transaction | Amount * 100 | Amount * 100 |
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
| 42 | Card Acceptor ID Code | `BANKINGAPPSYS` | `BANKINGAPPSYS` |
| 43 | Card Acceptor Name | `BANKING APP` | `BANKING APP` |
| 49 | Currency Code | `360` | `360` |
| 54 | Additional Amounts | - | `BAL:{newBalance}` |
| 58 | From Account | Description | Description |
| 60 | Transaction Type | `SETOR` | `SETOR` |
| 61 | To Account | AccountId | AccountId |
| 63 | Transaction Status | - | `APPROVED` / `DECLINED` |

**Response Codes:**
- `00` — Approved
- `05` — Do not honor
- `51` — Insufficient funds

#### 4.1.2 Tarik Tunai (Withdraw)

| Aspect | Value |
|--------|-------|
| **MTI Request** | `0200` |
| **MTI Response** | `0210` |
| **Processing Code** | `010000` |
| **Transaction Type** | Debit from account |
| **Balance Impact** | -Amount |

**Data Elements:** Same as Setor, except Processing Code = `010000` and Transaction Type = `TARIK`

#### 4.1.3 Transfer

| Aspect | Value |
|--------|-------|
| **MTI Request** | `0200` |
| **MTI Response** | `0210` |
| **Processing Code** | `400000` |
| **Transaction Type** | Transfer |
| **Balance Impact** | -Amount (from), +Amount (to) |

**Data Elements:**
- Same as Setor/Tarik
- Element 5: Amount Settlement = Amount * 100
- Element 6: Amount Cardholder Billing = `000000000000`
- Element 33: Forwarding Institution ID = `1234567890`
- Element 36: Track 3 Data = ToAccountNumber
- Element 62: Additional Data = ToAccountNumber

#### 4.1.4 Reversal

| Aspect | Value |
|--------|-------|
| **MTI Request** | `0420` |
| **MTI Response** | `0430` |
| **Processing Code** | `000000` |
| **Transaction Type** | Reversal |
| **Balance Impact** | Reverse original transaction |

---

### 4.2 Non-Financial Transaction Messages (NEW)

#### 4.2.1 Balance Inquiry (Cek Saldo)

| Aspect | Value |
|--------|-------|
| **MTI Request** | `0100` |
| **MTI Response** | `0110` |
| **Processing Code** | `310000` |
| **Transaction Type** | Inquiry |
| **Balance Impact** | None |

**Data Elements:**

| Bit | Name | Request | Response |
|-----|------|---------|----------|
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
| 42 | Card Acceptor ID Code | `BANKINGAPPSYS` | `BANKINGAPPSYS` |
| 43 | Card Acceptor Name | `BANKING APP` | `BANKING APP` |
| 49 | Currency Code | `360` | `360` |
| 54 | Additional Amounts | - | `BAL:{balance}` |
| 60 | Transaction Type | `INQUIRY` | `INQUIRY` |
| 61 | Account Reference | AccountId | AccountId |
| 63 | Transaction Status | - | `SUCCESS` / `ERROR` |

**Response Codes:**
- `00` — Approved/Found
- `40` — Request in progress
- `51` — Account not found

#### 4.2.2 Account Opening (NEW)

| Aspect | Value |
|--------|-------|
| **MTI Request** | `0100` |
| **MTI Response** | `0110` |
| **Processing Code** | `000000` |
| **Transaction Type** | Account Opening |
| **Balance Impact** | 0 (initial) |

**Data Elements:**
- Element 2: AccountNumber
- Element 3: Processing Code = `000000`
- Element 7: Transmission Date & Time
- Element 11: STAN
- Element 12: Time
- Element 13: Date
- Element 17: Date
- Element 22: Entry Mode = `012`
- Element 32: Acquiring Institution ID
- Element 37: RRN
- Element 41: Terminal ID
- Element 42: Acceptor ID
- Element 43: Acceptor Name
- Element 49: Currency = `360`
- Element 60: Transaction Type = `ACCOUNT_OPENING`
- Element 61: AccountId
- Element 63: Status = `SUCCESS` / `ERROR`

#### 4.2.3 Transaction History Inquiry (NEW)

| Aspect | Value |
|--------|-------|
| **MTI Request** | `0100` |
| **MTI Response** | `0110` |
| **Processing Code** | `000000` |
| **Transaction Type** | History Inquiry |

**Data Elements:**
- Element 2: AccountNumber
- Element 3: Processing Code = `000000`
- Element 7: Transmission Date & Time
- Element 11: STAN
- Element 12: Time
- Element 13: Date
- Element 17: Date
- Element 22: Entry Mode = `012`
- Element 32: Acquiring Institution ID
- Element 37: RRN
- Element 41: Terminal ID
- Element 42: Acceptor ID
- Element 43: Acceptor Name
- Element 49: Currency = `360`
- Element 60: Transaction Type = `HISTORY_INQUIRY`
- Element 61: AccountId
- Element 62: Date Range (from-to)
- Element 63: Status = `SUCCESS` / `ERROR`

---

## 5. Database Schema Changes

### 5.1 New Fields in transactions Table

```sql
ALTER TABLE transactions ADD COLUMN TransactionCategory VARCHAR(20) NOT NULL DEFAULT 'FINANCIAL';
-- Values: 'FINANCIAL', 'INQUIRY', 'ACCOUNT_OPENING'

ALTER TABLE transactions ADD COLUMN OperationType VARCHAR(50) NOT NULL DEFAULT 'UNKNOWN';
-- Values: 'SETOR', 'TARIK', 'TRANSFER', 'REVERSAL', 'BALANCE_INQUIRY', 'ACCOUNT_OPENING', 'HISTORY_INQUIRY'

ALTER TABLE transactions ADD COLUMN ResponseMessage VARCHAR(200) NULL;
-- Human-readable response message
```

### 5.2 New Table: iso8583_audit_log (Optional)

For comprehensive audit trail:

```sql
CREATE TABLE iso8583_audit_log (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    MessageType VARCHAR(10) NOT NULL, -- '0200', '0210', '0100', '0110', etc.
    Direction VARCHAR(10) NOT NULL, -- 'INBOUND', 'OUTBOUND'
    RawMessage TEXT NOT NULL,
    ParsedElements JSON NULL,
    STAN VARCHAR(6) NULL,
    RRN VARCHAR(12) NULL,
    AccountNumber VARCHAR(20) NULL,
    Amount DECIMAL(18,2) NULL,
    ResponseCode VARCHAR(4) NULL,
    ProcessingCode VARCHAR(6) NULL,
    TransactionType VARCHAR(50) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_stan (STAN),
    INDEX idx_rrn (RRN),
    INDEX idx_account (AccountNumber),
    INDEX idx_created (CreatedAt)
);
```

---

## 6. Revised Controller Architecture

### 6.1 AccountController Changes

```
AccountController
├── Index()                    → ISO 8583 Inquiry (0100/0110) + List View
├── Create()                   → ISO 8583 Account Opening (0100/0110)
├── Deposit()                  → ISO 8583 Financial (0200/0210)
├── Withdraw()                 → ISO 8583 Financial (0200/0210)
├── Transfer()                 → ISO 8583 Financial (0200/0210)
├── BalanceInquiry()          → ISO 8583 Non-Financial (0100/0110) [NEW]
├── TransactionHistory()      → ISO 8583 Non-Financial (0100/0110) [NEW]
└── Details()                  → ISO 8583 Inquiry (0100/0110)
```

### 6.2 New Method: Balance Inquiry

```csharp
[HttpGet("BalanceInquiry/{id}")]
public async Task<IActionResult> BalanceInquiry(int? id)
{
    // 1. Generate STAN
    var stan = Iso8583TransactionBuilder.GenerateSTAN();
    
    // 2. Build ISO 8583 Request (0100)
    var isoRequest = Iso8583TransactionBuilder.BuildBalanceInquiryMessage(account, stan, TerminalId);
    
    // 3. Query database (no balance change)
    var account = await _context.Accounts.FindAsync(id);
    
    // 4. Build ISO 8583 Response (0110)
    var isoResponse = Iso8583ResponseBuilder.BuildBalanceInquiryResponse(isoRequest, account);
    
    // 5. Record transaction (non-financial)
    var transaction = new Transaction
    {
        AccountId = account.Id,
        TransactionType = "Balance Inquiry",
        Amount = 0,
        Description = "Cek Saldo",
        TransactionDate = DateTime.Now,
        Stan = stan,
        Rrn = isoResponse.GetString(37),
        ResponseCode = "00",
        MTI = "0100/0110",
        Iso8583MessageRaw = isoResponse.ToIso8583String(),
        TransactionCategory = "INQUIRY",
        OperationType = "BALANCE_INQUIRY",
        ResponseMessage = $"Saldo: {account.Balance:C}"
    };
    
    // 6. Save to database
    _context.Transactions.Add(transaction);
    await _context.SaveChangesAsync();
    
    // 7. Return view with ISO 8583 data
    ViewBag.Iso8583Data = new { isoRequest, isoResponse, stan, rrn };
    return View(account);
}
```

### 6.3 ISO 8583 Builder Changes

```csharp
public static class Iso8583TransactionBuilder
{
    // Existing methods...
    public static Iso8583Message BuildSetorMessage(...);
    public static Iso8583Message BuildTarikMessage(...);
    public static Iso8583Message BuildTransferMessage(...);
    
    // NEW methods...
    public static Iso8583Message BuildBalanceInquiryMessage(Account account, string stan, string terminalId);
    public static Iso8583Message BuildAccountOpeningMessage(Account account, string stan, string terminalId);
    public static Iso8583Message BuildTransactionHistoryMessage(Account account, DateTime from, DateTime to, string stan, string terminalId);
}
```

### 6.4 ISO 8583 Response Builder Changes

```csharp
public static class Iso8583ResponseBuilder
{
    // Existing methods...
    public static Iso8583Message BuildSetorResponse(...);
    public static Iso8583Message BuildTarikResponse(...);
    public static Iso8583Message BuildTransferResponse(...);
    
    // NEW methods...
    public static Iso8583Message BuildBalanceInquiryResponse(Iso8583Message request, Account account);
    public static Iso8583Message BuildAccountOpeningResponse(Iso8583Message request, Account account);
    public static Iso8583Message BuildTransactionHistoryResponse(Iso8583Message request, List<Transaction> transactions);
}
```

---

## 7. Message Flow Diagrams

### 7.1 Financial Transaction Flow (Existing)

```
┌──────┐     ┌──────────┐     ┌─────────────┐     ┌──────────────┐     ┌──────────┐
│Client│────▶│Controller│────▶│ISO8583      │────▶│Database      │────▶│ISO8583   │
│      │     │          │     │Builder      │     │Update        │     │Response  │
└──────┘     └──────────┘     └─────────────┘     └──────────────┘     └──────────┘
                                │                     │                    │
                                │ Build 0200          │ Save changes        │ Build 0210
                                │                     │                    │
                                └─────────────────────┴────────────────────┘
                                                      │
                                                      ▼
                                               ┌──────────────┐
                                               │Transaction   │
                                               │Record        │
                                               │(with ISO8583 │
                                               │ raw message) │
                                               └──────────────┘
```

### 7.2 Non-Financial Transaction Flow (NEW)

```
┌──────┐     ┌──────────┐     ┌─────────────┐     ┌──────────────┐     ┌──────────┐
│Client│────▶│Controller│────▶│ISO8583      │────▶│Database      │────▶│ISO8583   │
│      │     │          │     │Builder      │     │Query         │     │Response  │
└──────┘     └──────────┘     └─────────────┘     └──────────────┘     └──────────┘
                                │                     │                    │
                                │ Build 0100          │ Read data           │ Build 0110
                                │                     │                    │
                                └─────────────────────┴────────────────────┘
                                                      │
                                                      ▼
                                               ┌──────────────┐
                                               │Transaction   │
                                               │Record        │
                                               │(with ISO8583 │
                                               │ raw message) │
                                               └──────────────┘
```

### 7.3 Balance Inquiry Detailed Flow

```
1. User requests balance inquiry for Account 1234567890
2. Controller generates STAN: "A1B2C3"
3. Build ISO 8583 Request (0100):
   - MTI: 0100
   - Bit 2: 1234567890
   - Bit 3: 310000
   - Bit 11: A1B2C3
   - Bit 37: RRN1234567890
   - ... other elements
4. Query database: SELECT * FROM accounts WHERE AccountNumber = '1234567890'
5. Build ISO 8583 Response (0110):
   - MTI: 0110
   - Bit 2: 1234567890
   - Bit 39: 00 (Approved)
   - Bit 54: BAL:150000.00
   - ... other elements
6. Record transaction:
   - TransactionType: "Balance Inquiry"
   - Amount: 0
   - STAN: A1B2C3
   - RRN: RRN1234567890
   - ResponseCode: 00
   - MTI: 0100/0110
   - Iso8583MessageRaw: [full 0110 message]
7. Return view with balance and ISO 8583 details
```

---

## 8. Security & Compliance

### 8.1 Audit Trail
- Setiap operasi menghasilkan pesan ISO 8583 yang disimpan
- STAN dan RRN untuk tracking
- Timestamp untuk setiap transaksi

### 8.2 Response Codes
- Semua operasi memiliki response code
- Response message untuk human-readable feedback

### 8.3 Data Integrity
- Hash atau checksum untuk pesan ISO 8583 (optional)
- Sequence number untuk mendeteksi replay attacks

### 8.4 Access Control
- Semua endpoint memerlukan authentication (jika ditambahkan)
- Rate limiting untuk API endpoints

---

## 9. API Changes

### 9.1 New Endpoints

```
POST /api/iso8583/simulate-balance-inquiry
POST /api/iso8583/simulate-account-opening
POST /api/iso8583/simulate-transaction-history
```

### 9.2 Modified Endpoints

```
GET /api/iso8583/transactions
- Add filter by transactionCategory
- Add filter by operationType
```

### 9.3 Request/Response Examples

#### Balance Inquiry Request
```json
{
  "accountNumber": "1234567890"
}
```

#### Balance Inquiry Response
```json
{
  "message": "Balance inquiry successful",
  "accountNumber": "1234567890",
  "accountHolder": "John Doe",
  "balance": 150000,
  "currency": "IDR",
  "iso8583Request": "0100...",
  "iso8583Response": "0110...",
  "stan": "A1B2C3",
  "rrn": "RRN1234567890",
  "responseCode": "00",
  "timestamp": "2026-09-03T02:00:00Z"
}
```

---

## 10. Implementation Plan

### Phase 1: Core ISO 8583 Enhancement (Week 1)
1. Add new fields to Transaction model
   - TransactionCategory
   - OperationType
   - ResponseMessage
2. Update Database Context
3. Create database migration

### Phase 2: ISO 8583 Builder Extension (Week 1)
1. Add BuildBalanceInquiryMessage()
2. Add BuildAccountOpeningMessage()
3. Add BuildTransactionHistoryMessage()
4. Add corresponding response builders

### Phase 3: Controller Updates (Week 2)
1. Modify AccountController
   - Add BalanceInquiry() action
   - Add TransactionHistory() action
   - Update Create() to use ISO 8583
2. Update Iso8583SimulatorController
   - Add new API endpoints

### Phase 4: View Updates (Week 2)
1. Create BalanceInquiry.cshtml
2. Update History.cshtml to show ISO 8583 details
3. Update Index.cshtml to show inquiry button

### Phase 5: Testing (Week 3)
1. Unit tests for new ISO 8583 builders
2. Integration tests for balance inquiry
3. API testing via Swagger
4. Database audit verification

### Phase 6: Documentation (Week 3)
1. Update README.md
2. Add API documentation
3. Add deployment guide

---

## 11. Benefits

### 11.1 Compliance
- Full audit trail for all operations
- ISO 8583 standard compliance
- Regulatory reporting ready

### 11.2 Security
- STAN/RRN for transaction tracking
- Response codes for all operations
- Message integrity verification

### 11.3 Monitoring
- Real-time transaction monitoring
- Anomaly detection
- Performance metrics

### 11.4 Interoperability
- Standard message format
- Easy integration with external systems
- Support for various payment networks

---

## 12. Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Performance overhead from ISO 8583 processing | Medium | Async processing, caching |
| Database schema changes | Low | Migration scripts, backward compatibility |
| Increased message size | Low | Compression, selective elements |
| Complex debugging | Medium | Comprehensive logging, message viewer |

---

## 13. Conclusion

Rancangan ini memastikan bahwa **setiap operasi perbankan** melalui mekanisme ISO 8583, termasuk:
- Balance Inquiry (0100/0110)
- Account Opening (0100/0110)
- Transaction History (0100/0110)
- Financial Transactions (0200/0210)
- Reversals (0420/0430)

Dengan pendekatan ini, aplikasi banking memiliki:
1. **Full audit trail** untuk semua operasi
2. **ISO 8583 compliance** untuk regulatory requirements
3. **Enhanced security** dengan STAN, RRN, dan response codes
4. **Better monitoring** dan troubleshooting

---

## 14. Approval Required

Sebelum implementasi dimulai, diperlukan approval untuk:
1. ✅ Rancangan arsitektur ISO 8583
2. ✅ Database schema changes
3. ✅ API endpoint additions
4. ✅ UI/UX changes untuk balance inquiry
5. ✅ Timeline 3 minggu untuk implementasi lengkap

---

**Document Version:** 1.0  
**Date:** 2026-09-03  
**Author:** Kilo Engineering Team  
**Status:** Pending Approval
