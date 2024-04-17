using System.Collections.Specialized;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var root = Environment.GetEnvironmentVariable("ROOT_URL") ?? "localhost:8082";

var handlers = new Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>>()
{
    { "/icm", IcmHander }
};

Console.WriteLine($"Starting HTTP service on {root}...");

var listener = new HttpListener();

listener.Prefixes.Add($"http://{root}/");

listener.Start();

listener.BeginGetContext(ListenerCallback, listener);

Thread.Sleep(int.MaxValue);

void ListenerCallback(IAsyncResult result)
{
    if (!listener.IsListening) { return; }

    var context = listener.EndGetContext(result);
    var request = context.Request;
    var response = context.Response;

    listener.BeginGetContext(ListenerCallback, listener);

    Handlers(request, response);
}

void Handlers(HttpListenerRequest request, HttpListenerResponse response)
{
    Console.WriteLine($"[{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}] {request.HttpMethod} {request.Url}");

    var path = request.Url?.AbsolutePath ?? string.Empty;
    var handler = handlers.GetValueOrDefault(path, DefaultHandler);
    try
    {
        handler(request, response);
    }
    catch (Exception e)
    {
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        response.Close();
        Console.WriteLine(e.ToString());
    }
}

void DefaultHandler(HttpListenerRequest request, HttpListenerResponse response)
{
    response.StatusCode = response.StatusCode == (int)HttpStatusCode.InternalServerError ? response.StatusCode : (int)HttpStatusCode.NotFound;
    response.ContentType = "application/json";

    dynamic payload = new JObject();
    payload.method = request.HttpMethod;
    payload.path = request.Url?.AbsolutePath;
    payload.queries = JObject.FromObject(NameValueCollectionToDictionary(request.QueryString));
    payload.headers = JObject.FromObject(NameValueCollectionToDictionary(request.Headers));
    if (request.HasEntityBody)
    {
        var encoding = request.ContentEncoding;
        using var body = request.InputStream;
        using var reader = new StreamReader(body, encoding);
        using var jsonReader = new JsonTextReader(reader);
        payload.body = JObject.ReadFrom(jsonReader);
    }

    var json = payload.ToString(Formatting.None);
    var bytes = Encoding.UTF8.GetBytes(json);

    using var output = response.OutputStream;
    output.Write(bytes, 0, bytes.Length);
}

void IcmHander(HttpListenerRequest request, HttpListenerResponse response)
{
    throw new NotImplementedException();
}

Dictionary<string, string> NameValueCollectionToDictionary(NameValueCollection collection)
{
    return collection?.AllKeys
        .Select<string?, string>(k => k?.ToString() ?? string.Empty)
        .ToDictionary(k => k, k => string.Join("&", collection?.GetValues(k) ?? Array.Empty<string>()))
        ?? new Dictionary<string, string>();
}
