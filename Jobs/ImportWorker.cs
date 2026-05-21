namespace TBRPicker.Jobs;

public class ImportWorker : BackgroundService
{
    private readonly ImportChannel _channel;
    private readonly ILogger<ImportWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ImportWorker(ImportChannel channel, ILogger<ImportWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Processing import for user {UserId}, shelves: {Shelves}",
                    job.UserId, string.Join(", ", job.Shelves));

                // We'll plug the actual scraping logic in here next
                await Task.Delay(500, stoppingToken); // placeholder

                _logger.LogInformation("Import complete for user {UserId}", job.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import failed for user {UserId}", job.UserId);
            }
        }
    }
}