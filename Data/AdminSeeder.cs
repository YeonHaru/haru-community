using System;
using System.Linq;
using System.Threading.Tasks;
using haru_community.Models;
using haru_community.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace haru_community.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var options = provider.GetRequiredService<IOptions<DefaultAdminOptions>>().Value;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
        var email = options.Email?.Trim();
        var password = options.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            logger.LogDebug("Default admin credentials are not configured. Skipping admin seeding.");
            return;
        }

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminRoleName = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            if (!roleResult.Succeeded)
            {
                logger.LogError("Failed to create admin role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Admin role created.");
        }

        var adminUser = await userManager.FindByEmailAsync(email);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, password);
            if (!createResult.Succeeded)
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Admin user {Email} created.", email);
        }

        if (!await userManager.CheckPasswordAsync(adminUser, password))
        {
            var removeResult = await userManager.RemovePasswordAsync(adminUser);
            if (!removeResult.Succeeded)
            {
                logger.LogError("Failed to reset admin password: {Errors}", string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                return;
            }

            var addPasswordResult = await userManager.AddPasswordAsync(adminUser, password);
            if (!addPasswordResult.Succeeded)
            {
                logger.LogError("Failed to set admin password: {Errors}", string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Admin user {Email} password reset.", email);
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);
            if (!addToRoleResult.Succeeded)
            {
                logger.LogError("Failed to add admin user to role: {Errors}", string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Admin user {Email} assigned to role {Role}.", email, adminRoleName);
        }
    }
}
