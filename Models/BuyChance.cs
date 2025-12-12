using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models
{
    public class BuyChance
    {
        public int DrawCount { get; set; }

        public int Price { get; set; }

        public string Display => $"{DrawCount}개 - {Price:N0}원";
    }
}
