using Asp.Versioning;
using FinLedger.Modules.Ledger.Application.Entries.CreateJournalEntry;
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
}
