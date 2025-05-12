using Microsoft.Extensions.Logging;

namespace FunctionRunner
{
    class LoggerFactoryAdapter(ILoggerFactory factory) : ILoggerProvider
    {
        readonly ILoggerFactory _factory = factory;

        public ILogger CreateLogger(string categoryName)
        {
            return _factory.CreateLogger(categoryName);
        }

        public void Dispose() { }
    }
}