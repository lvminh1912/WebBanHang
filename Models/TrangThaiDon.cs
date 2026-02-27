using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("TrangThaiDon")]
    public class TrangThaiDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Vì SQL không để Identity
        public int MaTrangThai { get; set; }

        [Required]
        [StringLength(50)]
        public string TenTrangThai { get; set; } = null!;

        public string? MoTa { get; set; }

        // Navigation Property
        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}