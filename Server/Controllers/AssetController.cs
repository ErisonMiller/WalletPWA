using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WalletPWA.Server.Data;
using WalletPWA.Shared;

using static WalletPWA.Server.Services.PriceService;

namespace WalletPWA.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetController : ControllerBase
    {
        private readonly DataContext _context;

        public AssetController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null) return NotFound();
            Guid UserId = Guid.Parse(user);
            return Ok(await _context.assets.Where(o => o.UserId == UserId && o.Quantity != 0).ToListAsync());
            //return Ok(await _context.assets.ToListAsync());
        }


        [HttpGet("AssetsNow")]
        public async Task<IActionResult> GetAssetNow()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null) return NotFound();
            Guid UserId = Guid.Parse(user);

            var assets = await _context.assets.Where(o => o.UserId == UserId && o.Quantity != 0).ToListAsync();
            //return Ok(await _context.assets.Where(o => o.UserId == UserId && o.Quantity != 0).OrderByDescending(asset => asset.Stock).ToListAsync());

            var data = "list%5B0%5D%5Bcategory%5D=1";

            foreach (var asset in assets)
            {
                data += "&list%5B0%5D%5Bcodes%5D%5B%5D=" + asset.Stock;
            }

            List<AssetNow> assestsNow = getAssetStatus(data);

            for (int i=0; i < assestsNow.Count(); i++)
            {
                assestsNow[i].asset = assets[i];
                assestsNow[i].totalValue = assets[i].Quantity * assestsNow[i].actualValue;
            }

            return Ok(assestsNow);
        }


        [HttpGet("AssetsResume")]
        public async Task<IActionResult> GetAssetResume()
        {
            try
            {

                List<AssetNow> assestsNow = getAssetStatus("list%5B0%5D%5Bcategory%5D=5&list%5B0%5D%5Bcodes%5D%5B%5D=ibovespa&list%5B1%5D%5Bcategory%5D=902&list%5B1%5D%5Bcodes%5D%5B%5D=sp-500");


                var httpRequest = (HttpWebRequest)WebRequest.Create("https://economia.awesomeapi.com.br/last/USD-BRL");

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                var streamReader = new StreamReader(httpResponse.GetResponseStream());
                var doc = JsonDocument.Parse(streamReader.ReadToEnd());


                List<AssetPrice> assestsPrice = new List<AssetPrice>();

                assestsPrice.Add(
                    new AssetPrice
                    {
                        ticker = "Dollar",
                        actualValue = double.Parse(doc.RootElement.GetProperty("USDBRL").GetProperty("bid").GetString().Replace('.',',')),
                        variation = double.Parse(doc.RootElement.GetProperty("USDBRL").GetProperty("pctChange").GetString().Replace('.', ','))
                    });

                assestsPrice.Add(
                    new AssetPrice
                    {
                        ticker = "Ibovespa",
                        actualValue = assestsNow[0].actualValue,
                        variation = assestsNow[0].variation
                    });
                assestsPrice.Add(
                    new AssetPrice
                    {
                        ticker = "S&P 500",
                        actualValue = assestsNow[1].actualValue,
                        variation = assestsNow[1].variation
                    });

                return Ok(assestsPrice);
            }catch (Exception ex)
            {
                List<AssetPrice> assestsPrice = new List<AssetPrice>();

                Console.WriteLine(ex);
                return Ok(assestsPrice);
            }
        }


        [HttpGet("Price/{stock}")]
        public async Task<string> GetPrice(string stock)
        {
            var url = "https://statusinvest.com.br/home/mainsearchquery?q=" + stock;
            
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            var streamReader = new StreamReader(httpResponse.GetResponseStream());
            
            var result = streamReader.ReadToEnd();
            
            
            return result;    
        }


        [HttpGet("Patrimony")]
        public async Task<IActionResult> GetPatrimony()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null) return NotFound();
            Guid UserId = Guid.Parse(user);

            return Ok(await _context.patrimonies.Where(o => o.UserId == UserId).OrderBy(o => o.Date).Take(30).ToListAsync());

        }

        [HttpGet("PriceList")]
        public async Task<string> GetPriceList(List<string> stocks)
        {
            var url = "https://statusinvest.com.br/category/treemapresult";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.ContentType = "application/x-www-form-urlencoded";

            var data = "list%5B0%5D%5Bcategory%5D=1";

            foreach(var stock in stocks)
            {
                data += "&list%5B0%5D%5Bcodes%5D%5B%5D=" + stock;
            }

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
            return "0";
        }

    }
}
