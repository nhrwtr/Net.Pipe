using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class NamedPipe : IDisposable
    {
        private static readonly int RecvPipeMax = 1;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _disposedValue;

        public Task CreateServerAsync(string pipeName, Action<string> onRecv, CancellationToken ct = default)
        {
            CancellationTokenSource combinedCts = 
                CancellationTokenSource.CreateLinkedTokenSource(ct, _cancellationTokenSource.Token);

            return Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        using (NamedPipeServerStream server = new(pipeName, PipeDirection.InOut, RecvPipeMax,
                            PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly))
                        {
                            Console.WriteLine($"reciver: wait connection... ");
                            await server.WaitForConnectionAsync(combinedCts.Token);

                            Console.WriteLine($"reciver: sender <=> reciver ");
                            using (StreamReader reader = new(server))
                            {
                                // 受信待ち
                                Console.WriteLine($"reciver: read start");
                                var message = await reader.ReadLineAsync();

                                Console.WriteLine($"reciver: reade complete. message is {message ?? "[null]"}");

                                onRecv(message ?? "");
                            }
                        }
                    }
                    catch (IOException ofex)
                    {
                        // クライアントが切断
                        Console.WriteLine("reciver: client disconnected!");
                        Console.WriteLine(ofex.Message);
                    }
                    catch (OperationCanceledException oce)
                    {
                        // パイプサーバーのキャンセル要求(OperationCanceledExceptionをthrowしてTaskが終わると、Taskは「Cancel」扱いになる)
                        Console.WriteLine($"reciver: cancel request from pipe server. {oce.GetType()}");
                        throw;
                    }
                    finally
                    {
                        Console.WriteLine("reciver: server closing.");
                    }
                }
            }, ct);
        }

        public async Task CreateClientAsync(string pipeName, string message)
        {
            await Task.Run(async () =>
            {
                using (NamedPipeClientStream client = new(".", pipeName, PipeDirection.InOut, 
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly, TokenImpersonationLevel.Impersonation))
                {
                    await client.ConnectAsync(1000);

                    using StreamWriter sw = new(client);
                    await sw.WriteLineAsync(message);
                    sw.Flush();

                    Console.WriteLine("  sender: complete");
                }

                Console.WriteLine("  sender: pipe closing.");
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // マネージド状態を破棄します (マネージド オブジェクト)
                    _cancellationTokenSource.Cancel();
                }

                // アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // 大きなフィールドを null に設定します
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
