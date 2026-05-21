using System.Threading.Channels;

namespace TBRPicker.Jobs;

public class ImportChannel
{
    private readonly Channel<ImportJob> _channel =
        Channel.CreateUnbounded<ImportJob>();

    public ChannelWriter<ImportJob> Writer => _channel.Writer;
    public ChannelReader<ImportJob> Reader => _channel.Reader;
}