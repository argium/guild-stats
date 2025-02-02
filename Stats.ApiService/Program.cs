using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Stats;
using Stats.DataWriter;
using Stats.Reports;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var report =

app.MapPost("/chart", async (ChartArgs args, [FromServices] IReportProducer<RaidVelocityReportRow> report, [FromServices] IDataWriter writer, CancellationToken ct) =>
{
	IAsyncEnumerable<RaidVelocityReportRow> data = report.GetDataAsync(new ReportArgs(args.GuildName, args.RealmName, args.Region, (int)args.Zone), ct);

	switch (args.Extension)
	{
		case Extension.CSV:
			var stream = new MemoryStream();
			await writer.WriteAsync("RaidVelocity", stream, data, ct);
			return Results.Stream(stream, "text/csv");

		case Extension.JPG:
			throw new NotImplementedException();

		default:
			throw new NotSupportedException();
	}
})
.WithName("GetChart");

app.MapDefaultEndpoints();

app.Run();

record ChartArgs
{
	public required string GuildName { get; set; }
	public required string RealmName { get; set; }
	public required string Region { get; set; }
	public required Zone Zone { get; set; }
	public required Extension Extension { get; set; }
}

enum Zone
{
	NerubarPalace = 38,
}

enum Extension
{
	CSV,

	JPG,
}
