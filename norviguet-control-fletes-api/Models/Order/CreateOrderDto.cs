using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Models.Order
{
    public class CreateOrderDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public OrderIncoterm Incoterm { get; set; } = OrderIncoterm.CIF;
        public int? SellerId { get; set; }
        public int? CustomerId { get; set; }
    }
}