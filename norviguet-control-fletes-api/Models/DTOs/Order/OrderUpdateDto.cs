using norviguet_control_fletes_api.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DTOs.Order
{
    public class OrderUpdateDto
    {
        [Required]
        public string Number { get; set; } = string.Empty;
        public OrderIncoterm Incoterm { get; set; } = OrderIncoterm.CIF;
        public byte[] RowVersion { get; set; } = [];

        // Foreign Keys
        public int SellerId { get; set; }
        public int CustomerId { get; set; }
    }
}