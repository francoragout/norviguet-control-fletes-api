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
        // Foreign Keys
        public int CustomerId { get; set; }
        public int SellerId { get; set; }

        // Additional related data
        public string CustomerName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public int DeliveryNotesCount { get; set; }
        public int ApprovedDeliveryNotesCount { get; set; }
        public int CarriersCount { get; set; }
        public int InvoicesCount { get; set; }
        public int PaymentOrdersCount { get; set; }
    }
}