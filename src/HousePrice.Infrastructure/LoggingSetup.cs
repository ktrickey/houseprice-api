using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.AspNetCore;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace HousePrice.Infrastructure
{
    public static class LoggingSetupExtensions
    {
        /// <summary>Sets Serilog as the logging provider.</summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="logger">The Serilog logger; if not supplied, the static <see cref="T:Serilog.Log" /> will be used.</param>
        /// <param name="dispose">When true, dispose <paramref name="logger" /> when the framework disposes the provider. If the
        /// logger is not specified but <paramref name="dispose" /> is true, the <see cref="M:Serilog.Log.CloseAndFlush" /> method will be
        /// called on the static <see cref="T:Serilog.Log" /> class instead.</param>
        /// <returns>The web host builder.</returns>
//        public static IWebHostBuilder UseLogging(
//            this IWebHostBuilder builder,
//            ILogger logger = null,
//            bool dispose = false)
//        {
//            if (builder == null)
//                throw new ArgumentNullException(nameof(builder));
//            builder.ConfigureServices((Action<IServiceCollection>) (collection =>
//                collection.AddSingleton<ILoggerFactory>((Func<IServiceProvider, ILoggerFactory>) (services =>
//                    (ILoggerFactory) new SerilogLoggerFactory(logger, dispose)))));
//            return builder;
//        }
    }
}