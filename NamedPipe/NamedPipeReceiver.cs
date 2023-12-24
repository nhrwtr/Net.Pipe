using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipe
{
    /// <summary>
    /// 名前付きパイプのサーバークラス
    /// </summary>
    public class NamedPipeReceiver : IDisposable
    {
        private static readonly int RecvPipeMax = 1;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _disposedValue;

        /// <summary>
        /// 名前付きパイプを送受信するサーバーを作成する
        /// </summary>
        public NamedPipeReceiver() { }

        /// <summary>
        /// サーバーを立ち上げる
        /// </summary>
        /// <param name="pipeName">パイプ名</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public Task<string?> LaunchAsync(string pipeName, CancellationToken ct = default)
        {
            CancellationTokenSource combinedCts = 
                CancellationTokenSource.CreateLinkedTokenSource(ct, _cancellationTokenSource.Token);

            return new Promise<string?>(async (resolve, reject) =>
            {
                try
                {
                    using (NamedPipeServerStream server = new(pipeName, PipeDirection.InOut, RecvPipeMax,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly))
                    {
                        Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: pipe connection waiting... ");
                        await server.WaitForConnectionAsync(combinedCts.Token);

                        Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: sender <=> receiver ");
                        using (StreamReader reader = new(server))
                        {
                            // 受信待ち
                            Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: read start");
                            var message = await reader.ReadLineAsync();

                            Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: read complete. message is {message ?? "[null]"}");

                            // 受信応答イベントを実行
                            resolve(message);
                        }
                    }
                }
                catch (IOException ioe)
                {
                    // クライアントが切断
                    Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: client already disconnected!");
                    reject(ioe);
                }
                catch (OperationCanceledException oce)
                {
                    // パイプサーバーのキャンセル要求(OperationCanceledExceptionをthrowしてTaskが終わると、Taskは「Cancel」扱いになる)
                    Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: cancel request from pipe server. {oce.GetType()}");
                    throw;
                }
                catch (Exception e)
                {
                    reject(e);
                }
                finally
                {
                    Console.WriteLine($"receiver[{Environment.CurrentManagedThreadId}]: pipe stop.");
                }
            });
        }

        /// <summary>
        /// 破棄メソッド
        /// </summary>
        /// <param name="disposing">マネージドオブジェクトを破棄するかどうか</param>
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

        /// <summary>
        /// 破棄メソッド
        /// </summary>
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
