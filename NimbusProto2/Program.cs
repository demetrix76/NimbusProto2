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
                // the browser received an URL with the confirmation code,
                // we need to deliver it to the already running app instance
                if(args[0].StartsWith("nimbuskeeper://"))
                {
                    NotifyOtherInstance(args[0]);
                    return;
                }
            }

            bool eventCreatedNew = false;
            using EventWaitHandle instanceGuardEvent = new(false, EventResetMode.AutoReset, "NimbusKeeperSingleInstanceEvent", out eventCreatedNew);

            if(!eventCreatedNew)
            {
                // another instance of this app is already running, notify it and exit
                instanceGuardEvent.Set();
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // register our custom URL scheme and undo the registration at exit
            using var protoHandlerRegistration = new ProtoHandlerRegistration();

            using var settings = Properties.Settings.Default;

            using var app = new NimbusApp(settings);

            var mainWindow = new MainWindow(app);

            // wait for the event in a background thread, activate the app
            // if another instance was launched
            SpawnInstanceGuardThread(instanceGuardEvent, mainWindow);

            Application.Run(mainWindow);
        }

        private static void NotifyOtherInstance(string arg)
        {
            try
            {
                Utils.PipePostSingleMessage("NimbusKeeperApp", arg);
            }
            catch (Exception)
            {
            }
        }

        private static void SpawnInstanceGuardThread(EventWaitHandle instanceGuardEvent, Form formToActivate)
        {
            new Thread(() =>
            {
                while (true)
                {
                    instanceGuardEvent.WaitOne();
                    formToActivate.BeginInvoke(() => formToActivate.Activate());
                }
            })
            { IsBackground = true }.Start();
        }
    }
}