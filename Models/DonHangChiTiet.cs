using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("DonHangChiTiet")]
    public class DonHangChiTiet
    {
        [Key]
        public int MaChiTiet { get; set; }

        [Required]
        public int MaDonHang { get; set; }

        [Required]
        public int MaMatHang { get; set; }

        [Required]
        public int SoLuong { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        // Navigation Properties
        [ForeignKey("MaDonHang")]
        public virtual DonHang? DonHang { get; set; }

        [ForeignKey("MaMatHang")]
        public virtual MatHang? MatHang { get; set; }
    }
}