using System.Collections.Generic;

namespace CloudRetailWebApp.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalUsers { get; set; }
        public int ProductCount { get; set; }
        public IReadOnlyList<OrderMessageModel> QueuedOrders { get; set; } = new List<OrderMessageModel>();
        public IReadOnlyList<string> ContractFiles { get; set; } = new List<string>();
    }
}

