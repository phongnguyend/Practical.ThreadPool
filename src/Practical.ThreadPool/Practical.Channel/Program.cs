using System.Threading.Channels;

namespace Practical.Channel;

internal class Program
{
    static async Task Main(string[] args)
    {
        BoundedChannelOptions options = new(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        Channel<Func<CancellationToken, ValueTask>> channel = System.Threading.Channels.Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        var cancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(async () => await ProcessTaskQueueAsync(channel, cancellationTokenSource.Token));
        await MonitorAsync(channel, cancellationTokenSource);
    }

    private static async Task ProcessTaskQueueAsync(Channel<Func<CancellationToken, ValueTask>> channel, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Func<CancellationToken, ValueTask>? workItem = await channel.Reader.ReadAsync(stoppingToken);

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

    private static async ValueTask MonitorAsync(Channel<Func<CancellationToken, ValueTask>> channel, CancellationTokenSource cancellationTokenSource)
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
                await channel.Writer.WriteAsync(async (token) =>
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