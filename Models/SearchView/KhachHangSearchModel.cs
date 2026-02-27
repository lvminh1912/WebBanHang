namespace Models.SearchView
{
    public class KhachHangSearchModel
    {
        public string? Keyword { get; set; } // Tìm chung cho Tên hoặc SĐT
        public string? DiaChi { get; set; }
        public int Page { get; set; } = 1;
    }
}