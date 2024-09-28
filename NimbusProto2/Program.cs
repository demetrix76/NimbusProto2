using System.IO.Pipes;
using System.Reflection;
using System.Text;

namespace NimbusProto2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            if(args.Length > 0)
            {
                if(args[0].StartsWith("nimbuskeeper://"))
                {
                    NotifyOtherInstance(args[0]);
                    return;
                }
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            using var protoHandlerRegistration = new ProtoHandlerRegistration();

            using var settings = Properties.Settings.Default;

            using var app = new NimbusApp(settings);

            Application.Run(new MainWindow(app));
        }

        private static void NotifyOtherInstance(string arg)
        {
            try
            {
                using var clientStream = new NamedPipeClientStream(".", "NimbusKeeperApp", PipeDirection.Out);
                clientStream.Connect(5000);
                if (clientStream.IsConnected)
                {
                    var bytes = Encoding.UTF8.GetBytes(arg);
                    clientStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}