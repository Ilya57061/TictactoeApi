using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Infrastructure.Notifications;

namespace TicTacToeArena.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();
        services.AddScoped<IGameNotifier, GameNotifier>();

        return services;
    }
}
