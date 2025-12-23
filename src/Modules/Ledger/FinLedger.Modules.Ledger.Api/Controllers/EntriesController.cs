using Asp.Versioning;
using FinLedger.Modules.Ledger.Application.Entries.CreateJournalEntry;
using FinLedger.Modules.Ledger.Application.Entries.PostJournalEntry;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Modules.Ledger.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ledger/[controller]")]
[ApiController]
public class EntriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EntriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new journal entry in Draft status.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateJournalEntryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { JournalEntryId = result });
    }

    /// <summary>
    /// Posts (Finalizes) a draft journal entry. Once posted, it becomes immutable.
    /// </summary>
    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(Guid id)
    {
        // Principal Signal: Using a specific command to trigger the state change
        await _mediator.Send(new PostJournalEntryCommand(id));
        return Ok(new { Message = "Journal entry has been successfully posted and is now immutable." });
    }
}
