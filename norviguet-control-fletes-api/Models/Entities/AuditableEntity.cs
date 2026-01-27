using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Entities
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
