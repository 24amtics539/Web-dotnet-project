using System;
using System.Threading.Tasks;
using CampusDeadline.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CampusDeadline.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Automatically create database and tables if not exist
                await context.Database.EnsureCreatedAsync();

                // If any demo user exists from previous run, remove it cleanly
                var demoEmail = "demo@campusdeadline.edu";
                var demoUser = await context.Users.FirstOrDefaultAsync(u => u.Email == demoEmail);
                if (demoUser != null)
                {
                    logger.LogInformation("Removing Academic Demo Account ({Email}) from database...", demoEmail);
                    context.Users.Remove(demoUser);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
            }
        }
    }
}
