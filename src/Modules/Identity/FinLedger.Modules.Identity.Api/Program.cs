using FinLedger.Modules.Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Registering Identity Module services, persistence, and MediatR handlers
builder.Services.AddIdentityModule(builder.Configuration);

// Add standard API support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// In a Modular Monolith, the Host project (Ledger.Api) handles Swagger.
// This Program.cs is kept lean for the specific needs of the Identity module.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Authentication and Authorization will be globally managed by the Host, 
// but we keep this here for standalone module integrity.
app.UseAuthorization();

app.MapControllers();

app.Run();
