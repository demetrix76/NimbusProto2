namespace NimbusProto2
{
    enum UIState
    {
        Offline,
        LoggedIn,
        NotLoggedIn
    }
    public partial class MainWindow : Form
    {
        private NimbusApp _app;
        private UIState _state = UIState.Offline;
        
        internal MainWindow(NimbusApp app)
        {
            InitializeComponent();
            _app = app;
        }

        private void btnLogInOut_Click(object sender, EventArgs e)
        {
            switch (_state)
            {
                case UIState.Offline:
                    checkState();
                    break;
                case UIState.LoggedIn:
                    if(DialogResult.Yes == MessageBox.Show(this, "Выйти из учётной записи?", "NimbusKeeper", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        LogOut();
                    break;
                case UIState.NotLoggedIn:
                    TryLogin();
                    break;
            }
        }

        private void TryLogin()
        {
            var loggingInDialog = new LoggingIn(_app);

            switch (loggingInDialog.ShowDialog())
            {
                case DialogResult.OK:
                    Activate();
                    checkState();
                    break;
                case DialogResult.Abort:
                    MessageBox.Show("Упс... что-то пошло не так", "Вход в аккаунт", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                default:
                    break;
            }
        }

        private void LogOut()
        {
            _app.ClearAccessToken();
            State = UIState.NotLoggedIn;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            checkState();
        }

        private void checkState()
        {
            if (_app.MayHaveAccess)
            {
                _app.GetUserInfo().ContinueWith(task => {
                    try
                    {
                        var userInfo = task.Result;
                        State = UIState.LoggedIn;
                        picAvatar.Image = userInfo.Avatar;
                        lblLogin.Text = userInfo.Login;
                    }
                    catch(Exception e)
                    {
                        if(e.InnerException is HttpRequestException requestException)
                        {
                            if (requestException.StatusCode == System.Net.HttpStatusCode.Unauthorized || requestException.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                _app.ClearAccessToken();
                                State = UIState.NotLoggedIn;
                                return;
                            }
                        }
                        State = UIState.Offline;
                        ScheduleStateCheck();
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
            else
                State = UIState.NotLoggedIn;
        }

        private UIState State
        {
            get { return _state; }
            set
            {
                _state = value;
                switch (_state)
                {
                    case UIState.Offline:
                        lblLogin.Text = "Оффлайн";
                        btnLogInOut.Text = "Повторить...";
                        break;
                    case UIState.LoggedIn:
                        btnLogInOut.Text = "Выйти";
                        break;
                    case UIState.NotLoggedIn:
                        picAvatar.Image = null;
                        lblLogin.Text = "Вход не выполнен";
                        btnLogInOut.Text = "Войти";
                        break;
                }
            }
        }

        private async void ScheduleStateCheck()
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            await timer.WaitForNextTickAsync();
            if (_state == UIState.Offline)
                checkState();
        }
    }
}
