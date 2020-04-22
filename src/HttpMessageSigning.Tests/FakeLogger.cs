using System;
using System.Collections.Generic;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning {
    public class FakeLogger<T> : ILogger<T> {
        public List<LogEntry> LoggedEntries { get; } = new List<LogEntry>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            var logEntry = new LogEntry {
                Level = logLevel,
                Exception = exception,
                Message = formatter(state, exception)
            };
            LoggedEntries.Add(logEntry);
        }

        public bool IsEnabled(LogLevel logLevel) {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) {
            return A.Fake<IDisposable>();
        }

        public class LogEntry {
            public LogLevel? Level { get; set; }
            public Exception Exception { get; set; }
            public string Message { get; set; }
        }
    }
}