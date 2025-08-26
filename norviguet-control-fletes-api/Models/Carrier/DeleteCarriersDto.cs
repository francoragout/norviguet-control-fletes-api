using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Carrier
{
    public class DeleteCarriersDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one ID must be provided.")]
        public List<int> Ids { get; set; } = new();
    }
}
