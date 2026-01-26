using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DTOs.Invoice
{
    public class InvoiceUpdateDto
    {
        public InvoiceType Type { get; set; } = InvoiceType.A;
        [Required]
        [RegularExpression("^\\d{5}-\\d{8}$", ErrorMessage = "InvoiceNumber must have the format NNNNN-NNNNNNNN")]
        public string InvoiceNumber { get; set; } = string.Empty;
        public int Price { get; set; }

        // Foreign Keys
        public int OrderId { get; set; }
        public int CarrierId { get; set; }
    }
}