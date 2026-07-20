using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        string[] roleNames = { "Admin", "Staff", "InnovationTeam" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // Seed default admin user
        var adminEmail = "admin@bou.or.ug";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "System Administrator",
                FirstName = "System",
                LastName = "Administrator",
                Title = "Administrator",
                BusinessUnit = "ICT",
                JobTitle = "System Administrator",
                Station = "Head Office",
                AgeBracket = "35-44",
                Gender = "Male",
                IsActive = true,
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}