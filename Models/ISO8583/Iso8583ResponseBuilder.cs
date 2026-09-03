using AplikasiBanking8583.Models;

namespace AplikasiBanking8583.Models.ISO8583
{
    public static class Iso8583ResponseBuilder
    {
        public static Iso8583Message BuildSetorResponse(Iso8583Message request, bool success, string responseCode, string accountNumber, decimal newBalance)
        {
            var response = new Iso8583Message("0210");

            foreach (var kvp in request.DataElements)
            {
                response.DataElements[kvp.Key] = kvp.Value;
            }

            response.SetElement(39, responseCode);
            response.SetElement(4, request.GetString(4));
            response.SetElement(2, accountNumber.PadRight(19).Substring(0, 19));
            response.SetElement(54, $"BAL:{newBalance:F2}");
            response.SetElement(63, "SETOR");

            return response;
        }

        public static Iso8583Message BuildTarikResponse(Iso8583Message request, bool success, string responseCode, string accountNumber, decimal newBalance)
        {
            var response = new Iso8583Message("0210");

            foreach (var kvp in request.DataElements)
            {
                response.DataElements[kvp.Key] = kvp.Value;
            }

            response.SetElement(39, responseCode);
            response.SetElement(4, request.GetString(4));
            response.SetElement(2, accountNumber.PadRight(19).Substring(0, 19));
            response.SetElement(54, $"BAL:{newBalance:F2}");
            response.SetElement(63, "TARIK");

            return response;
        }

        public static Iso8583Message BuildTransferResponse(Iso8583Message request, bool success, string responseCode, string fromAccount, string toAccount, decimal fromBalance, decimal toBalance)
        {
            var response = new Iso8583Message("0210");

            foreach (var kvp in request.DataElements)
            {
                response.DataElements[kvp.Key] = kvp.Value;
            }

            response.SetElement(39, responseCode);
            response.SetElement(4, request.GetString(4));
            response.SetElement(2, fromAccount.PadRight(19).Substring(0, 19));
            response.SetElement(54, $"FROM_BAL:{fromBalance:F2}|TO_BAL:{toBalance:F2}");
            response.SetElement(63, "TRANSFER");

            return response;
        }

        public static Iso8583Message BuildBalanceInquiryResponse(Iso8583Message request, bool success, string responseCode, string accountNumber, decimal balance)
        {
            var response = new Iso8583Message("0110");

            foreach (var kvp in request.DataElements)
            {
                response.DataElements[kvp.Key] = kvp.Value;
            }

            response.SetElement(39, responseCode);
            response.SetElement(2, accountNumber.PadRight(19).Substring(0, 19));
            response.SetElement(54, $"BAL:{balance:F2}");
            response.SetElement(63, "INQUIRY");

            return response;
        }

        public static Iso8583Message BuildAccountOpeningResponse(Iso8583Message request, bool success, string responseCode, string accountNumber, int accountId)
        {
            var response = new Iso8583Message("0110");

            foreach (var kvp in request.DataElements)
            {
                response.DataElements[kvp.Key] = kvp.Value;
            }

            response.SetElement(39, responseCode);
            response.SetElement(2, accountNumber.PadRight(19).Substring(0, 19));
            response.SetElement(61, accountId.ToString());
            response.SetElement(63, "ACCOUNT_OPENING");

            return response;
        }

        public static Iso8583Message BuildTransactionHistoryResponse(Iso8583Message request, bool success, string responseCode, string accountNumber, int transactionCount)
        {
            var response = new Iso8583Message("0110");

            foreach (var kvp in request.DataElements)
            {
                response.DataElements[kvp.Key] = kvp.Value;
            }

            response.SetElement(39, responseCode);
            response.SetElement(2, accountNumber.PadRight(19).Substring(0, 19));
            response.SetElement(61, transactionCount.ToString());
            response.SetElement(63, "HISTORY_INQUIRY");

            return response;
        }

        public static string GetResponseCode(bool success, string? customCode = null)
        {
            if (!string.IsNullOrEmpty(customCode))
            {
                return customCode;
            }
            return success ? "00" : "05";
        }
    }
}
