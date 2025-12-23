using Asp.Versioning;
using FinLedger.Modules.Ledger.Application.Entries.CreateJournalEntry;
using FinLedger.Modules.Ledger.Application.Entries.PostJournalEntry;
using FinLedger.Modules.Ledger.Application.Entries.ReverseJournalEntry;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Modules.Ledger.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ledger/[controller]")]
[ApiController]
public class EntriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EntriesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateJournalEntryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { JournalEntryId = result });
    }

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(Guid id)
    {
        await _mediator.Send(new PostJournalEntryCommand(id));
        return Ok(new { Message = "Journal entry posted successfully." });
    }

   
    [HttpPost("{id:guid}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] string reason)
    {
        var reversalId = await _mediator.Send(new ReverseJournalEntryCommand(id, reason));
        return Ok(new { ReversalEntryId = reversalId, Message = "Reversal entry created successfully." });
    }
}
