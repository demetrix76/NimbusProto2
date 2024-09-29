using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace NimbusProto2
{
    public class Utils
    {
        public static void OpenInBrowser(string? url)
        {
            if(url != null)
                Process.Start(new ProcessStartInfo(url) { UseShellExecute=true });
        }

        public static void OpenInBrowser(Uri? uri)
        {
            OpenInBrowser(uri?.ToString());
        }

        public static async Task<string> PipeGetSingleMessage(string pipeName, CancellationToken cancellationToken, int maxLength = 4096)
        {
            using var pipeServerStream = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message);
            await pipeServerStream.WaitForConnectionAsync(cancellationToken);
            byte[] buffer = new byte[maxLength];
            var bytesRead = await pipeServerStream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public static void PipePostSingleMessage(string pipeName, string message, int timeoutMsec = 5000)
        {
            using var pipeClientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            pipeClientStream.Connect(timeoutMsec);
            if(pipeClientStream.IsConnected)
            {
                pipeClientStream.Write(Encoding.UTF8.GetBytes(message));
            }
        }


    }
}
