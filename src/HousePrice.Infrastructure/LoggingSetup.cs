using System;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace HousePrice.Infrastructure
{
    public static class LoggingSetupExtensions
    {
        public static IWebHostBuilder UseAppLogging(
            this IWebHostBuilder builder
        )
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console());
            return builder;
        }

    }
}