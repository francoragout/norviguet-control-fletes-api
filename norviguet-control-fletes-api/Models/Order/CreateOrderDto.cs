namespace norviguet_control_fletes_api.Models.Order
{
    public class CreateOrderDto
    {
        public string DeliveryNote { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public float DiscountRate { get; set; }
        public int PurchaseOrder { get; set; }
        public int? CarrierId { get; set; }
        public int SellerId { get; set; }
        public int CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentOrderId { get; set; }
    }
}