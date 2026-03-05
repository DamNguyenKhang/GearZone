using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings;

public class AdminProductProfile : Profile
{
    public AdminProductProfile()
    {
        // Index Listing Map
        CreateMap<Product, AdminProductDto>()
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => 
                src.Variants != null && src.Variants.Any() ? src.Variants.FirstOrDefault().Sku : string.Empty))
            .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => 
                src.Store != null ? src.Store.StoreName : string.Empty))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => 
                src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.BasePrice))
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => 
                src.Variants != null ? src.Variants.Sum(v => v.StockQuantity) : 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => 
                src.Images != null && src.Images.Any()
                    ? (src.Images.FirstOrDefault(i => i.IsPrimary) != null 
                        ? src.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl 
                        : src.Images.FirstOrDefault().ImageUrl)
                    : null));

        // Details Maps
        CreateMap<Category, AdminCategoryInfoDto>();
        CreateMap<Brand, AdminBrandInfoDto>();
        CreateMap<Store, AdminStoreInfoDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.StoreName));

        CreateMap<VariantAttributeValue, AdminVariantAttributeDto>()
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => 
                src.CategoryAttribute != null ? src.CategoryAttribute.Name : string.Empty))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src =>
                src.CategoryAttributeOption != null ? src.CategoryAttributeOption.Value : string.Empty));

        CreateMap<ProductVariant, AdminProductVariantDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.VariantName))
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockQuantity))
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.AttributeValues));

        CreateMap<Product, AdminProductDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CommissionRate, opt => opt.MapFrom(src => 
                src.Store != null ? src.Store.CommissionRate * 100 : 0)) // Showing as percentage
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => 
                src.Variants != null ? src.Variants.Sum(v => v.StockQuantity) : 0))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => 
                src.Images != null ? src.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList() : new List<string>()))
            .ForMember(dest => dest.Specs, opt => opt.MapFrom(src =>
                src.Variants != null
                    ? src.Variants
                        .SelectMany(v => v.AttributeValues)
                        .Where(av => av.CategoryAttribute != null && av.CategoryAttributeOption != null)
                        .GroupBy(av => new { av.CategoryAttributeId, av.CategoryAttribute.Name })
                        .OrderBy(g => g.Key.CategoryAttributeId)
                        .Select(g => new AdminProductSpecDto
                        {
                            AttributeName = g.Key.Name,
                            Values = g.Select(av => av.CategoryAttributeOption.Value)
                                       .Where(v => !string.IsNullOrEmpty(v))
                                       .Distinct()
                                       .OrderBy(v => v)
                                       .ToList()
                        })
                        .ToList()
                    : new List<AdminProductSpecDto>()));
    }
}
