namespace Models.SearchView
{
    public class DonHangSearchModel
    {
        public string? Keyword { get; set; } // Tìm theo tên khách hoặc mã đơn
        public int? MaTrangThai { get; set; } // Lọc theo trạng thái (Mới, Đang giao...)
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public int Page { get; set; } = 1;
    }
}
