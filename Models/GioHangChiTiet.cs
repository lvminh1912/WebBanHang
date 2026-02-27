using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    [Table("GioHangChiTiet")]
    public class GioHangChiTiet
    {
        [Key]
        public int MaChiTiet { get; set; }
        public int MaGioHang { get; set; }
        public int MaMatHang { get; set; }
        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [ForeignKey("MaGioHang")]
        public virtual GioHang? GioHang { get; set; }
        [ForeignKey("MaMatHang")]
        public virtual MatHang? MatHang { get; set; }
    }
}
