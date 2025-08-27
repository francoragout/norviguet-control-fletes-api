namespace norviguet_control_fletes_api.Models.Payment
{
    public class DeletePaymentsDto
    {
        public List<int> PaymentIds { get; set; } = new();
    }
}