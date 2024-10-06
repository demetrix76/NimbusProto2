using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

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

        // gets a single property from a JSON response;
        // don't use repeated calls to this function to get multiple properties, for efficiency reasons
        public static string? GetPropertyFromResponse(string propertyName, string response)
        {
            try
            {
                var document = JsonDocument.Parse(response);
                return document.RootElement.GetProperty(propertyName).GetString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string URIPathCombine(string path1, string path2)
        {
            return path1.TrimEnd('/') + "/" + path2.TrimStart('/');
        }
    }

    // The original BindingList<T> has a fatal flaw:
    // it's ItemDeleted notification is sent after the item has been deleted,
    // so there's little we can do on the UI side unless we keep track of item indices which isn't nice;
    // hence the BetterBindingList<T> class with an extra notification that occurs right before deleting the item.
    public class BetterBindingList<T> : BindingList<T>
    {
        public BetterBindingList() { }
        public BetterBindingList(IList<T> list) : base(list) { }

        protected override void RemoveItem(int index)
        {
            FireBeforeRemove?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, index, index));
            base.RemoveItem(index);
        }

        public event EventHandler<ListChangedEventArgs>? FireBeforeRemove;
    };
}
