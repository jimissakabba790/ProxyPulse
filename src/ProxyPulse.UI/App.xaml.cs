using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyPulse.Common.Configuration;
using ProxyPulse.Common.Logging;
using System;
using System.Windows;

namespace ProxyPulse.UI
{
    /// <summary>
    /// Entry point for the ProxyPulse application.
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Configure options
                    services.Configure<AppSettings>(
                        context.Configuration.GetSection(nameof(AppSettings)));
                    services.Configure<LoggingConfig>(
                        context.Configuration.GetSection("Logging"));

                    // Register your services here
                    // services.AddSingleton<MainWindow>();
                    // services.AddSingleton<MainViewModel>();
                })
                .UseProxyPulseLogging()
                .Build();
        }

        /// <summary>
        /// Application startup handler.
        /// </summary>
        /// <param name="e">Startup event args.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Get the main window from DI
            // var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            // mainWindow.Show();

            base.OnStartup(e);
        }

        /// <summary>
        /// Application exit handler.
        /// </summary>
        /// <param name="e">Exit event args.</param>
        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }
    }
}
