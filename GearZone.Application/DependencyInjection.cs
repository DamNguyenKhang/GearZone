using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Auth;
using GearZone.Application.Features.Cart;
using GearZone.Application.Features.Catalog;
using GearZone.Application.Features.Payment;
using GearZone.Application.Features.Payout;
using GearZone.Application.Features.Payment;
using GearZone.Application.Features.Seller;
using Microsoft.Extensions.DependencyInjection;

namespace GearZone.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminStoreService, AdminStoreService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();
            services.AddScoped<IAdminCategoryService, AdminCategoryService>();
            services.AddScoped<IAdminProductService, AdminProductService>();
            services.AddScoped<IAdminOrderService, AdminOrderService>();
            services.AddScoped<IAdminBrandService, AdminBrandService>();
            services.AddScoped<IAdminPayoutService, AdminPayoutService>();
            services.AddScoped<ICatalogService, CatalogService>();
            services.AddScoped<ISellerStoreService, SellerStoreService>();
            services.AddScoped<IBankCatalogService, BankCatalogService>();
            services.AddScoped<ISellerProductService, SellerProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IPayoutService, PayoutService>();
            services.AddScoped<PaymentStrategyFactory>();
            return services;
        }
    }
}
