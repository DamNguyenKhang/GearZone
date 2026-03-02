namespace GearZone.Application.Features.Admin.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
    }
}
