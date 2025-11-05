using norviguet_control_fletes_api.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Order
{
    public class CreateOrderDto
    {
        [Required]
        public string OrderNumber { get; set; } = string.Empty;
        public OrderIncoterm Incoterm { get; set; } = OrderIncoterm.CIF;

        // Foreign Keys
        public int SellerId { get; set; }
        public int CustomerId { get; set; }
    }
}