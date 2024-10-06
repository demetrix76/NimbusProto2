using System.Web;
using System.Net;
using System.Text.Json;

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
            const string fieldsOfInterest = "_embedded.items.resource_id,_embedded.items.path,_embedded.items.name,_embedded.items.size,_embedded.items.type,_embedded.items.created,_embedded.items.modified," +
                "_embedded.items.mime_type,_embedded.items.preview,_embedded.items.public_url,_embedded.items.public_key";

            var dirToUpdate = _currentDir;

            try
            {
                var request = new RequestBuilder(HttpMethod.Get, Constants.DiskAPIUrl + "resources")
                    .WithAccept(Constants.MIME.JSON)
                    .WithAuthorization(_settings.AccessToken)
                    .WithQuery(
                        ("path", CurrentDir.FullPath),
                        ("preview_size", "S"),
                        ("fields", fieldsOfInterest)
                    ).Build();

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                    return (RequestStatus.AuthorizationError, null);

                var text = await response.Content.ReadAsStringAsync();

                var update = JsonSerializer.Deserialize<YADISK.ResourcesRoot>(text);

                if (update == null)
                    return (RequestStatus.InternalError, "Empty or unreadable response from the server");

                dirToUpdate.UpdateChildren(update._embedded);

                return (RequestStatus.OK, "");
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
        
    }
}

