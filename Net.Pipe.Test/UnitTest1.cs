using NamedPipe;
using System.Collections.Concurrent;
using System.Text;
using Xunit.Abstractions;

namespace Common.Test
{
    public class UnitTest1
    {
        public UnitTest1(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
        }

        private class Converter : TextWriter
        {
            ITestOutputHelper _output;
            public Converter(ITestOutputHelper output)
            {
                _output = output;
            }
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
            public override void WriteLine(string message)
            {
                _output.WriteLine(message);
            }
            public override void WriteLine(string format, params object[] args)
            {
                _output.WriteLine(format, args);
            }
            public override void Write(char value)
            {
                throw new NotSupportedException("This text writer only supports WriteLine(string) and WriteLine(string, params object[]).");
            }
        }

        [Theory]
        [InlineData("‚Ò‚å‚ñ‚±½·")]
        public async void Test1(string sendMsg)
        {
            string pipeName = "six-dimention-gate";

            CancellationTokenSource _cancelServer = new();
            CancellationTokenSource _cancelClient = new();

            var recvMsg = "";

            using (NamedPipeServer pipe = new())
            {
                _ = pipe.LaunchAsync(pipeName, msg => { recvMsg = msg; }, _cancelServer.Token);

                using NamedPipeClient client = new();
                await client.SendAsync(pipeName, sendMsg);

                await Task.Delay(2000);

                Console.WriteLine($"{sendMsg}, {(recvMsg == "" ? "[null]" : recvMsg)}");
                Assert.True(sendMsg == recvMsg, $"{sendMsg}, {(recvMsg == "" ? "[null]" : recvMsg)}");
            }
        }

        [Theory]
        [InlineData("‚Ò‚å‚ñ‚±½·")]
        public async void Test1_1(string sendMsg)
        {
            string pipeName = "six-dimention-gate";

            CancellationTokenSource _cancelServer = new();
            CancellationTokenSource _cancelClient = new();

            using (NamedPipeService pipe = new())
            {
                pipe.Received += UnitTest_Recieved;
                _ = pipe.LaunchAsync(pipeName, _cancelServer.Token);

                using NamedPipeClient client = new();
                await client.SendAsync(pipeName, sendMsg);

                await Task.Delay(2000);

                Console.WriteLine($"{sendMsg}, {(_receivedMessage == "" ? "[null]" : _receivedMessage)}");
                Assert.True(sendMsg == _receivedMessage, $"{sendMsg}, {(_receivedMessage == "" ? "[null]" : _receivedMessage)}");
            }
        }

        private string _receivedMessage = "";
        private void UnitTest_Recieved(string sendMsg)
        {
            _receivedMessage += sendMsg;
        }

        [Theory]
        [InlineData("‚Ò‚å‚ñ‚±½·")]
        public async void Test2(string sendMsg)
        {
            string pipeName = "six-dimention-gate";

            CancellationTokenSource _cancelServer = new();
            CancellationTokenSource _cancelClient = new();

            var recvMsg = "";

            using (NamedPipeServer pipe = new())
            {
                var recvTask = pipe.LaunchAsync(pipeName, msg => { }, _cancelServer.Token);

                using NamedPipeClient client = new();
                await client.SendAsync(pipeName, sendMsg);

                await Task.Delay(1000);

                _cancelServer.Cancel();

                await Task.Delay(1000);

                await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(pipeName, "CANCEL"));

                try
                {
                    await recvTask;
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.Message);
                    Assert.True(true == recvTask.IsCanceled);
                }
            }
        }

        [Theory]
        [InlineData("‚Ò‚å‚ñ‚±½·")]
        public async void Test3(string sendMsg)
        {
            string pipeName = "six-dimention-gate";

            CancellationTokenSource _cancelServer = new();
            CancellationTokenSource _cancelClient = new();

            NamedPipeServer pipe = new();
            Task recvTask = pipe.LaunchAsync(pipeName, msg => { }, _cancelServer.Token);

            using NamedPipeClient client = new();
            await client.SendAsync(pipeName, sendMsg);
            await Task.Delay(1000);
            pipe.Dispose();
            await Task.Delay(1000);

            await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(pipeName, "CANCEL"));

            try
            {
                await recvTask;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
                Assert.True(true == recvTask.IsCanceled);
            }
        }

        [Theory]
        [InlineData("‚Ò‚å‚ñ‚±½·", 100)]
        public async void Test4(string sendMsg, int count)
        {
            string pipeName = "six-dimention-gate";

            ConcurrentBag<string> list = [];
            CancellationTokenSource _cancelServer = new();
            CancellationTokenSource _cancelClient = new();

            using NamedPipeServer pipe = new();
            _ = pipe.LaunchAsync(pipeName, list.Add, _cancelServer.Token);

            using NamedPipeClient client = new();
            for (var i = 0; i < count; i++)
            {
                await client.SendAsync(pipeName, sendMsg + i.ToString());
            }

            for (var i = 0; i < count; i++)
            {
                string expected = sendMsg + i.ToString();
                if (!list.Contains(expected))
                {
                    Assert.Fail($"recieved failure! ={expected}");
                }
            }
        }

        [Fact]
        public async void Test5()
        {
            string[] sendMsgs = ["‚Ò‚å‚ñ‚±½·", "‚Ç[‚ñI", "‚¼‚Ë", "ˆÓ–¡‚ª‚í‚©‚è‚Ü‚¹‚ñ"];
            ConcurrentBag<string> list = [];
            string pipeName = "six-dimention-gate";

            CancellationTokenSource _cancelServer = new();
            CancellationTokenSource _cancelClient = new();

            using (NamedPipeServer pipe = new())
            {
                _ = pipe.LaunchAsync(pipeName, list.Add, _cancelServer.Token);

                List<Task> tasks = [];
                foreach (var sendMsg in sendMsgs)
                {
                    using NamedPipeClient client = new();
                    tasks.Add(Task.Run(async () =>
                    {
                        await client.SendAsync(pipeName, sendMsg);
                    }));
                }

                await Task.WhenAll(tasks);
            }

            await Task.Delay(3000);

            if (list.IsEmpty)
            {
                Assert.Fail("list is empty!");
            }
            Console.WriteLine($"{string.Join(",", list)}");
            foreach (var sendMsg in sendMsgs)
            {
                string? recvMsg = list.SingleOrDefault(e => e == sendMsg);
                Console.WriteLine($"{sendMsg}, {recvMsg ?? "[null]"}");
                Assert.True(sendMsg == recvMsg, $"{sendMsg}, {recvMsg ?? "[null]"}");
            }
        }
    }
}