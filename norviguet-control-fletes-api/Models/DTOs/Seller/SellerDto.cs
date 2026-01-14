namespace norviguet_control_fletes_api.Models.DTOs.Seller
{
    public class SellerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Zone { get; set; }
    }
}