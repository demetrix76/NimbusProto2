using System.Web;
using System.Net;
using System.Text.Json;
using VirtualFiles;

using IStream = System.Runtime.InteropServices.ComTypes.IStream;
using NimbusProto2.YADISK;

namespace NimbusProto2
{
    internal enum RequestStatus
    {
        OK,
        AuthorizationError,
        ConnectionError,
        InternalError
    }

    internal class NimbusApp : IDisposable
    {
        private Properties.Settings _settings;
        private HttpClient _httpClient = new HttpClient();
        private FSDirectory _rootDir = new("Root", null);
        private FSDirectory _currentDir;

        public NimbusApp(Properties.Settings settings)
        {
            _settings = settings;
            _currentDir = _rootDir;
        }
        public void Dispose() {}

        public bool MayHaveAccess {  get { return !String.IsNullOrEmpty(_settings.AccessToken); } }

        public void ClearAccessToken()
        {
            _settings.AccessToken = "";
        }

        public async Task<UserInfo> GetUserInfo()
        {
            var infoRequest = new RequestBuilder(HttpMethod.Get, "https://login.yandex.ru/info")
                .WithQuery(("format", "json"))
                .WithAccept(Constants.MIME.JSON)
                .WithAuthorization(_settings.AccessToken)
                .Build();

            var infoResponse = await _httpClient.SendAsync(infoRequest);
            infoResponse.EnsureSuccessStatusCode();

            var infoText = await infoResponse.Content.ReadAsStringAsync();
            Console.WriteLine(infoText);

            var userLogin = Utils.GetPropertyFromResponse("login", infoText) ?? "unknown";
            var avatarId = Utils.GetPropertyFromResponse("default_avatar_id", infoText);

            System.Drawing.Image? avatar = null;

            if (avatarId != null)
            {
                var imageRequest = new HttpRequestMessage(HttpMethod.Get, $"https://avatars.yandex.net/get-yapic/{avatarId}/islands-75");
                var imageResponse = await _httpClient.SendAsync(imageRequest);
                if (imageResponse.IsSuccessStatusCode)
                {
                    var imageData = await imageResponse.Content.ReadAsByteArrayAsync();
                    avatar = Image.FromStream(new MemoryStream(imageData));
                }
            }
            return new UserInfo() { Login = userLogin, Avatar = avatar };
        }

        public async Task LogIn(CancellationToken cancellationToken)
        {
            var readTask = Utils.PipeGetSingleMessage("NimbusKeeperApp", cancellationToken);//  ReadResponseFromPipe(cancellationToken);

            // open yandex auth page

            var browserRequest = new RequestBuilder(HttpMethod.Get, "https://oauth.yandex.ru/authorize")
                .WithQuery(
                    ("response_type", "code"),
                    ("client_id", _settings.ClientID),
                    ("device_id", _settings.DeviceId),
                    ("device_name", _settings.DeviceName),
                    ("redirect_uri", _settings.ConfirmationURI),
                    ("force_confirm", "yes")
                ).Build();

            Utils.OpenInBrowser(browserRequest.RequestUri);

            // wait for confirmation code
            var redirectedURL = await readTask;

            var confirmationCode = HttpUtility.ParseQueryString(new Uri(redirectedURL).Query)["code"];

            Console.WriteLine($"Confirmation code: {confirmationCode}");

            if (confirmationCode == null)
                throw new Exception("Server did not respond with a confirmation code");

            var authRequest = new RequestBuilder(HttpMethod.Post, "https://oauth.yandex.ru/token")
                   .WithHeaders(("Accept", "application/json"))
                   .WithFormContent(
                        ( "grant_type", "authorization_code" ),
                        ( "code", confirmationCode ),
                        ( "client_id", _settings.ClientID ),
                        ( "client_secret", _settings.ClientSecret ),
                        ( "device_id", _settings.DeviceId ),
                        ( "device_name", _settings.DeviceName )
                    ).Build();

            var authResponse = await _httpClient.SendAsync(authRequest);

            var status = authResponse.StatusCode;

            var responseText = await authResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Auth response: ${responseText}");

            authResponse.EnsureSuccessStatusCode();

            var accessToken = Utils.GetPropertyFromResponse("access_token", responseText);
                        
            _settings.AccessToken = accessToken;
        }

