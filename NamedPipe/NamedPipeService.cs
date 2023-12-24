using System;
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
        private bool disposedValue;

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
            Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: start");
            return Task.Run(async () =>
            {
                while (true)
                {
                    using NamedPipeReceiver receiver = new();
                    string? message = await receiver.LaunchAsync(pipeName, ct);

                    Console.WriteLine($"received[{Environment.CurrentManagedThreadId}]: message received.");
                    Received(message ?? "");

                    Console.WriteLine($"received[{Environment.CurrentManagedThreadId}]: restart.");
                }
            }, ct);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // マネージド状態を破棄します (マネージド オブジェクト)
                }
                disposedValue = true;
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
