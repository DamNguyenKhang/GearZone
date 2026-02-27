using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Auth;
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
            services.AddScoped<ISystemSettingService, SystemSettingService>();
            return services;
        }
    }
}
