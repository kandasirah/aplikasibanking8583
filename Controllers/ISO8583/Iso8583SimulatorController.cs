using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplikasiBanking8583.Models;
using AplikasiBanking8583.Models.ISO8583;
using System.Text;

namespace AplikasiBanking8583.Controllers.ISO8583
{
    [ApiController]
    [Route("api/iso8583")]
    public class Iso8583SimulatorController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private const string TerminalId = "TERMINAL01";

        public Iso8583SimulatorController(BankingDbContext context)
        {
            _context = context;
        }

        [HttpPost("simulate-setor")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> SimulateSetor([FromBody] SimulateSetorRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "Jumlah harus lebih dari 0" });
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);
            if (account == null)
            {
                return NotFound(new { error = "Rekening tidak ditemukan" });
            }

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildSetorMessage(account, request.Amount, request.Description ?? "Setor Tunai", stan, TerminalId);
            var newBalance = account.Balance + request.Amount;
            var isoResponse = Iso8583ResponseBuilder.BuildSetorResponse(isoRequest, true, "00", account.AccountNumber, newBalance);

            account.Balance += request.Amount;

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = "Setor",
                Amount = request.Amount,
                Description = request.Description ?? "Setor Tunai",
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0200/0210",
                Iso8583MessageRaw = isoResponse.ToIso8583String()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Setor tunai berhasil",
                accountNumber = account.AccountNumber,
                amount = request.Amount,
                newBalance = account.Balance,
                iso8583Request = isoRequest.ToIso8583String(),
                iso8583Response = isoResponse.ToIso8583String(),
                stan = stan,
                rrn = isoResponse.GetString(37),
                responseCode = "00",
                timestamp = DateTime.Now
            });
        }

        [HttpPost("simulate-tarik")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> SimulateTarik([FromBody] SimulateTarikRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "Jumlah harus lebih dari 0" });
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);
            if (account == null)
            {
                return NotFound(new { error = "Rekening tidak ditemukan" });
            }

            if (account.Balance < request.Amount)
            {
                return BadRequest(new { error = "Saldo tidak mencukupi", currentBalance = account.Balance });
            }

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildTarikMessage(account, request.Amount, request.Description ?? "Tarik Tunai", stan, TerminalId);
            var newBalance = account.Balance - request.Amount;
            var isoResponse = Iso8583ResponseBuilder.BuildTarikResponse(isoRequest, true, "00", account.AccountNumber, newBalance);

            account.Balance -= request.Amount;

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = "Tarik",
                Amount = request.Amount,
                Description = request.Description ?? "Tarik Tunai",
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0200/0210",
                Iso8583MessageRaw = isoResponse.ToIso8583String()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tarik tunai berhasil",
                accountNumber = account.AccountNumber,
                amount = request.Amount,
                newBalance = account.Balance,
                iso8583Request = isoRequest.ToIso8583String(),
                iso8583Response = isoResponse.ToIso8583String(),
                stan = stan,
                rrn = isoResponse.GetString(37),
                responseCode = "00",
                timestamp = DateTime.Now
            });
        }

        [HttpPost("simulate-transfer")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> SimulateTransfer([FromBody] SimulateTransferRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "Jumlah harus lebih dari 0" });
            }

            var fromAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == request.FromAccountNumber);
            var toAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == request.ToAccountNumber);

            if (fromAccount == null || toAccount == null)
            {
                return NotFound(new { error = "Rekening tidak ditemukan", fromFound = fromAccount != null, toFound = toAccount != null });
            }

            if (request.FromAccountNumber == request.ToAccountNumber)
            {
                return BadRequest(new { error = "Tidak bisa transfer ke rekening sendiri" });
            }

            if (fromAccount.Balance < request.Amount)
            {
                return BadRequest(new { error = "Saldo tidak mencukupi", currentBalance = fromAccount.Balance });
            }

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildTransferMessage(fromAccount, toAccount, request.Amount, request.Description ?? "Transfer", stan, TerminalId);
            var isoResponse = Iso8583ResponseBuilder.BuildTransferResponse(isoRequest, true, "00", fromAccount.AccountNumber, toAccount.AccountNumber, fromAccount.Balance - request.Amount, toAccount.Balance + request.Amount);

            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            var outTransaction = new Transaction
            {
                AccountId = fromAccount.Id,
                TransactionType = "Transfer Keluar",
                Amount = request.Amount,
                Description = $"Transfer ke {toAccount.AccountNumber} - {toAccount.AccountHolderName}. " + (request.Description ?? ""),
                Reference = isoResponse.GetString(37),
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0200/0210",
                Iso8583MessageRaw = isoResponse.ToIso8583String()
            };

            var inTransaction = new Transaction
            {
                AccountId = toAccount.Id,
                TransactionType = "Transfer Masuk",
                Amount = request.Amount,
                Description = $"Transfer dari {fromAccount.AccountNumber} - {fromAccount.AccountHolderName}. " + (request.Description ?? ""),
                Reference = isoResponse.GetString(37),
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0200/0210",
                Iso8583MessageRaw = isoResponse.ToIso8583String()
            };

            _context.Transactions.Add(outTransaction);
            _context.Transactions.Add(inTransaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Transfer berhasil",
                fromAccountNumber = fromAccount.AccountNumber,
                toAccountNumber = toAccount.AccountNumber,
                amount = request.Amount,
                fromNewBalance = fromAccount.Balance,
                toNewBalance = toAccount.Balance,
                iso8583Request = isoRequest.ToIso8583String(),
                iso8583Response = isoResponse.ToIso8583String(),
                stan = stan,
                rrn = isoResponse.GetString(37),
                responseCode = "00",
                timestamp = DateTime.Now
            });
        }

        [HttpGet("transactions")]
        [ProducesResponseType(typeof(List<object>), 200)]
        public async Task<IActionResult> GetTransactions([FromQuery] string? accountNumber, [FromQuery] int? page = 1, [FromQuery] int? pageSize = 50)
        {
            var query = _context.Transactions.Include(t => t.Account).AsQueryable();

            if (!string.IsNullOrEmpty(accountNumber))
            {
                query = query.Where(t => t.Account!.AccountNumber == accountNumber);
            }

            var total = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page.GetValueOrDefault(1) - 1) * pageSize.GetValueOrDefault(50))
                .Take(pageSize.GetValueOrDefault(50))
                .Select(t => new
                {
                    id = t.Id,
                    accountNumber = t.Account!.AccountNumber,
                    accountHolder = t.Account!.AccountHolderName,
                    type = t.TransactionType,
                    amount = t.Amount,
                    description = t.Description,
                    reference = t.Reference,
                    stan = t.Stan,
                    rrn = t.Rrn,
                    responseCode = t.ResponseCode,
                    mti = t.MTI,
                    iso8583Raw = t.Iso8583MessageRaw,
                    date = t.TransactionDate
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page = page.GetValueOrDefault(1),
                pageSize = pageSize.GetValueOrDefault(50),
                data = transactions
            });
        }

        [HttpGet("accounts")]
        [ProducesResponseType(typeof(List<object>), 200)]
        public async Task<IActionResult> GetAccounts()
        {
            var accounts = await _context.Accounts
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    id = a.Id,
                    accountNumber = a.AccountNumber,
                    accountHolder = a.AccountHolderName,
                    type = a.AccountType,
                    balance = a.Balance,
                    createdAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new { data = accounts });
        }

        [HttpGet("parse-message")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public IActionResult ParseIso8583Message([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest(new { error = "Message parameter is required" });
            }

            try
            {
                var isoMessage = Iso8583Message.Parse(message);
                var elements = isoMessage.DataElements.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return Ok(new
                {
                    mti = isoMessage.MTI,
                    raw = message,
                    elementCount = elements.Count,
                    elements = elements
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to parse ISO 8583 message", details = ex.Message });
            }
        }

        [HttpPost("simulate-balance-inquiry")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> SimulateBalanceInquiry([FromBody] SimulateBalanceInquiryRequest request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);
            if (account == null)
            {
                return NotFound(new { error = "Rekening tidak ditemukan" });
            }

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildBalanceInquiryMessage(account, stan, TerminalId);
            var isoResponse = Iso8583ResponseBuilder.BuildBalanceInquiryResponse(isoRequest, true, "00", account.AccountNumber, account.Balance);

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = "Cek Saldo",
                Amount = 0,
                Description = "Inquiry saldo via API",
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0100/0110",
                Iso8583MessageRaw = isoResponse.ToIso8583String(),
                TransactionCategory = "INQUIRY",
                OperationType = "BALANCE_INQUIRY",
                ResponseMessage = $"Saldo: Rp {account.Balance:N2}"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Balance inquiry successful",
                accountNumber = account.AccountNumber,
                accountHolder = account.AccountHolderName,
                balance = account.Balance,
                currency = "IDR",
                iso8583Request = isoRequest.ToIso8583String(),
                iso8583Response = isoResponse.ToIso8583String(),
                stan = stan,
                rrn = isoResponse.GetString(37),
                responseCode = "00",
                timestamp = DateTime.Now
            });
        }

        [HttpPost("simulate-account-opening")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> SimulateAccountOpening([FromBody] SimulateAccountOpeningRequest request)
        {
            if (string.IsNullOrEmpty(request.AccountNumber) || string.IsNullOrEmpty(request.AccountHolderName))
            {
                return BadRequest(new { error = "AccountNumber dan AccountHolderName harus diisi" });
            }

            var exists = await _context.Accounts.AnyAsync(a => a.AccountNumber == request.AccountNumber);
            if (exists)
            {
                return BadRequest(new { error = "Nomor rekening sudah ada" });
            }

            var account = new Account
            {
                AccountNumber = request.AccountNumber,
                AccountHolderName = request.AccountHolderName,
                AccountType = request.AccountType ?? "Tabungan",
                Balance = 0,
                CreatedAt = DateTime.Now
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildAccountOpeningMessage(account, stan, TerminalId);
            var isoResponse = Iso8583ResponseBuilder.BuildAccountOpeningResponse(isoRequest, true, "00", account.AccountNumber, account.Id);

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = "Pembukaan Akun",
                Amount = 0,
                Description = $"Pembukaan akun {account.AccountType} via API",
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0100/0110",
                Iso8583MessageRaw = isoResponse.ToIso8583String(),
                TransactionCategory = "ACCOUNT_OPENING",
                OperationType = "ACCOUNT_OPENING",
                ResponseMessage = "Akun berhasil dibuka"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Account opening successful",
                accountNumber = account.AccountNumber,
                accountHolder = account.AccountHolderName,
                accountType = account.AccountType,
                accountId = account.Id,
                iso8583Request = isoRequest.ToIso8583String(),
                iso8583Response = isoResponse.ToIso8583String(),
                stan = stan,
                rrn = isoResponse.GetString(37),
                responseCode = "00",
                timestamp = DateTime.Now
            });
        }

        [HttpPost("simulate-transaction-history")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> SimulateTransactionHistory([FromBody] SimulateTransactionHistoryRequest request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);
            if (account == null)
            {
                return NotFound(new { error = "Rekening tidak ditemukan" });
            }

            var transactions = await _context.Transactions
                .Where(t => t.AccountId == account.Id)
                .OrderByDescending(t => t.TransactionDate)
                .Take(request.PageSize)
                .ToListAsync();

            var fromDate = request.FromDate ?? DateTime.Today.AddDays(-30);
            var toDate = request.ToDate ?? DateTime.Now;

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildTransactionHistoryMessage(account, fromDate, toDate, stan, TerminalId);
            var isoResponse = Iso8583ResponseBuilder.BuildTransactionHistoryResponse(isoRequest, true, "00", account.AccountNumber, transactions.Count);

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = "Riwayat Transaksi",
                Amount = 0,
                Description = $"Inquiry riwayat transaksi via API - {transactions.Count} records",
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0100/0110",
                Iso8583MessageRaw = isoResponse.ToIso8583String(),
                TransactionCategory = "INQUIRY",
                OperationType = "HISTORY_INQUIRY",
                ResponseMessage = $"{transactions.Count} transaksi ditemukan"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Transaction history inquiry successful",
                accountNumber = account.AccountNumber,
                accountHolder = account.AccountHolderName,
                fromDate = fromDate,
                toDate = toDate,
                totalTransactions = transactions.Count,
                transactions = transactions.Select(t => new
                {
                    id = t.Id,
                    type = t.TransactionType,
                    amount = t.Amount,
                    description = t.Description,
                    date = t.TransactionDate,
                    stan = t.Stan,
                    rrn = t.Rrn,
                    responseCode = t.ResponseCode,
                    mti = t.MTI
                }).ToList(),
                iso8583Request = isoRequest.ToIso8583String(),
                iso8583Response = isoResponse.ToIso8583String(),
                stan = stan,
                rrn = isoResponse.GetString(37),
                responseCode = "00",
                timestamp = DateTime.Now
            });
        }
    }

    public class SimulateBalanceInquiryRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
    }

    public class SimulateAccountOpeningRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string? AccountType { get; set; }
    }

    public class SimulateTransactionHistoryRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageSize { get; set; } = 50;
    }

    public class SimulateSetorRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class SimulateTarikRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class SimulateTransferRequest
    {
        public string FromAccountNumber { get; set; } = string.Empty;
        public string ToAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