        public FSDirectory RootDir { get => _rootDir; } 

        public FSDirectory CurrentDir { get => _currentDir; set => _currentDir = value; }

        public async Task<(RequestStatus, string?)> RefreshCurrentDir(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Scanning {_currentDir.FullPath}");

            try
            {
                return (await UpdateDirectory(CurrentDir, cancellationToken), "");
            }
            catch(HttpRequestException e)
            {
                return (RequestStatus.ConnectionError, e.Message);
            }
            catch (Exception e)
            {
                return (RequestStatus.InternalError, e.Message);
            }
        }

        /* This method is deceivingly synchronous, and it is expected to return quickly,
         * not causing any delays to the UI; however, under the hood, it starts an
         * asynchronous operation to collect the full subtrees of the selected items.
         * VFDO's IDataObject methods will in fact wait for the completion of this
         * asynchronous operation, but this wait will occur on the COM pool thread,
         * effectively blocking the drop target application if it uses the
         * virtual file data (d'n'd within our app should not use these).
         * For IStreams, additional asynchronous tasks will be started.
         */
        public VFDO? CreateDataObjectForItems(IEnumerable<FSItem?> items, FSDirectory sourceDir, CancellationToken cancellationToken)
        {
            // TODO delete the ShallowCopy()?
            // take a snapshot of the selected items (make shallow copies, excluding directories' children)
            //items.Select(item => item.GetShallowCopy()).ToArray();
            // maybe we just need a list of full disk paths and paths relative to the source dir?
            // or maybe it's best to reuse the FSItem code?

            //foreach (var item in snapshot)
            //    item.Walk(item => Console.WriteLine(item.PathRelativeTo(sourceDir)));

            var sourceDirCopy = sourceDir;//.GetShallowCopy();
                        
            var snapshot = items.Where(item => item != null)
                .Select(item => item!.GetShallowCopy()).ToArray();

            if (null == snapshot || snapshot.Length == 0)
                return null;

            List<FSItem> readyList = [];
            bool failed = false;

            Func<IEnumerable<VirtualFiles.FileSource>> fileListSource = () =>
            {
                // this will be called by the COM system on an arbitrary thread
                // wait for the collectSubtree tasks to finish

                FSItem[] itemsToProcess;

                lock(readyList)
                {
                    while(readyList.Count == 0 && ! failed)
                        Monitor.Wait(readyList);

                    if (failed)
                        return [];

                    itemsToProcess = [.. readyList];
                }

                List<VirtualFiles.FileSource> result = [];

                foreach(var item in itemsToProcess)
                {
                    item.Walk(sourceItem => { 
                        if(sourceItem is FSDirectory sourceDir)
                        {
                            if(sourceDir.Children.Count == 0)
                            {
                                // need to create the directory explicitly as it has no files that would trigger its implicit creation
                                result.Add(new FileSource { 
                                    IsDirectory = true,
                                    Name = sourceDir.PathRelativeTo(sourceDirCopy),
                                    Size = 0,
                                    LastModified = sourceDir.LastModifiedTime,
                                    StreamSource = null
                                });
                            }
                        }
                        else if(sourceItem is FSFile sourceFile)
                        {
                            var name = sourceFile.PathRelativeTo(sourceDirCopy);

                            var accessToken = _settings.AccessToken;
                            var diskPath = sourceFile.FullPath;

                            result.Add(new FileSource
                            {
                                Name = sourceFile.PathRelativeTo(sourceDirCopy),
                                LastModified = sourceFile.LastModifiedTime,
                                Size = sourceFile.Size,
                                StreamSource = () => CreateStreamForFile(accessToken, diskPath, CancellationToken.None)
                            });
                        }
                    });
                }

                return result;
            };

            Func<Task> collectSubtrees = async () => {
                // TODO provide UI feedback, let the user know we're busy with something...
                // NOTE: this is executed on the main thread, so it's safe to inteact with the UI
                try
                {
                    await PopulateSubtrees(snapshot, cancellationToken);

                    lock (readyList)
                    {
                        readyList.AddRange(snapshot);
                        Monitor.Pulse(readyList);
                    }
                }
                catch(Exception)
                {
                    lock(readyList)
                    {
                        failed = true;
                    }
                }
            };

            collectSubtrees().ContinueWith(t => { });
            
            return new VFDO(fileListSource);
        }

