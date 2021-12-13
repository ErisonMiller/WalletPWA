using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPWA.Shared
{
    public class AssetNow
    {
        public Asset asset {get; set;}
        public float totalValue {get; set;}
        public float actualValue { get; set; }
        public float variation { get; set; }
        public string variation_F { get; set; }
    }
}
