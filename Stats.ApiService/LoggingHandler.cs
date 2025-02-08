namespace Stats;

public class LoggingHandler : DelegatingHandler
{
	private readonly ILogger<LoggingHandler> logger;

	public LoggingHandler(HttpMessageHandler innerHandler, ILogger<LoggingHandler> logger)
        : base(innerHandler)
    {
		this.logger = logger;
	}

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace($"Request: \n{request.ToString()}");
        if (request.Content != null)
        {
            this.logger.LogTrace(await request.Content.ReadAsStringAsync());
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        this.logger.LogTrace($"Response: \n{response.ToString()}");
        if (response.Content != null)
        {
            this.logger.LogTrace(await response.Content.ReadAsStringAsync());
        }

        return response;
    }
}
