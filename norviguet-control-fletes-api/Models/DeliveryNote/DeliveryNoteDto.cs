using System;
using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Models.DeliveryNote
{
    public class DeliveryNoteDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DeliveryNoteStatus Status { get; set; } = DeliveryNoteStatus.Pending;
        public string DeliveryNoteNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Location { get; set; }
        public int? CarrierId { get; set; }
        public int OrderId { get; set; }

        // Additional related data
        public string? CarrierName { get; set; }
    }
}
