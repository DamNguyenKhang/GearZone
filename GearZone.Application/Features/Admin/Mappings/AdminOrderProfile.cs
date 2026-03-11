using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings
{
    public class AdminOrderProfile : Profile
    {
        public AdminOrderProfile()
        {
            CreateMap<Order, AdminOrderDto>()
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PayoutStatus, opt => opt.MapFrom(src => src.PayoutStatus.ToString()));
        }
    }
}
