using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AplikasiBanking8583.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Rekening")]
        public int AccountId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Jenis Transaksi")]
        public string TransactionType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Jumlah")]
        public decimal Amount { get; set; }

        [StringLength(200)]
        [Display(Name = "Deskripsi")]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Referensi")]
        public string? Reference { get; set; }

        [StringLength(6)]
        [Display(Name = "STAN")]
        public string Stan { get; set; } = string.Empty;

        [StringLength(12)]
        [Display(Name = "RRN")]
        public string Rrn { get; set; } = string.Empty;

        [StringLength(4)]
        [Display(Name = "Response Code")]
        public string ResponseCode { get; set; } = "00";

        [StringLength(20)]
        [Display(Name = "MTI")]
        public string MTI { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "ISO8583 Message")]
        public string Iso8583MessageRaw { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Kategori Transaksi")]
        public string TransactionCategory { get; set; } = "FINANCIAL";

        [StringLength(50)]
        [Display(Name = "Jenis Operasi")]
        public string OperationType { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Pesan Response")]
        public string? ResponseMessage { get; set; }

        [Display(Name = "Tanggal")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public Account? Account { get; set; }
    }
}
