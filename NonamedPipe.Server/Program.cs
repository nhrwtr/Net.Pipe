// See https://aka.ms/new-console-template for more information

using Server;

//NonamedPipeServer.Test();

string pipeName = "six-dimention-gate";

CancellationTokenSource _cancelServer = new CancellationTokenSource();
CancellationTokenSource _cancelClient = new CancellationTokenSource();

var sendMsg = "ぴょんこスキ";
var recvMsg = "";

using (NamedPipe pipe = new())
{
    _ = pipe.CreateServerAsync(pipeName, msg => { recvMsg = msg; }, _cancelServer.Token);

    await pipe.CreateClientAsync(pipeName, sendMsg);
    await pipe.CreateClientAsync(pipeName, sendMsg);
    await pipe.CreateClientAsync(pipeName, sendMsg);
    await pipe.CreateClientAsync(pipeName, sendMsg);

    await Task.Delay(2000);

    Console.WriteLine(sendMsg, recvMsg);
}