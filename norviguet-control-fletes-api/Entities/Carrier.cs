namespace norviguet_control_fletes_api.Entities
{
    public class Carrier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Relationships
        public ICollection<DeliveryNote> DeliveryNotes { get; set; } = new List<DeliveryNote>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
