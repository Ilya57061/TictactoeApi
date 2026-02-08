using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicTacToeArena.Persistence.Data;

namespace TicTacToeArena.Persistence.Extensions;

public static class DatabaseExtension
{
    public static void AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<AppDbContext>(options =>
         options.UseNpgsql(connectionString,
         opt => opt.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        ApplyMigrations(services.BuildServiceProvider());
    }

    private static void ApplyMigrations(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Migration Error: ", ex.Message);
        }
    }
}