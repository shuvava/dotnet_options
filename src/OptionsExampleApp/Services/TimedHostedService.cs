using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace OptionsExampleApp.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ISomeService _service;
        private Timer _timer;
        private long iteration;


        public TimedHostedService(
            ISomeService service,
            ILogger<TimedHostedService> logger)
        {
            _service = service;
            _logger = logger;
        }


        public void Dispose()
        {
            _timer?.Dispose();
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }


        private void DoWork(object state)
        {
            _logger.LogInformation($"Iteration # {iteration}: {_service.PrintCurrentConfig()}");
            iteration += 1;
        }
    }
}
