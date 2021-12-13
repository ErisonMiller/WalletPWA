using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPWA.Shared
{
    public class Asset
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
    }
}
