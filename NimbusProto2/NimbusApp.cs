using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace NimbusProto2
{ 
    internal class MIME
    {
        public const string JSON = "application/json";
    }

    internal class NimbusApp : IDisposable
    {
        private Properties.Settings _settings;
        private HttpClient _httpClient = new HttpClient();


        public NimbusApp(Properties.Settings settings)
        {
            _settings = settings;
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
                .WithAccept(MIME.JSON)
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
    }
}

