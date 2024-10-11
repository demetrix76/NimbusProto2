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

        private BetterBindingList<FSItem>? _dirCurrentSource;
        private void DirInit()
        {
            lvDirView.Columns.AddRange([
                new () { DisplayIndex = 0, Text = "Имя", Width = 256 },
                new () { DisplayIndex = 1, Text = "Тип", Width = 192 },
                new () { DisplayIndex = 2, Text = "Создан", Width = 156 },
                new () { DisplayIndex = 3, Text = "Изменён", Width = 156 }
            ]);

            lvDirView.AllowColumnReorder = true;

            //lvDirView.View = View.Tile;
            lvDirView.View = View.Details;
            lvDirView.GridLines = true;

            lvDirView.SmallImageList = new() { ImageSize = new Size(Constants.UI.SmallIconSize, Constants.UI.SmallIconSize) };
            lvDirView.LargeImageList = new() { ImageSize = new Size(Constants.UI.LargeIconSize, Constants.UI.LargeIconSize) };

            lvDirView.SmallImageList.Images.Add(Constants.StockImageKeys.Folder, SystemIcons.GetStockIcon(StockIconId.Folder, Constants.UI.SmallIconSize).ToBitmap());
            lvDirView.SmallImageList.Images.Add(Constants.StockImageKeys.File, SystemIcons.GetStockIcon(StockIconId.DocumentNoAssociation, Constants.UI.SmallIconSize).ToBitmap());
            lvDirView.LargeImageList.Images.Add(Constants.StockImageKeys.Folder, SystemIcons.GetStockIcon(StockIconId.Folder, Constants.UI.LargeIconSize).ToBitmap());
            lvDirView.LargeImageList.Images.Add(Constants.StockImageKeys.File, SystemIcons.GetStockIcon(StockIconId.DocumentNoAssociation, Constants.UI.LargeIconSize).ToBitmap());

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
                case ListChangedType.ItemChanged:
                    DirItemChanged(list[e.NewIndex], e.PropertyDescriptor?.Name);
                    break;
                default:
                    break;
            }
        }

        private void DirItemChanged(FSItem fsItem, string? propertyName)
        {
            switch (propertyName)
            {
                case "Preview":
                {
                    if (fsItem.Preview != null)
                    {
                        lvDirView.SmallImageList!.Images.Add(fsItem.ID, Utils.DownsizedBitmap(fsItem.Preview, Constants.UI.SmallIconSize));
                        lvDirView.LargeImageList!.Images.Add(fsItem.ID, Utils.DownsizedBitmap(fsItem.Preview, Constants.UI.LargeIconSize));
                        lvDirView.Items[fsItem.ID]!.ImageKey = fsItem.ID;
                    }
                    else
                    {
                        lvDirView.SmallImageList!.Images.RemoveByKey(fsItem.ID);
                        lvDirView.LargeImageList!.Images.RemoveByKey(fsItem.ID);
                        lvDirView.Items[fsItem.ID]!.ImageKey = fsItem.DefaultImageKey;
                    }
                    break;
                }
                case "Name":
                    lvDirView.Items[fsItem.ID]!.Text = fsItem.Name;
                    break;
                case "CreationTime":
                    lvDirView.Items[fsItem.ID]!.SubItems[2].Text = fsItem.CreationTime.ToLocalTime().ToString();
                    break;
                case "LastModifiedTime":
                    lvDirView.Items[fsItem.ID]!.SubItems[3].Text = fsItem.LastModifiedTime.ToLocalTime().ToString();
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

            if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                DirRemoveItem(list[e.NewIndex]);
            }
        }

        private void DirAppendItem(FSItem? fsItem)
        {
            if (null == fsItem)
                return;

            var imageKey = lvDirView.SmallImageList!.Images.ContainsKey(fsItem.ID) ? fsItem.ID : fsItem.DefaultImageKey;

            var lvItem = new ListViewItem(fsItem.Name) { Tag = fsItem, ImageKey = imageKey, Name = fsItem.ID };

            lvItem.SubItems.Add(fsItem.DisplayType);
            lvItem.SubItems.Add(fsItem.CreationTime.ToLocalTime().ToString());
            lvItem.SubItems.Add(fsItem.LastModifiedTime.ToLocalTime().ToString());
            lvDirView.Items.Add(lvItem);
        }

        private void DirRemoveItem(FSItem? fsItem)
        {
            if (null == fsItem) return;
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
            
            DirCheckThumbnails();
        }

        private void DirBeginUpdate()
        {
            _app.RefreshCurrentDir(_cancellationTokenSourceUpdate.Token).ContinueWith(task =>
            {
                DirCheckThumbnails();
                // TODO respond to errors                
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        
        private void DirCheckThumbnails()
        {
            foreach (var item in _app.CurrentDir.Children.Where(e => e.PreviewNeedsRefresh))
                if(item is FSFile fsFile)
                    _app.Get(fsFile.PreviewURL!).ContinueWith(task => {
                        if(task.IsCompletedSuccessfully && task.Result != null)
                            item.Preview = Utils.BitmapFromArray(task.Result);
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
            if (e.KeyCode == Keys.Up && 0 != (e.Modifiers & Keys.Alt))
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
                    Text = String.IsNullOrEmpty(d.Name) ? "Диск" : d.Name,
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
            var monitoredDir = _app.CurrentDir;

            try
            {

                while (true)
                {
                    using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
                    await timer.WaitForNextTickAsync(cancellationToken);

                    if (monitoredDir == _app.CurrentDir)
                    {
                        await _app.RefreshCurrentDir(cancellationToken);
                        DirCheckThumbnails();
                    }
                    else
                        break;
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        private void lvDirView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            Console.WriteLine("ItermDrag");

            CancellationTokenSource cancellationSource = new();

            var dataObj = _app.CreateDataObjectForItems(
                lvDirView.SelectedItems.Cast<ListViewItem>().Select(item => item.Tag as FSItem),
                _app.CurrentDir,
                cancellationSource.Token
            );

            if (null == dataObj)
                return;

            dataObj.IsAsynchronous = true;

            var effect = VirtualFiles.DefaultDropSource.DoDragDrop(dataObj, DragDropEffects.Copy);

            if(0 == effect)
                cancellationSource.Cancel();
        }


        #endregion


    }
}
