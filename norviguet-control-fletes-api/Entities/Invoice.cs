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
        public InvoiceType Type { get; set; }
        public int PointOfSale { get; set; }
        public int Number { get; set; }

        // Relación uno a muchos con Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
