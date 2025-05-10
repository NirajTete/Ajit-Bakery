using Ajit_Bakery.Data;
using Microsoft.EntityFrameworkCore;

namespace Ajit_Bakery.Services
{ 

    public class BoxMasterResetService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BoxMasterResetService> _logger;
        //private readonly TimeSpan _interval = TimeSpan.FromHours(2); // 2-hour interval
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(10); // 2-hour interval

        public BoxMasterResetService(IServiceScopeFactory scopeFactory, ILogger<BoxMasterResetService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BoxMasterResetService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<DataDBContext>();

                        _logger.LogInformation("Resetting BoxMaster Use_flag...");

                        await _context.BoxMaster
                            .Where(b => b.Use_Flag != 0) // Reset only if needed
                            .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Use_Flag, 0), stoppingToken);

                        _logger.LogInformation("BoxMaster Use_flag reset successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while resetting BoxMaster Use_flag.");
                }

                await Task.Delay(_interval, stoppingToken); // Wait for 2 hours before next execution
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BoxMasterResetService is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }

}
