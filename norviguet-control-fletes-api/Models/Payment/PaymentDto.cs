namespace norviguet_control_fletes_api.Models.Payment
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PointOfSale { get; set; } = string.Empty;
        public List<int> OrderIds { get; set; } = new();
    }
}
