using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Messages;
using Voyage.Data;
using Voyage.Data.Identity;
using Voyage.Services;
using Voyage.Services.Contracts;
using Voyage.Web.Infrastructure.ModelBinding;

namespace Voyage.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddDatabaseDeveloperPageExceptionFilter();

            return services;
        }

        public static IServiceCollection AddApplicationIdentity(this IServiceCollection services)
        {
            services.AddDefaultIdentity<IdentityUser>(options =>
                options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<VoyageUserManager>();

            return services;
        }

        public static IServiceCollection AddApplicationMvc(this IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
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

            return services;
        }

        public static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
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

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDestinationService, DestinationService>();
            services.AddScoped<ITripService, TripService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IHomeService, HomeService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IPackingService, PackingService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<ITravelExpertService, TravelExpertService>();
            services.AddScoped<IExpertTipService, ExpertTipService>();
            services.AddScoped<ITripSharingService, TripSharingService>();

            return services;
        }
    }
}
