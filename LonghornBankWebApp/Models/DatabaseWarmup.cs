using LonghornBankWebApp.DAL;
using Microsoft.EntityFrameworkCore;

namespace LonghornBankWebApp.Models
{
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio
    public class MyBackgroundService : IHostedService, IDisposable
    {
        private readonly ILogger<MyBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public MyBackgroundService(ILogger<MyBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        private static readonly Action<ILogger, string, Exception> _myBackgroundServiceStarting =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "MyBackgroundServiceStarting"), "MyBackgroundService is starting. {Message}");

        private static readonly Action<ILogger, string, Exception> _myBackgroundServiceStopping =
    LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, "MyBackgroundServiceStopping"), "MyBackgroundService is stopping. {Message}");

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _myBackgroundServiceStarting(_logger, "", null);

            // Schedule the task to run every minute
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _myBackgroundServiceStopping(_logger, "", null);

            // Stop the timer
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using var scope = _scopeFactory.CreateScope();
            // Get a reference to the AppDbContext
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // from the stocks model, get the stock prices
            var stockPrices = await dbContext.Stocks.Include(s => s.StockPrices).ToListAsync();

            // TODO: Add your background task logic here

            await dbContext.SaveChangesAsync();
        }

        public void Dispose() => _timer?.Dispose();
    }
}