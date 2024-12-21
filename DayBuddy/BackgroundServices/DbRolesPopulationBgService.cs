using DayBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace DayBuddy.BackgroundServices
{
    /// <summary>
    /// Adds the default roles in the MongoDB database if they are not already added.
    /// </summary>
    public class DbRolesPopulationBgService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public DbRolesPopulationBgService(IServiceProvider services, IConfiguration config)
        {
            _services = services;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<DayBuddyRole>>();
                string[] roles = _config.GetSection("DefaultRoles").Get<string[]>()!;

                foreach (string role in roles)
                {
                    // Check if the role already exists
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var result = await roleManager.CreateAsync(new DayBuddyRole { Name = role });

                        if (!result.Succeeded)
                        {
                            Console.WriteLine($"ERROR WHEN CREATING ROLE: {role}");
                        }
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
