using Microsoft.AspNetCore.Identity;
using DayBuddy.Models;
using DayBuddy.Settings;
using DayBuddy.Hubs;
using DayBuddy.Services;
using MongoDB.Driver;

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
            builder.Services.AddSingleton<BuddyGroupCacheService>();
            builder.Services.AddScoped<UserService>();

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

            _ = AddRolesInDb(app, builder.Configuration);

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<BuddyMatchHub>("/BuddyHub");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        //Add the default roles from appsettings if there are not present in the db
        private static async Task AddRolesInDb(WebApplication app, ConfigurationManager config)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var roleManager = services.GetRequiredService<RoleManager<DayBuddyRole>>();
                string[] roles = config.GetSection("DefaultRoles").Get<string[]>()!;

                foreach (string role in roles)
                {
                    //if it doesn't exist, add it
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        IdentityResult result = await roleManager.CreateAsync(new DayBuddyRole() { Name = role });
                        if (!result.Succeeded)
                        {
                            Console.WriteLine("ERROR WHEN CREATING ROLE, Program.cs");
                        }
                    };
                }
            }
        }
    }
}
