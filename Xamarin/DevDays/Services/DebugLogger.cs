using System.Diagnostics;
using Splat;

namespace DevDays.Services
{
    public class DebugLogger : ILogger
    {
        public void Write(string message, LogLevel logLevel)
        {
            Debug.WriteLine($"{logLevel}: {message}");
        }

        public LogLevel Level { get; set; }
    }
}