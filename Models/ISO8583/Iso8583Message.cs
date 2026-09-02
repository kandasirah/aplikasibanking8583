using System.Text;

namespace AplikasiBanking8583.Models.ISO8583
{
    public class Iso8583Message
    {
        public string MTI { get; set; } = "0200";
        public Dictionary<int, string> DataElements { get; set; } = new();

        public Iso8583Message()
        {
        }

        public Iso8583Message(string mti)
        {
            MTI = mti;
        }

        public void SetElement(int elementNumber, string value)
        {
            DataElements[elementNumber] = value;
        }

        public string? GetElement(int elementNumber)
        {
            return DataElements.TryGetValue(elementNumber, out var value) ? value : null;
        }

        public string ToIso8583String()
        {
            var sb = new StringBuilder();
            sb.Append(MTI);

            var elementNumbers = DataElements.Keys.OrderBy(k => k).ToList();
            int maxElement = elementNumbers.Any() ? elementNumbers.Max() : 0;

            long bitmap = 0;
            foreach (var elementNum in elementNumbers)
            {
                bitmap |= 1L << (128 - elementNum);
            }

            for (int i = 127; i >= 0; i--)
            {
                sb.Append((bitmap & (1L << i)) != 0 ? '1' : '0');
            }

            foreach (var elementNum in elementNumbers)
            {
                var value = DataElements[elementNum];
                if (value != null)
                {
                    sb.Append(value);
                }
            }

            return sb.ToString();
        }

        public static Iso8583Message Parse(string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length < 132)
            {
                throw new ArgumentException("Invalid ISO 8583 message length");
            }

            var isoMessage = new Iso8583Message();
            isoMessage.MTI = message.Substring(0, 4);

            long bitmap = 0;
            for (int i = 0; i < 128; i++)
            {
                if (message[4 + i] == '1')
                {
                    bitmap |= 1L << (127 - i);
                }
            }

            int currentPos = 132;
            for (int elementNum = 1; elementNum <= 128; elementNum++)
            {
                if ((bitmap & (1L << (128 - elementNum))) != 0)
                {
                    int fieldLength = GetFieldLength(elementNum);
                    if (fieldLength > 0)
                    {
                        if (currentPos + fieldLength > message.Length)
                        {
                            break;
                        }
                        isoMessage.DataElements[elementNum] = message.Substring(currentPos, fieldLength);
                        currentPos += fieldLength;
                    }
                }
            }

            return isoMessage;
        }

        private static int GetFieldLength(int elementNumber)
        {
            return elementNumber switch
            {
                2 => 19,
                3 => 6,
                4 => 12,
                5 => 12,
                6 => 12,
                7 => 10,
                11 => 6,
                12 => 6,
                13 => 4,
                14 => 4,
                15 => 4,
                17 => 4,
                22 => 12,
                23 => 3,
                32 => 11,
                33 => 11,
                35 => 37,
                36 => 104,
                37 => 12,
                38 => 6,
                39 => 2,
                41 => 8,
                42 => 15,
                43 => 40,
                48 => 120,
                49 => 3,
                52 => 16,
                53 => 16,
                54 => 120,
                55 => 255,
                56 => 255,
                57 => 255,
                58 => 255,
                59 => 255,
                60 => 255,
                61 => 255,
                62 => 255,
                63 => 255,
                64 => 8,
                _ => 0
            };
        }

        public string GetString(int elementNumber)
        {
            return GetElement(elementNumber) ?? string.Empty;
        }

        public long GetLong(int elementNumber)
        {
            var value = GetElement(elementNumber);
            return long.TryParse(value, out var result) ? result : 0;
        }

        public decimal GetDecimal(int elementNumber)
        {
            var value = GetElement(elementNumber);
            return decimal.TryParse(value, out var result) ? result / 100 : 0;
        }

        public DateTime? GetDateTime(int elementNumber)
        {
            var value = GetElement(elementNumber);
            if (string.IsNullOrEmpty(value)) return null;

            if (value.Length == 4 && int.TryParse(value, out var time))
            {
                return DateTime.Today.AddHours(time / 10000).AddMinutes((time % 10000) / 100).AddSeconds(time % 100);
            }

            if (value.Length == 10 && int.TryParse(value, out var dateTime))
            {
                return DateTime.Today.AddHours(dateTime / 10000000).AddMinutes((dateTime % 10000000) / 100000).AddSeconds((dateTime % 100000) / 1000);
            }

            return null;
        }
    }
}
