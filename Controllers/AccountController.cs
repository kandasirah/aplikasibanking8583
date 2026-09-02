using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplikasiBanking8583.Models;
using AplikasiBanking8583.Models.ISO8583;

namespace AplikasiBanking8583.Controllers
{
    public class AccountController : Controller
    {
        private readonly BankingDbContext _context;
        private const string TerminalId = "TERMINAL01";

        public AccountController(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _context.Accounts.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return View(accounts);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(Account account)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Accounts.AnyAsync(a => a.AccountNumber == account.AccountNumber);
                if (exists)
                {
                    ModelState.AddModelError("AccountNumber", "Nomor rekening sudah ada");
                    return View(account);
                }

                account.Balance = 0;
                account.CreatedAt = DateTime.Now;
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        public async Task<IActionResult> Deposit(int? id)
        {
            if (id == null) return NotFound();
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();
            return View(account);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Deposit(int id, decimal amount, string description)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Jumlah harus lebih dari 0";
                return RedirectToAction(nameof(Deposit), new { id });
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildSetorMessage(account, amount, string.IsNullOrEmpty(description) ? "Setor Tunai" : description, stan, TerminalId);
            var isoResponse = Iso8583ResponseBuilder.BuildSetorResponse(isoRequest, true, "00", account.AccountNumber, account.Balance + amount);

            account.Balance += amount;

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = "Setor",
                Amount = amount,
                Description = string.IsNullOrEmpty(description) ? "Setor Tunai" : description,
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0200/0210",
                Iso8583MessageRaw = isoResponse.ToIso8583String()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Setor tunai berhasil. Saldo baru: Rp {account.Balance:N2}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Withdraw(int? id)
        {
            if (id == null) return NotFound();
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();
            return View(account);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Withdraw(int id, decimal amount, string description)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Jumlah harus lebih dari 0";
                return RedirectToAction(nameof(Withdraw), new { id });
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            if (account.Balance < amount)
            {
                TempData["Error"] = "Saldo tidak mencukupi";
                return RedirectToAction(nameof(Withdraw), new { id });
            }

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildTarikMessage(account, amount, string.IsNullOrEmpty(description) ? "Tarik Tunai" : description, stan, TerminalId);
            var newBalance = account.Balance - amount;
            var isoResponse = Iso8583ResponseBuilder.BuildTarikResponse(isoRequest, true, "00", account.AccountNumber, newBalance);

            account.Balance -= amount;

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = "Tarik",
                Amount = amount,
                Description = string.IsNullOrEmpty(description) ? "Tarik Tunai" : description,
                TransactionDate = DateTime.Now,
                Stan = stan,
                Rrn = isoResponse.GetString(37),
                ResponseCode = "00",
                MTI = "0200/0210",
                Iso8583MessageRaw = isoResponse.ToIso8583String()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Tarik tunai berhasil. Saldo baru: Rp {account.Balance:N2}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Transfer(int? id)
        {
            if (id == null) return NotFound();
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            ViewBag.OtherAccounts = await _context.Accounts.Where(a => a.Id != id).ToListAsync();
            return View(account);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Transfer(int id, int toAccountId, decimal amount, string description)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Jumlah harus lebih dari 0";
                return RedirectToAction(nameof(Transfer), new { id });
            }

            if (id == toAccountId)
            {
                TempData["Error"] = "Tidak bisa transfer ke rekening sendiri";
                return RedirectToAction(nameof(Transfer), new { id });
            }

            var fromAccount = await _context.Accounts.FindAsync(id);
            var toAccount = await _context.Accounts.FindAsync(toAccountId);

            if (fromAccount == null || toAccount == null)
            {
                TempData["Error"] = "Rekening tidak ditemukan";
                return RedirectToAction(nameof(Transfer), new { id });
            }

            if (fromAccount.Balance < amount)
            {
                TempData["Error"] = "Saldo tidak mencukupi";
                return RedirectToAction(nameof(Transfer), new { id });
            }

            var stan = Iso8583TransactionBuilder.GenerateSTAN();
            var isoRequest = Iso8583TransactionBuilder.BuildTransferMessage(fromAccount, toAccount, amount, string.IsNullOrEmpty(description) ? "Transfer" : description, stan, TerminalId);
            var isoResponse = Iso8583ResponseBuilder.BuildTransferResponse(isoRequest, true, "00", fromAccount.AccountNumber, toAccount.AccountNumber, fromAccount.Balance - amount, toAccount.Balance + amount);

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            var outTransaction = new Transaction
            {
                AccountId = fromAccount.Id,
                TransactionType = "Transfer Keluar",
                Amount = amount,
                Description = $"Transfer ke {toAccount.AccountNumber} - {toAccount.AccountHolderName}. " + (string.IsNullOrEmpty(description) ? "" : description),
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
                Amount = amount,
                Description = $"Transfer dari {fromAccount.AccountNumber} - {fromAccount.AccountHolderName}. " + (string.IsNullOrEmpty(description) ? "" : description),
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

            TempData["Success"] = $"Transfer berhasil ke {toAccount.AccountNumber}. Saldo baru: Rp {fromAccount.Balance:N2}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> History(int? id)
        {
            if (id == null) return NotFound();
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            var transactions = await _context.Transactions
                .Where(t => t.AccountId == id)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.Account = account;
            return View(transactions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();
            return View(account);
        }
    }
}
