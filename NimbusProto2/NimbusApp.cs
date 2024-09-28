using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace NimbusProto2
{ 
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
            var infoRequest = new HttpRequestMessage(HttpMethod.Get, "https://login.yandex.ru/info?format=json");
            infoRequest.Headers.Add("Accept", "application/json");
            infoRequest.Headers.Add("Authorization", $"OAuth {_settings.AccessToken}");

            var infoResponse = await _httpClient.SendAsync(infoRequest);
            infoResponse.EnsureSuccessStatusCode();

            var infoText = await infoResponse.Content.ReadAsStringAsync();
            Console.WriteLine(infoText);

            var userLogin = getPropertyFromResponse("login", infoText) ?? "unknown";
            var avatarId = getPropertyFromResponse("default_avatar_id", infoText);

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
            var readTask = ReadResponseFromPipe(cancellationToken);

            // open yandex auth page
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "response_type", "code"},
                    { "client_id", _settings.ClientID },
                    { "device_id", _settings.DeviceId },
                    { "device_name", _settings.DeviceName },
                    { "redirect_uri", _settings.ConfirmationURI },
                    {"force_confirm", "yes" }
                }
            );

            UriBuilder uriBuilder = new UriBuilder("https://oauth.yandex.ru/authorize")
            {
                Query = content.ReadAsStringAsync().Result
            };
            
           Process.Start(new ProcessStartInfo(uriBuilder.Uri.ToString()) { UseShellExecute = true });

            // wait for confirmation code
            var redirectedURL = await readTask;

            var confirmationCode = HttpUtility.ParseQueryString(new Uri(redirectedURL).Query)["code"];

            Console.WriteLine($"Confirmation code: {confirmationCode}");

            if (confirmationCode == null)
                throw new Exception("Server did not respond with a confirmation code");

            var authRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth.yandex.ru/token");
            authRequest.Headers.Add("Accept", "application/json");
            authRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"grant_type", "authorization_code" },
                {"code", confirmationCode },
                {"client_id", _settings.ClientID },
                {"client_secret", _settings.ClientSecret },
                {"device_id", _settings.DeviceId },
                {"device_name", _settings.DeviceName },
            });

            var authResponse = await _httpClient.SendAsync(authRequest);

            var status = authResponse.StatusCode;


            var responseText = await authResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Auth response: ${responseText}");

            authResponse.EnsureSuccessStatusCode();

            var accessToken = getAccessTokenFromResponse(responseText);
                        
            _settings.AccessToken = accessToken;
            Console.WriteLine($"Access token: {accessToken}");
        }

        private async Task<string> ReadResponseFromPipe(CancellationToken cancellationToken)
        {
            using var pipeServerStream = new NamedPipeServerStream("NimbusKeeperApp", PipeDirection.In, 1, PipeTransmissionMode.Message);
            
            await pipeServerStream.WaitForConnectionAsync(cancellationToken);

            byte[] buffer = new byte[4096];

            var bytesRead = await pipeServerStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            var result = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

            return result;
        }

        private static string? getAccessTokenFromResponse(string response)
        {
            try
            {
                var document = JsonDocument.Parse(response);
                return document.RootElement.GetProperty("access_token").GetString();
            }
            catch(Exception)
            {
                return null;
            }
        }

        private static string? getPropertyFromResponse(string propertyName, string response)
        {
            try
            {
                var document = JsonDocument.Parse(response);
                return document.RootElement.GetProperty(propertyName).GetString();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

