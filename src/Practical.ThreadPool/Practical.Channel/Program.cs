using System.Threading.Channels;

namespace Practical.Channel;

internal class Program
{
    static void Main(string[] args)
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        var channel = System.Threading.Channels.Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        var cancellationTokenSource = new CancellationTokenSource();

        Task.WaitAll(new[] { ProcessTaskQueueAsync(channel, cancellationTokenSource.Token), MonitorAsync(channel, cancellationTokenSource) });
    }

    private static async Task ProcessTaskQueueAsync(ChannelReader<Func<CancellationToken, ValueTask>> channelReader, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Func<CancellationToken, ValueTask>? workItem = await channelReader.ReadAsync(stoppingToken);

                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private static async Task MonitorAsync(ChannelWriter<Func<CancellationToken, ValueTask>> channelWriter, CancellationTokenSource cancellationTokenSource)
    {
        int workItemCount = 0;
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            var keyStroke = Console.ReadKey();
            if (keyStroke.Key == ConsoleKey.W)
            {
                workItemCount++;
                int workItemId = workItemCount;
                Console.WriteLine("Queuing work item {0}", workItemId);
                await channelWriter.WriteAsync(async (token) =>
                {
                    Console.WriteLine("Work item {0} is starting.", workItemId);
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                    Console.WriteLine("Work item {0} is completed.", workItemId);
                });
            }
            else if (keyStroke.Key == ConsoleKey.Q)
            {
                cancellationTokenSource.Cancel();
            }
        }
    }
}