        private async Task PopulateSubtrees(FSItem[] items, CancellationToken cancellationToken)
        {
            foreach (var item in items)
            {
                if (item is FSDirectory fsDir)
                {
                    await UpdateDirectory(fsDir, cancellationToken);
                    await PopulateSubtrees([.. fsDir.Children], cancellationToken);
                }
            }
        }

        private async Task<RequestStatus> UpdateDirectory(FSDirectory directory, CancellationToken cancellationToken)
        {
            const string fieldsOfInterest = "_embedded.items.resource_id,_embedded.items.path,_embedded.items.name,_embedded.items.size,_embedded.items.type,_embedded.items.created,_embedded.items.modified," +
                "_embedded.items.mime_type,_embedded.items.preview,_embedded.items.public_url,_embedded.items.public_key";

            var request = new RequestBuilder(HttpMethod.Get, Constants.DiskAPIUrl + "resources")
                    .WithAccept(Constants.MIME.JSON)
                    .WithAuthorization(_settings.AccessToken)
                    .WithQuery(
                        ("path", directory.FullPath),
                        ("preview_size", "S"),
                        ("fields", fieldsOfInterest)
                    ).Build();

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                return RequestStatus.AuthorizationError;

            response.EnsureSuccessStatusCode();

            var text = await response.Content.ReadAsStringAsync(cancellationToken);

            var update = JsonSerializer.Deserialize<YADISK.ResourcesRoot>(text) ?? throw new Exception("Empty or unreadable response from the server");
            
            directory.UpdateChildren(update._embedded);

            return RequestStatus.OK;
        }
        
        /* Create an IStream and immediately start supplying it with data;
         * This method is synchronous, but internally it starts an asynchronous task pumping data from an 
         * HTTP stream into the IStream
         */
        // this will be called on the pool thread, so we copy all the required data
        private static IStream? CreateStreamForFile(string accessToken, string diskPath, CancellationToken cancellationToken)
        {
            var virtualStream = new VirtualStream();

            async Task ReadStream()
            {
                try
                {
                    using var httpClient = new HttpClient();

                    var infoRequest = new RequestBuilder(HttpMethod.Get, Constants.DiskAPIUrl + "resources/download")
                            .WithAccept(Constants.MIME.JSON)
                            .WithAuthorization(accessToken)
                            .WithQuery(("path", diskPath))
                            .Build();

                    var infoResponse = await httpClient.SendAsync(infoRequest, cancellationToken);
                    infoResponse.EnsureSuccessStatusCode();

                    var responseText = await infoResponse.Content.ReadAsStringAsync(cancellationToken);

                    // can it ever happen that we'll have templated = true?
                    var response = JsonSerializer.Deserialize<DownloadResponse>(responseText);

                    if (response?.href == null)
                        throw new Exception("Download error");

                    var downloadRequest = new RequestBuilder(HttpMethod.Get, response.href)
                        .WithAuthorization(accessToken)
                        .Build();

                    var downloadResponse = await httpClient.SendAsync(downloadRequest, cancellationToken);
                    downloadResponse.EnsureSuccessStatusCode();

                    var responseStream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);

                    const int blockSize = 256 * 1024;
                    var buffer = new byte[blockSize];

                    while (true)
                    {
                        int read = await responseStream.ReadAsync(buffer, 0, blockSize);
                        if (read == 0) // end of data   
                        {
                            virtualStream.Push([]);
                            break;
                        }
                        else
                        {
                            var block = new byte[read];
                            Array.Copy(buffer, 0, block, 0, read);
                            virtualStream.Push(block);
                        }
                    }

                }
                catch(Exception)
                {
                    virtualStream.SignalError();
                }
            }

            Task.Run(ReadStream);

            return virtualStream;
        }
    }
}

