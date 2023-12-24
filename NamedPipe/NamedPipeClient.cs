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
    /// 名前付きパイプのクライアントクラス
    /// </summary>
    public class NamedPipeClient : IDisposable
    {
        private bool _disposedValue;

        /// <summary>
        /// 名前付きパイプを送受信するクライアントを作成する
        /// </summary>
        public NamedPipeClient() { }

        /// <summary>
        /// メッセージ文字列を送信する
        /// </summary>
        /// <param name="pipeName">パイプ名</param>
        /// <param name="message">送信メッセージ</param>
        /// <returns></returns>
        public async Task SendAsync(string pipeName, string message)
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
                }
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
