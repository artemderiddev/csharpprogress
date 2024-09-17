using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsolePlayground
{
    public static class LoggerServiceLocator
    {
        private static readonly ILoggerFactory _factory = LoggerFactory.Create(x => x.AddConsole().SetMinimumLevel(LogLevel.Warning));

        public static ILogger<T> CreateLogger<T>()
        {
            return _factory.CreateLogger<T>();
        }
    }
}
