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
using DayBuddy.Factories;

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
            builder.Services.AddSingleton<IAuthorizationHandler, EmailVerifiedHandler>();
            
            MongoDbConfig? mongoDBSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();
            
            if(mongoDBSettings == null)
            {
                Console.WriteLine("ERROR, DB CONFIG MISSING");
                return;
            }

            builder.Services.AddSingleton(mongoDBSettings);

            //Initialize the Identity authentication system using the DayBuddy roles
            //then add the mongodb settings from reading the appsetings.json
            builder.Services.AddIdentity<DayBuddyUser, DayBuddyRole>().
                AddMongoDbStores<DayBuddyUser, DayBuddyRole, Guid>(mongoDBSettings.ConnectionString, mongoDBSettings.Name);

            //factory method do add custom data in the claims to act as a cache to limit the calls to the db for some data
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<DayBuddyUser>, DayBuddyUserClaimsPrincipalFactory>();

            builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                return new MongoClient(mongoDBSettings.ConnectionString);
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;
            });

            var app = builder.Build();

            //app.UseStatusCodePagesWithRedirects();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapHub<BuddyMatchHub>("/BuddyHub");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
