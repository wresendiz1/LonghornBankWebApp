using LonghornBankWebApp.DAL;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LonghornBankWebApp.Models
{
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio
    public class DatabaseWarmup : IHostedService
    {
        private readonly ILogger<DatabaseWarmup> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public DatabaseWarmup(ILogger<DatabaseWarmup> logger, IServiceScopeFactory scopeFactory)
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
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

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
            // Ping the website to ensure it is up and avoid cold start
            using var httpClient = new HttpClient();

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("https://longhornbanktrust.azurewebsites.net/Stocks/Details/1");

                //if (response.StatusCode == HttpStatusCode.OK)
                //{

                //    //Console.WriteLine(await response.Content.ReadAsStringAsync());
                //}
                //else
                //{
                //_logger.LogError("Error pinging website");
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
            }

            using var scope = _scopeFactory.CreateScope();

            // Get a reference to the AppDbContext
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // from the stocks model, get the stock prices
            _ = dbContext.Stocks.Include(s => s.StockPrices).AsNoTracking().ToListAsync();
        }

        public void Dispose() => _timer?.Dispose();
    }
}