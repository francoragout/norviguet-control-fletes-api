using norviguet_control_fletes_api.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Invoice
{
    public class CreateInvoiceDto
    {
        //[Required]
        //public DateTime CreatedAt { get; set; }
        [Required]
        public InvoiceType Type { get; set; }
        [Required]
        public int PointOfSale { get; set; }
    }
}