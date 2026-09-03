using AplikasiBanking8583.Models;

namespace AplikasiBanking8583.Models.ISO8583
{
    public static class Iso8583TransactionBuilder
    {
        public static Iso8583Message BuildSetorMessage(Account account, decimal amount, string description, string stan, string terminalId)
        {
            var mti = "0200";
            var message = new Iso8583Message(mti);

            message.SetElement(2, account.AccountNumber.PadRight(19).Substring(0, 19));
            message.SetElement(3, "310000");
            message.SetElement(4, ((long)(amount * 100)).ToString().PadLeft(12, '0'));
            message.SetElement(7, DateTime.Now.ToString("MMddHHmmss"));
            message.SetElement(11, stan.PadLeft(6, '0'));
            message.SetElement(12, DateTime.Now.ToString("HHmmss"));
            message.SetElement(13, DateTime.Now.ToString("MMdd"));
            message.SetElement(17, DateTime.Now.ToString("MMdd"));
            message.SetElement(22, "012");
            message.SetElement(32, "1234567890");
            message.SetElement(35, account.AccountNumber.PadRight(37).Substring(0, 37));
            message.SetElement(37, Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper());
            message.SetElement(41, terminalId.PadRight(8).Substring(0, 8));
            message.SetElement(42, "BANKINGAPPSYS");
            message.SetElement(43, "BANKING APPLICATION TERMINAL");
            message.SetElement(49, "360");
            message.SetElement(58, description.PadRight(120).Substring(0, 120));
            message.SetElement(60, "SETOR");
            message.SetElement(61, account.Id.ToString());

            return message;
        }

        public static Iso8583Message BuildTarikMessage(Account account, decimal amount, string description, string stan, string terminalId)
        {
            var mti = "0200";
            var message = new Iso8583Message(mti);

            message.SetElement(2, account.AccountNumber.PadRight(19).Substring(0, 19));
            message.SetElement(3, "010000");
            message.SetElement(4, ((long)(amount * 100)).ToString().PadLeft(12, '0'));
            message.SetElement(7, DateTime.Now.ToString("MMddHHmmss"));
            message.SetElement(11, stan.PadLeft(6, '0'));
            message.SetElement(12, DateTime.Now.ToString("HHmmss"));
            message.SetElement(13, DateTime.Now.ToString("MMdd"));
            message.SetElement(17, DateTime.Now.ToString("MMdd"));
            message.SetElement(22, "012");
            message.SetElement(32, "1234567890");
            message.SetElement(35, account.AccountNumber.PadRight(37).Substring(0, 37));
            message.SetElement(37, Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper());
            message.SetElement(41, terminalId.PadRight(8).Substring(0, 8));
            message.SetElement(42, "BANKINGAPPSYS");
            message.SetElement(43, "BANKING APPLICATION TERMINAL");
            message.SetElement(49, "360");
            message.SetElement(58, description.PadRight(120).Substring(0, 120));
            message.SetElement(60, "TARIK");
            message.SetElement(61, account.Id.ToString());

            return message;
        }

        public static Iso8583Message BuildTransferMessage(Account fromAccount, Account toAccount, decimal amount, string description, string stan, string terminalId)
        {
            var mti = "0200";
            var message = new Iso8583Message(mti);

            message.SetElement(2, fromAccount.AccountNumber.PadRight(19).Substring(0, 19));
            message.SetElement(3, "400000");
            message.SetElement(4, ((long)(amount * 100)).ToString().PadLeft(12, '0'));
            message.SetElement(5, ((long)(amount * 100)).ToString().PadLeft(12, '0'));
            message.SetElement(6, "000000000000");
            message.SetElement(7, DateTime.Now.ToString("MMddHHmmss"));
            message.SetElement(11, stan.PadLeft(6, '0'));
            message.SetElement(12, DateTime.Now.ToString("HHmmss"));
            message.SetElement(13, DateTime.Now.ToString("MMdd"));
            message.SetElement(17, DateTime.Now.ToString("MMdd"));
            message.SetElement(22, "012");
            message.SetElement(32, "1234567890");
            message.SetElement(33, "1234567890");
            message.SetElement(35, fromAccount.AccountNumber.PadRight(37).Substring(0, 37));
            message.SetElement(36, toAccount.AccountNumber.PadRight(104).Substring(0, 104));
            message.SetElement(37, Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper());
            message.SetElement(41, terminalId.PadRight(8).Substring(0, 8));
            message.SetElement(42, "BANKINGAPPSYS");
            message.SetElement(43, "BANKING APPLICATION TERMINAL");
            message.SetElement(49, "360");
            message.SetElement(58, description.PadRight(120).Substring(0, 120));
            message.SetElement(60, "TRANSFER");
            message.SetElement(61, $"{fromAccount.Id}-{toAccount.Id}");
            message.SetElement(62, toAccount.AccountNumber.PadRight(255).Substring(0, 255));

            return message;
        }

