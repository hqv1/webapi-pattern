using System;
using Microsoft.Extensions.Logging;
using ILogger = Hqv.CSharp.Common.Interfaces.ILogger;

namespace WebApiPattern.Asp.Shared
{
    public class Logger : ILogger
    {
        private readonly ILogger<Logger> _logger;
        private readonly EventId _eventId = new EventId(1);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Microsoft logger</param>
        public Logger(ILogger<Logger> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="correlationId"></param>
        public void LogWarning(string message, Exception exception, string correlationId)
        {
            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(_eventId, exception, message);
            }
            else
            {
                _logger.LogWarning(_eventId, exception, message + $" CorrelationId is {correlationId}");
            }
        }

        public void LogInfo(string message, object detail, string correlationId = null)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string message, object detail, string correlationId = null)
        {
            throw new NotImplementedException();
        }

        public void LogException(string message, Exception exception, string correlationId = null)
        {
            throw new NotImplementedException();
        }

        public void LogException(string message, object detail, string correlationId = null)
        {
            throw new NotImplementedException();
        }
    }
}