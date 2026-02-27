using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public int MaGioHang { get; set; }
        public int MaKhachHang { get; set; }

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }
        public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; } = new List<GioHangChiTiet>();
    }
}
