using System.Collections.Generic;

namespace CloudRetailWebApp.Models
{
    public class CustomerDashboardViewModel
    {
        public string Username { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public int CartItems { get; set; }
        public IEnumerable<ProductModel> FeaturedProducts { get; set; } = new List<ProductModel>();
    }
}

