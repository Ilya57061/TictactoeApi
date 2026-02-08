using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Domain.Common;
using TicTacToeArena.Persistence.Data;
using TicTacToeArena.Persistence.Extensions;

namespace TicTacToeArena.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationDbContext(configuration);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        return services;
    }
}