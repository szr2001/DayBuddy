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

namespace DayBuddy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
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
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("EmailVerified", policy =>
                    policy.Requirements.Add(new EmailVerifiedRequirement()));
            });
            builder.Services.AddSingleton<IAuthorizationHandler, EmailVerifiedHandler>();
            
            MongoDbConfig? mongoDBSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();
            
            if(mongoDBSettings == null)
            {
                Console.WriteLine("ERROR, DB CONFIG MISSING");
                return;
            }

            builder.Services.AddSingleton(mongoDBSettings);

            //Initialize the Identity authentication system using the Tournament roles
            //then add the mongodb settings from reading the appsetings.json
            builder.Services.AddIdentity<DayBuddyUser, DayBuddyRole>().
                AddMongoDbStores<DayBuddyUser, DayBuddyRole, Guid>(mongoDBSettings.ConnectionString, mongoDBSettings.Name);

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
