using System.Collections.Concurrent;

namespace Practical.ThreadPool
{
    internal class ThreadPool
    {
        private ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private List<Thread> _threads = new List<Thread>();
        private bool _stopped = false;

        public ThreadPool(int threadCount)
        {
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(DoWork);
                thread.Start();
                _threads.Add(thread);
            }
        }

        public void Execute(Action action)
        {
            _queue.Enqueue(action);
        }

        public void Stop()
        {
            Console.WriteLine("Thread Pool is stopping...");
            _stopped = true;
            foreach (var thread in _threads)
            {
                thread.Interrupt();
            }
        }

        public int QueueLength => _queue.Count;

        private void DoWork()
        {
            while (!_stopped)
            {
                try
                {
                    if (_queue.TryDequeue(out var action))
                    {
                        action();
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    var msg = $"Thread: {Thread.CurrentThread.ManagedThreadId} is stopping...";
                    Console.WriteLine(msg);
                }
            }
        }
    }
}
