using Stats.Domain;

namespace Stats.Web;

public class StatsApiClient(HttpClient httpClient)
{
    public async Task<Stream> GetChartAsync(GuildReportRequest args)
    {
        var response = await httpClient.PostAsJsonAsync("/reports/guild", args);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStreamAsync();
    }
}
