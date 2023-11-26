using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder Builder = WebApplication.CreateBuilder(args);

Builder.Services.AddEndpointsApiExplorer();
Builder.Services.AddSwaggerGen();
Builder.Services.AddDbContext<AnalyticsDbContext>(Options => { Options.UseNpgsql("Name=ConnectionStrings:AnalyticsDb"); });
Builder.Services.AddHealthChecks().AddDbContextCheck<AnalyticsDbContext>(customTestQuery: async (DbContext, CancellationToken) =>
{
	// Check if all migrations have been applied to the database
	IEnumerable<string> PendingMigrations = await DbContext.Database.GetPendingMigrationsAsync(CancellationToken);
	return !PendingMigrations.Any();
});

WebApplication App = Builder.Build();

if (App.Environment.IsDevelopment())
{
	App.UseSwagger();
	App.UseSwaggerUI();
}

App.MapHealthChecks("/healthz");
App.MapPost("/datarouter/api/v1/public/data",
	async (AnalyticsDbContext DbContext, [FromBody] AnalyticsRequest Request, [FromQuery] string SessionId, [FromQuery] string AppId, [FromQuery] string AppVersion, [FromQuery] string AppEnvironment, [FromQuery] string UserId) =>
	{
		IEnumerable<AnalyticsEvent> Events = Request.Events.Select(Event => new AnalyticsEvent
		{
			SessionId = SessionId,
			AppId = AppId,
			AppVersion = AppVersion,
			AppEnvironment = AppEnvironment,
			UserId = UserId,
			Event = Event
		});

		await DbContext.AddRangeAsync(Events);
		await DbContext.SaveChangesAsync();

		return Results.Created();
	});

App.Run();

internal class AnalyticsRequest
{
	public List<JsonDocument> Events { get; set; } = new();
}

internal class AnalyticsEvent
{
	public int Id { get; set; }
	public string? SessionId { get; set; }
	public string? AppId { get; set; }
	public string? AppVersion { get; set; }
	public string? AppEnvironment { get; set; }
	public string? UserId { get; set; }
	public JsonDocument? Event { get; set; }
}

internal class AnalyticsDbContext : DbContext
{
	public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> Options) : base(Options)
	{
	}

	public DbSet<AnalyticsEvent> Events { get; set; } = null!;
}