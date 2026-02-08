using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicTacToeArena.Application.Core.Commands;
using TicTacToeArena.Application.Core.Queries;
using TicTacToeArena.Application.DTOs;

namespace TicTacToeArena.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GamesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("open")]
    public async Task<ActionResult<IReadOnlyList<GameDto>>> OpenGames(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOpenGamesQuery(), ct);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Create([FromBody] CreateGameCommand request, CancellationToken ct)
    {
        var dto = await _mediator.Send(new CreateGameCommand(request.PlayerName), ct);
        return Ok(dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GameDto>> Get([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGameQuery(id), ct);
        
        return Ok(result);
    }

    [HttpPost("{id:guid}/join")]
    public async Task<ActionResult<GameDto>> Join([FromRoute] Guid id, [FromBody] JoinGameCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(new JoinGameCommand(id, request.PlayerName), ct);
       
        return Ok(result);
    }

    [HttpPost("{id:guid}/move")]
    public async Task<ActionResult<GameDto>> Move([FromRoute] Guid id, [FromBody] MakeMoveCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(new MakeMoveCommand(id, request.PlayerName, request.CellIndex), ct);
        
        return Ok(result);
    }

    [HttpPost("{id:guid}/rematch")]
    public async Task<ActionResult<GameDto>> Rematch([FromRoute] Guid id, [FromBody] RematchGameCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RematchGameCommand(id, request.PlayerName), ct);
        
        return Ok(result);
    }

}
