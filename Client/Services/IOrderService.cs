using System.Collections.Generic;
using System.Threading.Tasks;
using WalletPWA.Shared;

namespace WalletPWA.Client.Services
{
    public interface IOrderService
    {
        Task<List<Patrimony>> GetPatrimony();
        Task<List<Order>> GetOrders();

        Task<List<Asset>> GetAssets();
        Task<List<AssetNow>> GetAssetsNow();
        Task<List<AssetPrice>> GetResume();

        Task<string> GetPrice(string stock);

        Task<List<Order>> CreateOrder(Order order);

        Task<List<Order>> UpdateOrder(Order order);

        Task<List<Order>> DeleteOrder(Order order);

    }
}
