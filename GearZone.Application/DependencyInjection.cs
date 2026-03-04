using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Auth;
using GearZone.Application.Features.Catalog;
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
            services.AddScoped<IAdminBrandService, AdminBrandService>();
            services.AddScoped<ICatalogService, CatalogService>();
            return services;
        }
    }
}
