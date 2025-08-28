namespace norviguet_control_fletes_api.Entities
{
    public enum InvoiceType
    {
        A,
        B,
        C,
        Z
    }

    public enum InvoicePOS
    {
        POS_0 = 0,
        POS_99 = 99,
    }

    public class Invoice
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public InvoiceType Type { get; set; }
        public InvoicePOS POS { get; set; }

        // Relación uno a muchos con Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
