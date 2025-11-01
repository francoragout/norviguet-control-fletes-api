using norviguet_control_fletes_api.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string OrderNumber { get; set; } = string.Empty;
        public OrderIncoterm Incoterm { get; set; } = OrderIncoterm.CIF;
        public int? CustomerId { get; set; }
        public int? SellerId { get; set; }

        // Additional related data
        public string? CustomerName { get; set; }
        public string? SellerName { get; set; }
        public int PendingDeliveryNoteNumbersCount { get; set; }
        public bool HasInvoice { get; set; }
    }
}