        public static Iso8583Message BuildReversalMessage(string originalStan, string originalRrn, string terminalId)
        {
            var mti = "0420";
            var message = new Iso8583Message(mti);

            message.SetElement(3, "000000");
            message.SetElement(7, DateTime.Now.ToString("MMddHHmmss"));
            message.SetElement(11, Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper());
            message.SetElement(12, DateTime.Now.ToString("HHmmss"));
            message.SetElement(13, DateTime.Now.ToString("MMdd"));
            message.SetElement(17, DateTime.Now.ToString("MMdd"));
            message.SetElement(37, Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper());
            message.SetElement(38, originalStan.PadLeft(6, '0'));
            message.SetElement(41, terminalId.PadRight(8).Substring(0, 8));
            message.SetElement(42, "BANKINGAPPSYS");
            message.SetElement(90, originalRrn);

            return message;
        }

        public static Iso8583Message BuildReversalResponse(Iso8583Message reversal, string responseCode)
        {
            var mti = "0430";
            var response = new Iso8583Message(mti);

            foreach (var kvp in reversal.DataElements)
            {
                response.DataElements[kvp.Key] = kvp.Value;
            }

            response.SetElement(39, responseCode);
            response.SetElement(90, reversal.GetString(90));

            return response;
        }

        public static Iso8583Message BuildBalanceInquiryMessage(Account account, string stan, string terminalId)
        {
            var mti = "0100";
            var message = new Iso8583Message(mti);

            message.SetElement(2, account.AccountNumber.PadRight(19).Substring(0, 19));
            message.SetElement(3, "310000");
            message.SetElement(7, DateTime.Now.ToString("MMddHHmmss"));
            message.SetElement(11, stan.PadLeft(6, '0'));
            message.SetElement(12, DateTime.Now.ToString("HHmmss"));
            message.SetElement(13, DateTime.Now.ToString("MMdd"));
            message.SetElement(17, DateTime.Now.ToString("MMdd"));
            message.SetElement(22, "012");
            message.SetElement(32, "1234567890");
            message.SetElement(35, account.AccountNumber.PadRight(37).Substring(0, 37));
            message.SetElement(37, Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper());
            message.SetElement(41, terminalId.PadRight(8).Substring(0, 8));
            message.SetElement(42, "BANKINGAPPSYS");
            message.SetElement(43, "BANKING APPLICATION TERMINAL");
            message.SetElement(49, "360");
            message.SetElement(60, "INQUIRY");
            message.SetElement(61, account.Id.ToString());

            return message;
        }

        public static Iso8583Message BuildAccountOpeningMessage(Account account, string stan, string terminalId)
        {
            var mti = "0100";
            var message = new Iso8583Message(mti);

            message.SetElement(2, account.AccountNumber.PadRight(19).Substring(0, 19));
            message.SetElement(3, "000000");
            message.SetElement(7, DateTime.Now.ToString("MMddHHmmss"));
            message.SetElement(11, stan.PadLeft(6, '0'));
            message.SetElement(12, DateTime.Now.ToString("HHmmss"));
            message.SetElement(13, DateTime.Now.ToString("MMdd"));
            message.SetElement(17, DateTime.Now.ToString("MMdd"));
            message.SetElement(22, "012");
            message.SetElement(32, "1234567890");
            message.SetElement(35, account.AccountNumber.PadRight(37).Substring(0, 37));
            message.SetElement(37, Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper());
            message.SetElement(41, terminalId.PadRight(8).Substring(0, 8));
            message.SetElement(42, "BANKINGAPPSYS");
            message.SetElement(43, "BANKING APPLICATION TERMINAL");
            message.SetElement(49, "360");
            message.SetElement(60, "ACCOUNT_OPENING");
            message.SetElement(61, account.Id.ToString());
            message.SetElement(63, $"NAME:{account.AccountHolderName}|TYPE:{account.AccountType}");

            return message;
        }

        public static Iso8583Message BuildTransactionHistoryMessage(Account account, DateTime fromDate, DateTime toDate, string stan, string terminalId)
        {
            var mti = "0100";
            var message = new Iso8583Message(mti);

            message.SetElement(2, account.AccountNumber.PadRight(19).Substring(0, 19));
            message.SetElement(3, "000000");
            message.SetElement(7, DateTime.Now.ToString("MMddHHmmss"));
            message.SetElement(11, stan.PadLeft(6, '0'));
            message.SetElement(12, DateTime.Now.ToString("HHmmss"));
            message.SetElement(13, DateTime.Now.ToString("MMdd"));
            message.SetElement(17, DateTime.Now.ToString("MMdd"));
            message.SetElement(22, "012");
            message.SetElement(32, "1234567890");
            message.SetElement(35, account.AccountNumber.PadRight(37).Substring(0, 37));
            message.SetElement(37, Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper());
            message.SetElement(41, terminalId.PadRight(8).Substring(0, 8));
            message.SetElement(42, "BANKINGAPPSYS");
            message.SetElement(43, "BANKING APPLICATION TERMINAL");
            message.SetElement(49, "360");
            message.SetElement(60, "HISTORY_INQUIRY");
            message.SetElement(61, account.Id.ToString());
            message.SetElement(62, $"FROM:{fromDate:yyyyMMdd}|TO:{toDate:yyyyMMdd}");

            return message;
        }

        public static string GenerateSTAN()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }
    }
}
