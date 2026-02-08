using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeArena.Infrastructure.Hubs;

public static class ConnectionMap
{
    public static ConcurrentDictionary<string, (Guid? GameId, string PlayerName)> Connections = new();
}

