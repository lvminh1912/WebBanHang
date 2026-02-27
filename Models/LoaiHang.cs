using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("LoaiHang")]
    public class LoaiHang
    {
        [Key]
        public int MaLoaiHang { get; set; }

        [Required]
        [StringLength(100)]
        public string TenLoaiHang { get; set; } = null!;

        [StringLength(255)]
        public string? MoTa { get; set; }

        // Navigation Property
        public virtual ICollection<MatHang> MatHangs { get; set; } = new List<MatHang>();
    }
}