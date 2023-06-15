namespace Practical.ThreadPool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var threadPool = new ThreadPool(3);

            for (int i = 0; i < 200; i++)
            {
                int taskNumber = i;
                threadPool.Execute(() =>
                {
                    var msg = $"Thread: {Thread.CurrentThread.ManagedThreadId} executing Task: {taskNumber}";
                    Console.WriteLine(msg);
                    Thread.Sleep(1000);
                });
            }

            Console.WriteLine("Press any key to stop...");
            Console.ReadLine();

            threadPool.Stop();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}