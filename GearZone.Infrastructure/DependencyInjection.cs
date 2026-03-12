using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Infrastructure.External;
using GearZone.Infrastructure.Repositories;
using GearZone.Infrastructure.Settings;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayOS;

namespace GearZone.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PayOSSettings>(options =>
            {
                options.ClientId = configuration["PAYOS_CLIENT_ID"]!;
                options.ApiKey = configuration["PAYOS_API_KEY"]!;
                options.ChecksumKey = configuration["PAYOS_CHECKSUM_KEY"]!;
                options.ReturnUrl = configuration["PAYOS_RETURN_URL"]!;
                options.CancelUrl = configuration["PAYOS_CANCEL_URL"]!;
            });

            services.Configure<PayOSPayoutSettings>(options =>
            {
                options.ClientId = configuration["PAYOS_PAYOUT_CLIENT_ID"]!;
                options.ApiKey = configuration["PAYOS_PAYOUT_API_KEY"]!;
                options.ChecksumKey = configuration["PAYOS_PAYOUT_CHECKSUM_KEY"]!;
            });

            services.AddMemoryCache();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFileStorageService, CloudinaryStorageService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IPaymentStrategy, PayOSPaymentStrategy>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ICategoryAttributeRepository, CategoryAttributeRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ISubOrderRepository, SubOrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductAttributeValueRepository, ProductAttributeValueRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<IVariantAttributeValueRepository, VariantAttributeValueRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
            services.AddScoped<IStoreFollowRepository, StoreFollowRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IPayoutBatchRepository, PayoutBatchRepository>();
            services.AddScoped<IPayoutTransactionRepository, PayoutTransactionRepository>();
            services.AddScoped<IPayoutItemRepository, PayoutItemRepository>();
            services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

            services.AddScoped<IPaymentStrategy, PayOSPaymentStrategy>();
            services.AddScoped<IPaymentStrategy, CodPaymentStrategy>();
            services.AddScoped<IPayoutClient, PayOSPayoutClient>();

            services.AddKeyedSingleton("OrderClient", (sp, key) =>
            {
                var settings = sp.GetRequiredService<IOptions<PayOSSettings>>().Value;
                return new PayOSClient(new PayOSOptions
                {
                    ClientId = settings.ClientId,
                    ApiKey = settings.ApiKey,
                    ChecksumKey = settings.ChecksumKey,
                    LogLevel = LogLevel.Debug,
                });
            });

            services.AddKeyedSingleton("TransferClient", (sp, key) =>
            {
                var settings = sp.GetRequiredService<IOptions<PayOSPayoutSettings>>().Value;
                return new PayOSClient(new PayOSOptions
                {
                    ClientId = settings.ClientId,
                    ApiKey = settings.ApiKey,
                    ChecksumKey = settings.ChecksumKey,
                    LogLevel = LogLevel.Debug,
                });
            });

            services.AddHangfireServer(opt => opt.WorkerCount = 2);

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString));

            return services;
        }
    }
}

