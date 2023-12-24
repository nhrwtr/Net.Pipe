using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class NonamedPipeClient
    {
        public static void Start(string[] args)
        {
            if (args.Length > 0)
            {
                using PipeStream pipeClient =
                    new AnonymousPipeClientStream(PipeDirection.In, args[0]);
                Console.WriteLine("[CLIENT] Current TransmissionMode: {0}.",
                   pipeClient.TransmissionMode);

                using StreamReader sr = new(pipeClient);
                // Display the read text to the console
                string? temp;

                // Wait for 'sync message' from the server.
                do
                {
                    Console.WriteLine("[CLIENT] Wait for sync...");
                    temp = sr.ReadLine();
                }
                while (!temp?.StartsWith("SYNC") ?? false);

                // Read the server data and echo to the console.
                while ((temp = sr.ReadLine()) != null)
                {
                    Console.WriteLine("[CLIENT] Echo: " + temp);
                }
            }
            Console.Write("[CLIENT] Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
