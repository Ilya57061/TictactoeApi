using MediatR;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.Common.Errors;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Domain.Entities;

namespace TicTacToeArena.Application.Core.Commands;

public sealed record UpsertPlayerCommand(string Name) : IRequest<PlayerDto>;

public sealed class UpsertPlayerCommandHandler : IRequestHandler<UpsertPlayerCommand, PlayerDto>
{
    private readonly IRepository<Player, Guid> _players;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertPlayerCommandHandler(IRepository<Player, Guid> players, IUnitOfWork unitOfWork)
    {
        _players = players;
        _unitOfWork = unitOfWork;
    }

    public async Task<PlayerDto> Handle(UpsertPlayerCommand request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new AppException("INVALID_NAME", "Name is required.");

        var existing = await _players.FirstOrDefaultAsync(
            p => p.Name.ToLower() == name.ToLower(),
            asNoTracking: false,
            ct: ct);

        if (existing is null)
        {
            existing = new Player(name);

            await _players.AddAsync(existing, ct);
        }
       
        await _unitOfWork.SaveChangesAsync(ct);

        return existing.ToDto();
    }
}