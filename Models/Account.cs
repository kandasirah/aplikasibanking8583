using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AplikasiBanking8583.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Nomor Rekening")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Nama Pemilik")]
        public string AccountHolderName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Saldo")]
        public decimal Balance { get; set; } = 0;

        [Required]
        [StringLength(50)]
        [Display(Name = "Tipe Rekening")]
        public string AccountType { get; set; } = "Tabungan";

        [Display(Name = "Tanggal Dibuat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Transaction> Transactions { get; set; } = new();
    }
}
