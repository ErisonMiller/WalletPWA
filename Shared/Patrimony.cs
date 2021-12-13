using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPWA.Shared
{
    public class Patrimony
    {
        [Key, Column(Order = 0)]
        public Guid? UserId { get; set; }
        [Key, Column(Order = 1)]
        public DateTime Date { get; set; }

        public float priceToday { get; set; }
        public float priceOriginal { get; set; }
    }
}
