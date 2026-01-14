using norviguet_control_fletes_api.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DTOs.Order
{
    public class UpdateOrderDto
    {
        [Required]
        public string OrderNumber { get; set; } = string.Empty;
        public OrderIncoterm Incoterm { get; set; } = OrderIncoterm.CIF;

        // Foreign Keys
        public int SellerId { get; set; }
        public int CustomerId { get; set; }
    }
}