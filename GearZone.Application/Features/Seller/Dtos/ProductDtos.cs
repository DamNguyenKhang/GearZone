using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GearZone.Application.Features.Seller.Dtos
{
    public class SellerProductListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int TotalStock { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PrimaryImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SellerProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Specifications { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsDraft { get; set; }

        public List<ProductSpecDto> Specifications { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
        public List<IFormFile> Images { get; set; } = new();
    }

    public class ProductSpecDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ProductVariantDto
    {
        public string VariantName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        
        // For dynamic attributes per variant
        public List<AttributeSelectionDto> Attributes { get; set; } = new();
    }

    public class AttributeSelectionDto
    {
        public int AttributeId { get; set; }
        public int OptionId { get; set; }
    }

    public class CategoryAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FilterType { get; set; } = string.Empty;
        public List<CategoryAttributeOptionDto> Options { get; set; } = new();
    }

    public class CategoryAttributeOptionDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
