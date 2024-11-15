using System.Net;

namespace FunctionRunner
{
    internal class HttpHandler
    {
        HttpListener _listener;
        int _port;
        public HttpHandler(int port)
        {
            _port = port > 0 ? port : 8080;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_port}/");
        }

        public void Start()
        {
            _listener.Start();
            _listener.BeginGetContext(ListenerCallback, _listener);
            Console.WriteLine($"Listening http://localhost:{_port}/");
        }

        void ListenerCallback(IAsyncResult result)
        {
            if (!_listener.IsListening) { return; }

            var context = _listener.EndGetContext(result);
            var request = context.Request;
            var response = context.Response;

            _listener.BeginGetContext(ListenerCallback, _listener);

            Handler(request, response);
        }

        void Handler(HttpListenerRequest request, HttpListenerResponse response)
        {
            Console.WriteLine($"[HTTP: {request.HttpMethod} {request.Url} at {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}] ");
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        }
    }
}