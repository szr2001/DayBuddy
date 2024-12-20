using Microsoft.AspNetCore.Identity;
using DayBuddy.Models;
using DayBuddy.Settings;
using DayBuddy.Hubs;
using DayBuddy.Services;
using MongoDB.Driver;
using DayBuddy.BackgroundServices;
using DayBuddy.Services.Caches;
using Stripe;
using DayBuddy.Authorization.Requirements;
using DayBuddy.Authorization;
using Microsoft.AspNetCore.Authorization;
using DayBuddy.Filters;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace DayBuddy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();

            //Classes that interact with the MongoDB are set as scoped
            builder.Services.AddScoped<MessagesService>();
            builder.Services.AddScoped<ChatGroupsService>();
            builder.Services.AddScoped<UserReportService>();
            builder.Services.AddScoped<UserService>();

            //Filters can be used with the attribute
            //[ServiceFilter(typeof(EnsureUserNotNullFilter))]
            //It must be applied like the [Authorize] attribute
            builder.Services.AddScoped<EnsureDayBuddyUserNotNullFilter>();

            builder.Services.AddSingleton<BuddyGroupCacheService>();
            builder.Services.AddSingleton<MessagesCacheService>();
            builder.Services.AddSingleton<ProfanityFilterService>();
            builder.Services.AddSingleton<UserProfileValidatorService>();
            builder.Services.AddSingleton<GmailSMTPEmailService>();
            //hosted service run as part of the starting/closing process before everything else runs.
            builder.Services.AddHostedService<GroupCachePopulationService>();
            builder.Services.AddHostedService<DbRolesPopulationService>();
            builder.Services.AddHostedService<MessagesCacheFlushService>();

            //More requirements for Authorize attribute
            //like [Authorize("EmailVerified")] to only allow access to user who have email verified
            //we add the Requirement that acts as an Id, then the Handler that handle the requirement
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("EmailVerified", policy =>
                {
                    policy.Requirements.Add(new EmailVerifiedRequirement());

                });
            });
            builder.Services.AddScoped<IAuthorizationHandler, EmailVerifiedHandler>();

            // Add rate limiter
            builder.Services.AddRateLimiter(options =>
            {
                options.OnRejected = (context, token) =>
                {
                    var clientIp = GetClientIp(context.HttpContext);
                    Console.WriteLine($"Rate limit exceeded for User: {clientIp}");
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return new ValueTask(); 
                };

                options.AddPolicy("GeneralPolicy", httpContext =>
                {
                    var clientIp = GetClientIp(httpContext);
                    return RateLimitPartition.GetTokenBucketLimiter(clientIp, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 24,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        TokensPerPeriod = 12,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 1
                    });
                });

                options.AddPolicy("RestrictedPolicy", httpContext =>
                {
                    var clientIp = GetClientIp(httpContext);
                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1,
                        Window = TimeSpan.FromMinutes(1),
                    });
                });
            });

            MongoDbConfig? mongoDBSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();
            if(mongoDBSettings == null)
            {
                Console.WriteLine("ERROR, DB CONFIG MISSING");
                return;
            }

            builder.Services.AddSingleton(mongoDBSettings);

            //Initialize the Identity authentication system using the DayBuddy roles
            //then add the mongodb settings from reading the appsetings.json
            builder.Services.AddIdentity<DayBuddyUser, DayBuddyRole>()
                .AddMongoDbStores<DayBuddyUser, DayBuddyRole, Guid>(mongoDBSettings.ConnectionString, mongoDBSettings.Name)
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                return new MongoClient(mongoDBSettings.ConnectionString);
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;
            });

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Error/HandleError", "?code={0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapHub<BuddyMatchHub>("/BuddyHub");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        //add a way to insert UserID in the claims and use that instead of Ip
        private static string GetClientIp(HttpContext httpContext)
        {
            // Check if behind a proxy
            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return httpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault() ?? "unknown";
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
