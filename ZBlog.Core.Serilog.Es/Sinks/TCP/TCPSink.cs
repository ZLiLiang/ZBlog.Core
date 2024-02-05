using System.Net;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace ZBlog.Core.Serilog.Es.Sinks.TCP
{
    public class TCPSink : ILogEventSink, IDisposable
    {
        private readonly ITextFormatter _formatter;
        private readonly TCPSocketWriter _socketWriter;

        public TCPSink(IPAddress iPAddress, int port, ITextFormatter formatter)
        {
            _socketWriter = new TCPSocketWriter(new Uri($"tcp://{iPAddress}:{port}"));
            _formatter = formatter;
        }

        public TCPSink(Uri uri, ITextFormatter formatter)
        {
            _socketWriter = new TCPSocketWriter(uri);
            _formatter = formatter;
        }

        public void Dispose()
        {
            _socketWriter.Dispose();
        }

        public void Emit(LogEvent logEvent)
        {
            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
                _formatter.Format(logEvent, sw);

            sb.Replace("RenderedMessage", "message");
            _socketWriter.Enqueue(sb.ToString());
        }
    }
}
