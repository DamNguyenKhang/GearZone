using System;

namespace GearZone.Domain.Entities
{
    public class VariantAttributeValue : Entity<Guid>
    {
        public Guid VariantId { get; set; }
        public int CategoryAttributeId { get; set; }
        public string Value { get; set; } = string.Empty;

        public ProductVariant Variant { get; set; } = null!;
        public CategoryAttribute CategoryAttribute { get; set; } = null!;
    }
}
