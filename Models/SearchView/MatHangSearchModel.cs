namespace Models.SearchView
{
    public class MatHangSearchModel
    {
        public string? SearchName { get; set; }
        public int? MaLoai { get; set; }
        public decimal? GiaMin { get; set; }
        public decimal? GiaMax { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string SortOrder { get; set; } = "";
    }
}
