namespace TBRPicker.Jobs;

public class ImportQueue
{
    private readonly ImportChannel _channel;

    public ImportQueue(ImportChannel channel)
    {
        _channel = channel;
    }

    public async Task EnqueueAsync(ImportJob job)
    {
        await _channel.Writer.WriteAsync(job);
    }
}