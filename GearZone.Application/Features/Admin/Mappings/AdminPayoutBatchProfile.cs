using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings
{
    public class AdminPayoutBatchProfile : Profile
    {
        public AdminPayoutBatchProfile()
        {
            CreateMap<PayoutBatch, AdminPayoutBatchDto>()
                .ForMember(dest => dest.Transactions, opt => opt.Ignore());

            CreateMap<PayoutTransaction, AdminPayoutTransactionDto>()
                .ForMember(dest => dest.TransactionCode, opt => opt.MapFrom(src => src.TransactionCode.ToString()))
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
                .ForMember(dest => dest.StoreEmail, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.BankName))
                .ForMember(dest => dest.BankAccountNumber, opt => opt.MapFrom(src => src.BankAccountNumber));
        }
    }
}
