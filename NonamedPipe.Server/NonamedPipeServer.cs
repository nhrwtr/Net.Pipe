using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class NonamedPipeServer
    {
        public static void Test()
        {
            Process pipeClient = new();

            pipeClient.StartInfo.FileName = "pipeClient.exe";

            using (AnonymousPipeServerStream pipeServer =
                new(PipeDirection.Out, HandleInheritability.Inheritable))
            {
                Console.WriteLine("[SERVER] Current TransmissionMode: {0}.",
                    pipeServer.TransmissionMode);

                // Pass the client process a handle to the server.
                pipeClient.StartInfo.Arguments =
                    pipeServer.GetClientHandleAsString();
                pipeClient.StartInfo.UseShellExecute = false;
                pipeClient.Start();

                pipeServer.DisposeLocalCopyOfClientHandle();

                try
                {
                    // Read user input and send that to the client process.
                    using StreamWriter sw = new(pipeServer);
                    sw.AutoFlush = true;
                    // Send a 'sync message' and wait for client to receive it.
                    sw.WriteLine("SYNC");
                    pipeServer.WaitForPipeDrain();
                    // Send the console input to the client process.
                    Console.Write("[SERVER] Enter text: ");
                    sw.WriteLine(Console.ReadLine());
                }
                // Catch the IOException that is raised if the pipe is broken
                // or disconnected.
                catch (IOException e)
                {
                    Console.WriteLine("[SERVER] Error: {0}", e.Message);
                }
            }

            pipeClient.WaitForExit();
            pipeClient.Close();
            Console.WriteLine("[SERVER] Client quit. Server terminating.");
        }
    }
}
