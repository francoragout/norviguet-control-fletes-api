namespace norviguet_control_fletes_api.Models.DTOs.Carrier
{
    public class CarrierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte[]? RowVersion { get; set; }
    }
}
