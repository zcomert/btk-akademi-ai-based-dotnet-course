using Microsoft.EntityFrameworkCore;
using StoreWeb.Repositories;
using StoreWeb.Services;

namespace StoreWeb.Infrastructure.Extensions;

public static class ApplicationService
{
    public static async Task ConfigureAndSeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<RepositoryContext>();
        await context.Database.MigrateAsync();

        var authService = services.GetRequiredService<IAuthService>();
        await authService.SeedDefaultsAsync();
    }
}
