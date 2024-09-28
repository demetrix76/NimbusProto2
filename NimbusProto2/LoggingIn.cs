namespace NimbusProto2
{
    public partial class LoggingIn : Form
    {
        private NimbusApp _app;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        internal LoggingIn(NimbusApp app)
        {
            InitializeComponent();
            _app = app;
        }

        private void LoggingIn_Load(object sender, EventArgs e)
        {
            _app.LogIn(_cancellationTokenSource.Token).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                    DialogResult = DialogResult.OK;
                else if (task.IsFaulted)
                    DialogResult = DialogResult.Abort;
                else
                    DialogResult = DialogResult.Cancel;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;
            _cancellationTokenSource.Cancel();
        }
    }
}
