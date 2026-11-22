using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Messages;
using Voyage.Web.Data;
using Voyage.Web.Infrastructure;
using Voyage.Web.Infrastructure.Identity;
using Voyage.Web.Infrastructure.ModelBinding;
using Voyage.Web.Services;
using Voyage.Web.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddUserManager<VoyageUserManager>();

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new TrimmingModelBinderProvider());

    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => ErrorMessages.ValueRequired);

    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (value, field) => ErrorMessages.InvalidValue);

    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        _ => ErrorMessages.InvalidValue);

    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
        _ => ErrorMessages.InvalidValue);

    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(
        _ => ErrorMessages.InvalidValue);

    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(
        () => ErrorMessages.InvalidValue);

    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        _ => ErrorMessages.InvalidValue);

    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(
        () => ErrorMessages.InvalidValue);

    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
        _ => ErrorMessages.ValueRequired);

    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => ErrorMessages.ValueRequired);

    options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(
        () => ErrorMessages.ValueRequired);
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("favorites", httpContext =>
    {
        string partitionKey = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IPackingService, PackingService>();

var app = builder.Build();

await app.SeedRolesAndAdminAsync();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/HttpError", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();