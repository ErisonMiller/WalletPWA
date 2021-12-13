using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPWA.Shared
{
    public class Order
    {



        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? AssetId { get; set; }
        public Guid? UserId { get; set; }
        public MarketType AssetMarketType { get; set; }
        [Required]
        [MaxLength(8)]
        public string Stock { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }






        public float OperationCost { get; set; } = 0.0f;
        public float OperationProfit { get; set; } = 0.0f;
        public DateTime Date { get; set; }

        public Asset GetAsset()
        {
            return new Asset
            {
                AssetId = this.AssetId,
                UserId = this.UserId,
                AssetMarketType = this.AssetMarketType,
                Stock = this.Stock,
                Price = this.Price,
                Quantity = this.Quantity
            };
        }
    }
}
