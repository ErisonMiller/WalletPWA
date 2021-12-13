using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPWA.Shared
{
    public class AssetPrice
    {
        public string ticker { get; set; }
        public double actualValue { get; set; }
        public double variation { get; set; }
    }
}
