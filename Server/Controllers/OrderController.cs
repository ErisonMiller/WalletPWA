using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WalletPWA.Server.Data;
using WalletPWA.Shared;

namespace WalletPWA.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly DataContext _context;

        public OrderController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(await _context.orders.Where(o => o.UserId == UserId).OrderByDescending(order => order.Date).ThenBy(x => x.AssetId).Take(100).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            order.Stock.ToUpper();
            if (User.Identity.IsAuthenticated)
            {
                //find current user Id
                order.UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                //DayTrade does not affect the assets
                if (order.AssetMarketType == MarketType.DayTrade)
                {
                    //get the last daytrade order
                    var lastOrder = await _context.orders.Where(o => o.UserId == order.UserId && o.Stock == order.Stock &&
                        o.AssetMarketType == order.AssetMarketType && o.Date == order.Date).GroupBy(x => true).
                        Select(
                            x => new
                            {
                                Quantity = x.Sum(o => o.Quantity),
                                Price = x.Sum(o => o.Quantity * o.Price - o.OperationProfit)
                            }
                        ).FirstAsync();

                    //if the sing is different means that a lost or a gain occurs
                    if (lastOrder != null && Math.Sign(lastOrder.Quantity) != Math.Sign(order.Quantity))
                    {
                        float qnt = lastOrder.Quantity;
                        if (Math.Abs(lastOrder.Quantity) > Math.Abs(order.Quantity)) qnt = order.Quantity;
                        order.OperationProfit = qnt * (lastOrder.Price - order.Price);
                    }

                }
                else //creates or update a asset
                {
                    Asset asset = await _context.assets.Where(o => o.UserId == order.UserId && o.Stock == order.Stock
                    && o.AssetMarketType == order.AssetMarketType).FirstOrDefaultAsync();

                    if (asset == null)
                    {
                        _context.assets.Add(order.GetAsset());
                    }
                    else
                    {
                        if (Math.Sign(asset.Quantity) != Math.Sign(order.Quantity))
                        {
                            float qnt = asset.Quantity;
                            if (Math.Abs(asset.Quantity) > Math.Abs(order.Quantity)) qnt = order.Quantity;
                            order.OperationProfit = qnt * (asset.Price - order.Price);
                        }
                        int quantity = asset.Quantity + order.Quantity;
                        float avgPrice = (asset.Quantity * asset.Price + order.Quantity * order.Price + order.OperationProfit) / quantity;
                        asset.Quantity = quantity;
                        asset.Price = avgPrice;

                        _context.assets.Update(asset);
                    }
                }

                _context.orders.Add(order);
                await _context.SaveChangesAsync();
            }
            return await GetOrders();
        }


        [HttpPut]
        public async Task<IActionResult> UpdateOrder(Order order)
        {
            order.Stock.ToUpper();
            _context.orders.Update(order);
            await _context.SaveChangesAsync();
            return await GetOrders();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOrder(Order order)
        {
            _context.orders.Remove(order);
            await _context.SaveChangesAsync();
            return await GetOrders();
        }
    }
}

