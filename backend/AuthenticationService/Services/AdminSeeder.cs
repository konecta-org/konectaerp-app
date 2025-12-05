using AuthenticationService.Data;
using AuthenticationService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Services
{
    public class AdminSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            // Create System Admin role if it doesn't exist
            if (!await roleManager.RoleExistsAsync("System Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("System Admin"));
                Console.WriteLine("System Admin role created.");
            }
            
            // Check if admin user already exists
            var adminUser = await userManager.FindByEmailAsync("admin@konecta.com");
            if (adminUser == null)
            {
                // Create admin user
                adminUser = new ApplicationUser
                {
                    UserName = "admin@konecta.com",
                    Email = "admin@konecta.com",
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };
                
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                
                if (result.Succeeded)
                {
                    // Assign System Admin role
                    await userManager.AddToRoleAsync(adminUser, "System Admin");
                    Console.WriteLine("Admin user created successfully!");
                    Console.WriteLine("Email: admin@konecta.com");
                    Console.WriteLine("Password: Admin@123456");
                }
                else
                {
                    Console.WriteLine("Failed to create admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"- {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
                
                // Ensure admin has System Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "System Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "System Admin");
                    Console.WriteLine("System Admin role assigned to existing admin user.");
                }
            }
        }
    }
}
