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
        private readonly NimbusApp _app;
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
                    if (DialogResult.Yes == MessageBox.Show(this, "����� �� ������� ������?", "NimbusKeeper", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
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
                    MessageBox.Show("���... ���-�� ����� �� ���", "���� � �������", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        DirChdir(_app.RootDir);
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
                        lblLogin.Text = "�������";
                        btnLogInOut.Text = "���������...";
                        break;
                    case UIState.LoggedIn:
                        btnLogInOut.Text = "�����";
                        break;
                    case UIState.NotLoggedIn:
                        picAvatar.Image = null;
                        lblLogin.Text = "���� �� ��������";
                        btnLogInOut.Text = "�����";
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

        private BetterBindingList<FSItem>? _dirCurrentSource;
        private void DirInit()
        {
            lvDirView.Columns.AddRange([
                new () { DisplayIndex = 0, Text = "���", Width = 256 },
                new () { DisplayIndex = 1, Text = "���", Width = 192 },
                new () { DisplayIndex = 2, Text = "������", Width = 156 },
                new () { DisplayIndex = 3, Text = "�������", Width = 156 }
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
            if (sender is not BetterBindingList<FSItem> list || sender != _dirCurrentSource)
                return;

            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    DirAppendItem(list[e.NewIndex]);
                    break;
                case ListChangedType.ItemDeleted:
                    // do nothing here as this notification arrives too late
                    break;
                default:
                    break;
            }
        }

        private void DirItemWillBeDeleted(object? sender, ListChangedEventArgs e)
        {
            // here we only get the ItemDeleted notification,
            // **before** the actual deletion occurs
            if (sender is not BetterBindingList<FSItem> list || sender != _dirCurrentSource)
                return;

            if(e.ListChangedType == ListChangedType.ItemDeleted)
            {
                DirRemoveItem(list[e.NewIndex]);
            }
        }

        private void DirAppendItem(FSItem? fsItem)
        {
            if (null == fsItem)
                return;

            var lvItem = new ListViewItem(fsItem.Name) { Tag = fsItem, ImageKey = fsItem.ImageKey, Name = fsItem.ID };

            lvItem.SubItems.Add(fsItem.DisplayType);
            lvItem.SubItems.Add(fsItem.CreationTime.ToLocalTime().ToString());
            lvItem.SubItems.Add(fsItem.LastModifiedTime.ToLocalTime().ToString());
            lvDirView.Items.Add(lvItem);
        }

        private void DirRemoveItem(FSItem? fsItem)
        {
            if(null == fsItem) return;
            lvDirView.Items.RemoveByKey(fsItem.ID);
        }

        private void DirSetSource(BetterBindingList<FSItem> list)
        {
            if (null != _dirCurrentSource)
            {
                _dirCurrentSource.ListChanged -= DirContentsChanged;
                _dirCurrentSource.FireBeforeRemove -= DirItemWillBeDeleted;
            }

            _dirCurrentSource = list;
            _dirCurrentSource.ListChanged += DirContentsChanged;
            _dirCurrentSource.FireBeforeRemove += DirItemWillBeDeleted;

            lvDirView.Items.Clear();
            lvDirView.BeginUpdate();
            foreach (var fsItem in list)
                DirAppendItem(fsItem);
            lvDirView.EndUpdate();
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

            _cancellationTokenSourceUpdate.Cancel();
            _cancellationTokenSourceUpdate = new CancellationTokenSource();

            DirSetSource(target.Children);
            _app.CurrentDir = target;
            DirBeginUpdate();
            DirScheduleUpdate(_cancellationTokenSourceUpdate.Token);
            DirUpdatePathControl();
        }
        
        private void lvDirView_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Up && 0 != (e.Modifiers & Keys.Alt))
            {
                e.Handled = true;
                DirChdir(_app.CurrentDir.Parent);
            }
        }

        private void DirUpdatePathControl()
        {
            pnlPath.Controls.Clear();
            foreach (var btn in _app.CurrentDir.DirectoryChain.Select(d =>
            {
                var btn = new Button()
                {
                    Text = String.IsNullOrEmpty(d.Name) ? "����" : d.Name,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    FlatStyle = FlatStyle.Flat,
                    AutoEllipsis = true
                };
                btn.Click += (s, e) => { this.DirChdir(d); };
                return btn;
            }))
            {
                pnlPath.Controls.Add(btn);
            }
        }

        private async void DirScheduleUpdate(CancellationToken cancellationToken)
        {
            /* PROBLEM: this tends to cause excessive updates in certain cases, e.g. when we chdir to the root dir
             * multiple times within the update timespan, thus making multiple calls to DirScheduleUpdate
             */
            var monitoredDir = _app.CurrentDir;

            while(true)
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
                await timer.WaitForNextTickAsync();

                if (monitoredDir == _app.CurrentDir)
                {
                    await _app.RefreshCurrentDir(cancellationToken);
                }
                else
                    break;
            }
        }


        #endregion


    }
}
