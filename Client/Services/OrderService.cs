using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WalletPWA.Shared;

namespace WalletPWA.Client.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        
        public OrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }



        public async Task<List<Order>> GetOrders()
        {
            return await _httpClient.GetFromJsonAsync<List<Order>>("api/order");
        }

        public async Task<List<Asset>> GetAssets()
        {
            return await _httpClient.GetFromJsonAsync<List<Asset>>("api/asset");
        }


        public async Task<List<AssetNow>> GetAssetsNow()
        {
            return await _httpClient.GetFromJsonAsync<List<AssetNow>>("api/asset/AssetsNow");
        }


        public async Task<List<AssetPrice>> GetResume()
        {
            return await _httpClient.GetFromJsonAsync<List<AssetPrice>>("api/asset/AssetsResume");
        }

        public async Task<List<Order>> CreateOrder(Order order)
        {
            var result = await _httpClient.PostAsJsonAsync($"api/order", order);
            var orders = await result.Content.ReadFromJsonAsync<List<Order>>();
            return orders;
        }

        public async Task<List<Order>> UpdateOrder(Order order)
        {

            var result = await _httpClient.PutAsJsonAsync($"api/order", order);
            var orders = await result.Content.ReadFromJsonAsync<List<Order>>();
            return orders;
        }

        public async Task<List<Order>> DeleteOrder(Order order)
        {

            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = JsonContent.Create(order),
                Method = HttpMethod.Delete,
                RequestUri = new System.Uri("api/order", System.UriKind.Relative)
            };
            var result = await _httpClient.SendAsync(request);

            //var result = await _httpClient.DeleteAsync($"api/order", order);
            var orders = await result.Content.ReadFromJsonAsync<List<Order>>();
            return orders;
        }

        public async Task<string> GetPrice(string stock)
        {

           return await _httpClient.GetStringAsync("api/asset/Price/"+stock );
        }

        public async Task<List<Patrimony>> GetPatrimony()
        {
            return await _httpClient.GetFromJsonAsync<List<Patrimony>>("api/asset/Patrimony");
        }
    }
}
