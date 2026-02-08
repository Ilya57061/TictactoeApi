using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicTacToeArena.Application.Core.Commands;
using TicTacToeArena.Application.Core.Queries;
using TicTacToeArena.Application.DTOs;

namespace TicTacToeArena.Controllers;

[ApiController]
[Route("api/players")]
public sealed class PlayersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlayersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<PlayerDto>> Login([FromBody] UpsertPlayerCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpsertPlayerCommand(request.Name), ct);
        
        return Ok(result);
    }

    [HttpGet("{name}/stats")]
    public async Task<ActionResult<PlayerDto>> Stats([FromRoute] string name, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPlayerStatsQuery(name), ct);
        
        return Ok(result);
    }

}
