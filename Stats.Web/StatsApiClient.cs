using Stats.Domain;

namespace Stats.Web;

public class StatsApiClient(HttpClient httpClient)
{
    public async Task<Stream> GetGuildReportCsvAsync(GuildReportRequest args)
    {
		args.FileType = FileType.CSV;
        var response = await httpClient.PostAsJsonAsync("/reports/guild", args);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStreamAsync();
    }

    public async Task<Response<ChartDataResponse?>> GetGuildReportChartDataAsync(GuildReportRequest args)
    {
		HttpResponseMessage? response = null;
		args.FileType = FileType.Chart;
		try {
			response = await httpClient.PostAsJsonAsync("/reports/guild", args);
			response.EnsureSuccessStatusCode();
			// return await response.Content.ReadFromJsonAsync<T>();

			return new() { Value = await response.Content.ReadFromJsonAsync<ChartDataResponse>() };
		}
		catch (HttpRequestException hex) when (response != null)
		{
			var problem = await response.Content.ReadFromJsonAsync<ClientProblemDetails>();
			return new () { Error = problem };
		}
    }
}

public record Response<T>
{
	public T? Value { get; set; }
	public ClientProblemDetails? Error { get; set; }
}

public class ClientProblemDetails
{
    public string Type { get; set; } = default!;

    public string Title { get; set; } = default!;

    public int Status { get; set; } = default!;

    public string Detail { get; set; } = default!;

    public string Instance { get; set; } = default!;
}
