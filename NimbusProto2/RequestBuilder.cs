
namespace NimbusProto2
{
    public class RequestBuilder
    {
        private HttpMethod _method;
        private readonly UriBuilder _uriBuilder;

        private List<(string, string)>? _headers;
        private List<KeyValuePair<string, string>>? _query;
        private List<KeyValuePair<string, string>>? _formContent;
        public RequestBuilder(HttpMethod method, string baseUri) 
        {
            _method = method;
            _uriBuilder = new(baseUri);
        }
        public RequestBuilder WithQuery(params (string, string)[] args)
        {
            _query ??= [];
            _query.AddRange(MakeKVPairs(args));
            return this;
        }

        public RequestBuilder WithFormContent(params (string, string)[] args)
        {
            _formContent ??= [];
            _formContent.AddRange(MakeKVPairs(args));
            return this;
        }

        public RequestBuilder WithHeaders(params (string, string)[] args)
        {
            _headers ??= [];
            _headers.AddRange(args);
            return this;
        }

        public RequestBuilder WithAuthorization(string token)
        {
            return WithHeaders(("Authorization", $"OAuth {token}"));
        }

        public RequestBuilder WithAccept(params string[] acceptedTypes)
        {
            return WithHeaders((from type in acceptedTypes select ("Accept", type)).ToArray());
        }

        private static IEnumerable<KeyValuePair<string, string>> MakeKVPairs(params (string, string)[] args)
        {
            foreach(var (k, v) in args)
            {
                yield return KeyValuePair.Create(k, v);
            }
        }

        public HttpRequestMessage Build()
        {
            if(_query != null)
                _uriBuilder.Query = new FormUrlEncodedContent(_query).ReadAsStringAsync().Result;

            HttpRequestMessage request = new(_method, _uriBuilder.Uri);

            if(_headers != null)
                foreach(var (k, v) in _headers)
                    request.Headers.Add(k, v);

            if (_formContent != null)
                request.Content = new FormUrlEncodedContent(_formContent);

            return request;
        }
    }
}
