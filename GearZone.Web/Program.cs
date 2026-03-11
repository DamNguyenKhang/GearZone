using GearZone.Application;
using GearZone.Domain.Entities;
using GearZone.Infrastructure;
using GearZone.Infrastructure.Jobs;
using GearZone.Infrastructure.Seed;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration["DB_CONNECTION_STRING"] ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    // Use Identity's scheme as the default for everything
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["GOOGLE_CLIENT_ID"] ?? "";
    options.ClientSecret = builder.Configuration["GOOGLE_CLIENT_SECRET"] ?? "";
});

builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(GearZone.Application.Abstractions.Services.IAuthService).Assembly);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.AccessDeniedPath = "/Auth/Login";
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    opt.SlidingExpiration = true;
});

builder.Services
    .AddDatabase(connectionString)
    .AddApplication()
    .AddInfrastructure()
    ;

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    await IdentitySeeder.SeedAsync(userManager, roleManager, configuration);
    await CatalogSeeder.SeedAsync(dbContext);
}

app.UseHttpsRedirection();
app.UseCors();

// URL Rewrite for backward compatibility
var rewriteOptions = new RewriteOptions()
    .AddRedirect("(?i)Public/Catalog/Browse/?$", "products")
    .AddRedirect("(?i)Public/Catalog/Browse(.*)", "products$1");
app.UseRewriter(rewriteOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

using (var scope = app.Services.CreateScope())
{
    // Recurring jobs
    RecurringJob.AddOrUpdate<PayoutBatchJob>(
        "generate-weekly-payout",
        job => job.GenerateWeeklyBatchAsync(),
        "1 17 * * 0", // Chủ nhật 17:01 UTC = Thứ 2 00:01 VN
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<PayoutBatchJob>(
        "retry-failed-payouts",
        job => job.RetryFailedTransactionsAsync(),
        "0 */6 * * *",
        TimeZoneInfo.Utc);
}

app.UseStaticFiles();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
