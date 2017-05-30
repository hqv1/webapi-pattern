using System.Web.Http.ExceptionHandling;
using Microsoft.Owin.Logging;

namespace WebApiPattern.Asp.Framework
{
    public class NLogExceptionLogger : ExceptionLogger
    {
        private readonly ILogger _logger;

        public NLogExceptionLogger(ILogger logger)
        {
            _logger = logger;
        }
        public override void Log(ExceptionLoggerContext context)
        {
            _logger.WriteError("Unhandled exception", context.Exception);
        }
        
    }
}