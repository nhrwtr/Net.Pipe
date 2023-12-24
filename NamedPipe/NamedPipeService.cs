using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NamedPipe
{
    public class NamedPipeService : IDisposable
    {
        private bool _disposedValue;
        private ConcurrentBag<Task> _tasks = [];

        /// <summary>
        /// 名前付きパイプを送受信するサーバーを作成する
        /// </summary>
        public NamedPipeService() { }

        public event Action<string> Received = delegate { };

        /// <summary>
        /// サーバーを立ち上げる
        /// </summary>
        /// <param name="pipeName">パイプ名</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public Task LaunchAsync(string pipeName, CancellationToken ct = default)
        {
            Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: server start");
            return Task.Run(async () =>
            {
                while (true)
                {
                    using NamedPipeReceiver receiver = new();
                    string? message = await receiver.LaunchAsync(pipeName, ct);

                    _tasks.Add(ReceivedAction(message));

                    Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: reopen.");
                }
            }, ct);
        }

        private async Task ReceivedAction(string? message)
        {
            Console.WriteLine($"received proc[{Environment.CurrentManagedThreadId}]: enter.");
            await Task.Delay(3000);
            Received(message ?? "");
            Console.WriteLine($"received proc[{Environment.CurrentManagedThreadId}]: exit.");
        }

        public async Task WaitAllTask()
        {
            Console.WriteLine($"{_tasks.Count}");
            await Task.WhenAll( _tasks );
            _tasks.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // マネージド状態を破棄します (マネージド オブジェクト)
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
