namespace norviguet_control_fletes_api.Models.Payment
{
    public class CreatePaymentDto
    {
        public int PointOfSale { get; set; }
        public int OrderNumber { get; set; }
    }
}