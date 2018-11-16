using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OptionsExampleApp.Models;
using OptionsExampleApp.Services;


namespace OptionsExampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            BuildConfiguration();
            var serviceFactory = ServiceProviderFactory();
            var presenter = serviceFactory.GetService<IPresenter>();
            Console.WriteLine("Application started");

            Console.WriteLine("Press any key to see #1 example");
            Console.ReadKey();
            Console.WriteLine("Register the Configuration instance which binds against root.");
            var config1 = serviceFactory.GetService<IOptions<RootOptions>>();
            Console.WriteLine(presenter.Serialize(config1.Value));

            Console.WriteLine("Press any key to see #2 example");
            Console.ReadKey();
            var config2 = serviceFactory.GetService<IOptions<MyOptionsWithDelegateConfig>>();
            Console.WriteLine(presenter.Serialize(config2.Value));

            Console.WriteLine("Press any key to see difference between IOptions & IOptionsSnapshot & IOptionsMonitor");
            Console.ReadKey();
            var someService1 = serviceFactory.GetService<ISomeService>();
            Console.WriteLine(someService1.PrintCurrentConfig());
            Console.WriteLine("change config and Press any key");
            Console.ReadKey();
            Console.WriteLine(someService1.PrintCurrentConfig());

            Console.WriteLine("create new instance of IOptions");
            var settings1 = serviceFactory.GetService<IOptions<Settings>>();
            Console.WriteLine(presenter.Serialize(settings1.Value));
            Console.ReadKey();
            Console.WriteLine("create new instance of IOptionsSnapshot");
            var settings2 = serviceFactory.GetService<IOptionsSnapshot<Settings>>();
            Console.WriteLine(presenter.Serialize(settings2.Value));
            Console.WriteLine("change config and Press any key");
            Console.ReadKey();
            Console.WriteLine("create new instance of IOptionsSnapshot");
            var settings3 = serviceFactory.GetService<IOptionsSnapshot<Settings>>();
            Console.WriteLine(presenter.Serialize(settings2.Value));

            Console.WriteLine("Create new instance of service");
            Console.ReadKey();
            var someService2 = serviceFactory.GetService<ISomeService>();
            Console.WriteLine(someService2.PrintCurrentConfig());

            var key = new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);

            while (key.Key != ConsoleKey.Enter)
            {
                Console.WriteLine("change config and Press any key to check or Enter to exit");
                key = Console.ReadKey();
                Console.WriteLine(someService2.PrintCurrentConfig());
            }
        }

        private static IConfiguration Configuration { get; set; }


        private static void BuildConfiguration()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                // Override config by env, using like Logging:Level or Logging__Level
                .AddEnvironmentVariables();

            Configuration = configBuilder.Build();
        }

        private static IServiceProvider ServiceProviderFactory()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            // Example #1: Basic options
            // Register the Configuration instance which binds against root.
            services.Configure<RootOptions>(Configuration);
            // Example #2: Options bound and configured by a delegate
            services.Configure<MyOptionsWithDelegateConfig>(myOptions =>
            {
                myOptions.Id = 500;
            });
            // Example #3: Sub-options
            // Bind options using a sub-section of the appsettings.json file.
            services.Configure<Settings>(Configuration.GetSection("Configuration"));

            services.AddSingleton<IPresenter, Presenter>();
            services.AddTransient<ISomeService, SomeService>();

            return services.BuildServiceProvider();
        }


        private async Task RunHostApp()
        {
            var host = new HostBuilder()
                .UseEnvironment(EnvironmentName.Development)
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        // Override config by env, using like Logging:Level or Logging__Level
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        // Development service configuration
                    }
                    else
                    {
                        // Non-development service configuration
                    }

                    services.AddOptions();
                    // Example #1: Basic options
                    // Register the Configuration instance which binds against root.
                    services.Configure<RootOptions>(Configuration);
                    // Example #2: Options bound and configured by a delegate
                    services.Configure<MyOptionsWithDelegateConfig>(myOptions =>
                    {
                        myOptions.Id = 500;
                    });
                    // Example #3: Sub-options
                    // Bind options using a sub-section of the appsettings.json file.
                    services.Configure<Settings>(Configuration.GetSection("Configuration"));

                    services.AddSingleton<IPresenter, Presenter>();
                    services.AddScoped<ISomeService, SomeService>();

                    services.AddHostedService<TimedHostedService>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                });

            await host.RunConsoleAsync();
        }
    }
}
