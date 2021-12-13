using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WalletPWA.Server.Data;
using WalletPWA.Shared;
using static WalletPWA.Server.Services.PriceService;

namespace WalletPWA.Server.Services
{
    //Everyday this schedule updates patrimony of every user using the actual value of the assets in the user wallet
    public class UpdatePatrimony : IHostedService, IDisposable
    {

        private int executionCount = 0;
        private readonly ILogger<UpdatePatrimony> _logger;
        private Timer _timer = null!;
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdatePatrimony(ILogger<UpdatePatrimony> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            DateTime now = DateTime.Now;
            DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, 22, 0, 0, 0);
            if (now > firstRun)
            {
                firstRun = firstRun.AddDays(1);
            }

            TimeSpan timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }


            _timer = new Timer(DoWork, null, timeToGo, TimeSpan.FromDays(1));

            //_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));


            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<DataContext>();
            //get asset list
            List<string> assets = await _context.assets.Where(x => x.Quantity > 0).Select(o => o.Stock).Distinct().ToListAsync();
            var usersAsync = _context.assets.GroupBy(x => x.UserId).
                            Select(
                                x => new
                                {
                                    UserId = x.Key,
                                    Date = DateTime.Now,
                                    assetList = x.Select(o => new { o.Stock, o.Quantity }).ToList(),
                                    priceOriginal = x.Sum(o => o.Quantity * o.Price)
                                }
                            ).ToListAsync();


            var prices = GetAssetListPrice(assets);            

            var users = await usersAsync;

            List< Patrimony > patrimonies = new List< Patrimony >();    
            
            foreach(var user in users)
            {
                float sum = 0;
                foreach(var value in user.assetList)
                {
                    sum += prices[value.Stock] * value.Quantity;
                }

                patrimonies.Add(new Patrimony { UserId = user.UserId, Date =user.Date,  priceOriginal=user.priceOriginal, priceToday = sum});
            }

            _context.AddRange(patrimonies);
            await _context.SaveChangesAsync();

            Console.WriteLine(JsonSerializer.Serialize(patrimonies));
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
    
}
