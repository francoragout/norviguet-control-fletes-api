namespace norviguet_control_fletes_api.Entities
{
    public enum InvoiceType
    {
        A,
        B,
        C,
        Z
    }

    public class Invoice
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public InvoiceType Type { get; set; }
        public string PointOfSale { get; set; } = string.Empty;

        // Relación uno a muchos con Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
