using GearZone.Domain.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Auth;
using GearZone.Application.Features.Seller;
using GearZone.Infrastructure.External;
using GearZone.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GearZone.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFileStorageService, CloudinaryStorageService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IPaymentService, PayOSPaymentService>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ICategoryAttributeRepository, CategoryAttributeRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<IVariantAttributeValueRepository, VariantAttributeValueRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminStoreService, AdminStoreService>();
            services.AddScoped<ISellerStoreService, SellerStoreService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
