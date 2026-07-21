using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Apply versioned migrations instead of EnsureCreated. EnsureCreated bypasses
        // migration history and makes later production schema upgrades unreliable.
        await context.Database.MigrateAsync();

        // Seed roles
        string[] roleNames = { "Admin", "Staff", "InnovationTeam" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // Do not create a predictable administrator password in source code.
        // The first administrator must be provisioned through the documented,
        // environment-specific bootstrap command added at the application layer.
    }
}
