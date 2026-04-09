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
                .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.OrderCode))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.ReceiverName))
                .ForMember(dest => dest.GrandTotal, opt => opt.MapFrom(src => src.GrandTotal))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
                .ForMember(dest => dest.ReceiverPhone, opt => opt.MapFrom(src => src.ReceiverPhone))
                .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt));
        }
    }
}
