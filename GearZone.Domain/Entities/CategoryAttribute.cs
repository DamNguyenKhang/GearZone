using System;
using System.Collections.Generic;

namespace GearZone.Domain.Entities
{
    public class CategoryAttribute : Entity<int>
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FilterType { get; set; } = "Checkbox";
        public int DisplayOrder { get; set; }
        public bool IsFilterable { get; set; } = true;

        public Category Category { get; set; } = null!;
        public ICollection<CategoryAttributeOption> Options { get; set; } = new List<CategoryAttributeOption>();
    }
}
