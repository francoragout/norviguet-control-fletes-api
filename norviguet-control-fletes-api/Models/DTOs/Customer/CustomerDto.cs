namespace norviguet_control_fletes_api.Models.DTOs.Customer
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
    }
}
