using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using DayBuddy.Models;
using DayBuddy.Settings;
using DayBuddy.Services;

namespace ValorantTournament
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();

            var mongoDBSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

            //Initialize the Identity authentication system using the Tournament roles
            //then add the mongodb settings from reading the appsetings.json
            builder.Services.AddIdentity<DayBuddyUser, DayBuddyRole>().
                AddMongoDbStores<DayBuddyUser, DayBuddyRole, Guid>(mongoDBSettings.ConnectionString, mongoDBSettings.Name);

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

            app.MapHub<ChatHubService>("/chatHub");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

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
