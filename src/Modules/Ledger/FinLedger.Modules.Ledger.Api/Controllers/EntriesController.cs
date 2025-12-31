using Asp.Versioning;
using FinLedger.Modules.Ledger.Application.Entries.CreateJournalEntry;
using FinLedger.Modules.Ledger.Application.Entries.PostJournalEntry;
using FinLedger.Modules.Ledger.Application.Entries.ReverseJournalEntry;
using MediatR;
using Microsoft.AspNetCore.Authorization; // Added for Security
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Modules.Ledger.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ledger/[controller]")]
[ApiController]
[Authorize] // All endpoints in this controller require a valid JWT token
public class EntriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EntriesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Creates a new journal entry in Draft status.
    /// Access: Accountants or Admins.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AccountantAccess")] // Enforcing Tenant-aware RBAC
    public async Task<IActionResult> Create(CreateJournalEntryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { JournalEntryId = result });
    }

    /// <summary>
    /// Finalizes a journal entry. Once posted, it becomes immutable.
    /// Access: Accountants or Admins.
    /// </summary>
    [HttpPost("{id:guid}/post")]
    [Authorize(Policy = "AccountantAccess")]
    public async Task<IActionResult> Post(Guid id)
    {
        await _mediator.Send(new PostJournalEntryCommand(id));
        return Ok(new { Message = "Journal entry posted successfully." });
    }

    /// <summary>
    /// Performs a full reversal of a posted journal entry for correction.
    /// Access: Restricted to Admins for audit integrity.
    /// </summary>
    [HttpPost("{id:guid}/reverse")]
    [Authorize(Policy = "AdminOnly")] // Higher security for critical audit corrections
    public async Task<IActionResult> Reverse(Guid id, [FromBody] string reason)
    {
        var reversalId = await _mediator.Send(new ReverseJournalEntryCommand(id, reason));
        return Ok(new { ReversalEntryId = reversalId, Message = "Reversal entry created successfully." });
    }
}
