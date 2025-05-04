﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using Avalonia;
using System;
using System.IO;
using EHRp.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EHRp
{
    /// <summary>
    /// Main program class for the application.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Gets the service provider for the application.
        /// </summary>
        public static IServiceProvider? ServiceProvider { get; private set; }
        
        /// <summary>
        /// Gets the configuration for the application.
        /// </summary>
        public static IConfiguration? Configuration { get; private set; }
        
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            // Set up configuration
            var basePath = Directory.GetCurrentDirectory();
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: false, reloadOnChange: true)
                .AddJsonFile(Path.Combine(basePath, $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json"), optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            // Set up dependency injection
            var services = new ServiceCollection();
            services.AddApplicationServices(Configuration);
            ServiceProvider = services.BuildServiceProvider();
            
            // Start the application
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        
        /// <summary>
        /// Builds the Avalonia application.
        /// </summary>
        /// <returns>The application builder.</returns>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
