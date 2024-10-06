using System.ComponentModel;

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

        private CancellationTokenSource _cancellationTokenSourceUpdate = new();

        internal MainWindow(NimbusApp app)
        {
            InitializeComponent();
            _app = app;

            DirInit();
        }

        private void btnLogInOut_Click(object sender, EventArgs e)
        {
            switch (_state)
            {
                case UIState.Offline:
                    checkState();
                    break;
                case UIState.LoggedIn:
                    if (DialogResult.Yes == MessageBox.Show(this, "Выйти из учётной записи?", "NimbusKeeper", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
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
                _app.GetUserInfo().ContinueWith(task =>
                {
                    try
                    {
                        var userInfo = task.Result;
                        State = UIState.LoggedIn;
                        picAvatar.Image = userInfo.Avatar;
                        lblLogin.Text = userInfo.Login;
                        DirBeginUpdate();
                    }
                    catch (Exception e)
                    {
                        if (e.InnerException is HttpRequestException requestException)
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

        #region Directory List operations

        private BindingList<FSItem>? _dirCurrentSource;
        private void DirInit()
        {
            lvDirView.Columns.AddRange([
                new () { DisplayIndex = 0, Text = "Имя", Width = 256 },
                new () { DisplayIndex = 1, Text = "Тип", Width = 192 },
                new () { DisplayIndex = 2, Text = "Создан", Width = 156 },
                new () { DisplayIndex = 3, Text = "Изменён", Width = 156 }
            ]);

            lvDirView.AllowColumnReorder = true;

            lvDirView.View = View.Details;
            lvDirView.GridLines = true;

            lvDirView.SmallImageList = new() { ImageSize = new Size(32, 32) };
            lvDirView.LargeImageList = new() { ImageSize = new Size(128, 128) };

            lvDirView.SmallImageList.Images.Add(Constants.StockImageKeys.Folder, SystemIcons.GetStockIcon(StockIconId.Folder, 32).ToBitmap());
            lvDirView.SmallImageList.Images.Add(Constants.StockImageKeys.File, SystemIcons.GetStockIcon(StockIconId.DocumentNoAssociation, 32).ToBitmap());
            lvDirView.LargeImageList.Images.Add(Constants.StockImageKeys.Folder, SystemIcons.GetStockIcon(StockIconId.Folder, 256).ToBitmap());
            lvDirView.LargeImageList.Images.Add(Constants.StockImageKeys.File, SystemIcons.GetStockIcon(StockIconId.DocumentNoAssociation, 256).ToBitmap());

            DirSetSource(_app.CurrentDir.Children);
        }
        private void DirContentsChanged(object? sender, ListChangedEventArgs e)
        {
            if (sender is not BindingList<FSItem> list || sender != _dirCurrentSource)
                return;

            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    DirAppendItem(list[e.NewIndex]);
                    break;
                default:
                    break;
            }
        }

        private void DirAppendItem(FSItem? fsItem)
        {
            if (null == fsItem)
                return;

            var lvItem = new ListViewItem(fsItem.Name) { Tag = fsItem, ImageKey = fsItem.ImageKey };

            lvItem.SubItems.Add(fsItem.DisplayType);
            lvItem.SubItems.Add(fsItem.CreationTime.ToLocalTime().ToString());
            lvItem.SubItems.Add(fsItem.LastModifiedTime.ToLocalTime().ToString());
            lvDirView.Items.Add(lvItem);
        }

        private void DirSetSource(BindingList<FSItem> list)
        {
            if (null != _dirCurrentSource)
                _dirCurrentSource.ListChanged -= DirContentsChanged;

            _dirCurrentSource = list;
            _dirCurrentSource.ListChanged += DirContentsChanged;
            lvDirView.Items.Clear();
            foreach (var fsItem in list)
                DirAppendItem(fsItem);
        }

        private void DirBeginUpdate()
        {
            _app.RefreshCurrentDir(_cancellationTokenSourceUpdate.Token).ContinueWith(task =>
            {
                // TODO respond to errors                
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void lvDirView_ItemActivate(object sender, EventArgs e)
        {
            DirChdir((from item in lvDirView.SelectedItems.Cast<ListViewItem>()
                      where item.Tag is FSDirectory
                      select item.Tag as FSDirectory).FirstOrDefault());
        }
        private void DirChdir(FSDirectory? target)
        {
            if (null == target) return;

            DirSetSource(target.Children);
            _app.CurrentDir = target;
            DirBeginUpdate();
        }
        
        private void lvDirView_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Up && 0 != (e.Modifiers & Keys.Alt))
            {
                e.Handled = true;
                DirChdir(_app.CurrentDir.Parent);
            }
        }


        #endregion


    }
}
