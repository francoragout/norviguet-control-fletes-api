using norviguet_control_fletes_api.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DeliveryNote
{
    public class CreateDeliveryNoteDto
    {
        [Required]
        [RegularExpression("^\\d{5}-\\d{8}$", ErrorMessage = "DeliveryNoteNumber must have the format NNNNN-NNNNNNNN")]
        public string DeliveryNoteNumber { get; set; } = string.Empty;
        [Required]
        [StringLength(50, ErrorMessage = "Address must be between 1 and 50 characters long.")]
        public string Address { get; set; } = string.Empty;
        [Required]
        [StringLength(50, ErrorMessage = "Location must be between 1 and 50 characters long.")]
        public string Location { get; set; } = string.Empty;
        public int CarrierId { get; set; }
        public int OrderId { get; set; }
    }
}
