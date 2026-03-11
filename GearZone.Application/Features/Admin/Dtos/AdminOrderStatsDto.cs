namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminOrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}
