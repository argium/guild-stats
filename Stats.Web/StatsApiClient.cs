namespace Stats.Web;

public class StatsApiClient(HttpClient httpClient)
{
    public async Task<Stream> GetChartAsync(ChartArgs args)
    {
        var response = await httpClient.PostAsJsonAsync("/chart", args);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStreamAsync();
    }
}

public record ChartArgs
{
	public string GuildName { get; set; } = string.Empty;
	public string RealmName { get; set; } = string.Empty;
	public string Region { get; set; } = "us";
	public Zone Zone { get; set; } = Zone.NerubarPalace;
	public Extension Extension { get; set; } = Extension.CSV;
}

public enum Zone
{
	NerubarPalace = 38,
}

public enum Extension
{
	CSV,

	JPG,
}
