using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using WalletPWA.Shared;

namespace WalletPWA.Server.Services
{
    public static class PriceService
    {

        public static List<AssetNow> getAssetStatus(string data)
        {
            var url = "https://statusinvest.com.br/category/treemapresult";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            var streamReader = new StreamReader(httpResponse.GetResponseStream());
            var doc = JsonDocument.Parse(streamReader.ReadToEnd());
            var assetPriceJson = doc.RootElement.GetProperty("data");

            

            return JsonSerializer.Deserialize<List<AssetNow>>(assetPriceJson);
        }

        public static Dictionary<string, float> GetAssetListPrice(List<string> stocks)
        {
            var data = "list%5B0%5D%5Bcategory%5D=1";

            foreach (var stock in stocks)
            {
                data += "&list%5B0%5D%5Bcodes%5D%5B%5D=" + stock;
            }

            List<AssetNow> assetsNow = getAssetStatus(data);



            Dictionary<string, float> prices = new Dictionary<string, float>();

            for(int i = 0; i < assetsNow.Count; i++)
            {
                prices.Add(stocks[i], assetsNow[i].actualValue);
            }

            return prices;

        }

    }
}
