using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.SearchView
{
    public class HeaderViewModel
    {
        public string FullName { get; set; } = "Khách hàng";
        public string Avatar { get; set; } = "/images/no-image.png";
        public int CartCount { get; set; } = 0;
        public decimal CartTotal { get; set; } = 0;
    }
}
