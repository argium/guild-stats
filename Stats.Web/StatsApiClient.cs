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

    public async Task<T?> GetGuildReportChartDataAsync<T>(GuildReportRequest args)
    {
		args.FileType = FileType.Chart;
        var response = await httpClient.PostAsJsonAsync("/reports/guild", args);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<T>();
    }
}
