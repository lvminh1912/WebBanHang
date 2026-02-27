using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("TinhThanh")]
    public class TinhThanh
    {
        [Key]
        public int MaTinh { get; set; }

        [StringLength(50)]
        public string? TenTinh { get; set; }
    }
}