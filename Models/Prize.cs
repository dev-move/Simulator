using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models
{
    public class Prize
    {
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public double Probability { get; set; } = 1.0;
        public int TiketValue { get; set; }

        public Rarity Rarity =>
            TiketValue switch
            {
                30 => Rarity.Legendary,
                5 => Rarity.Rare,
                2 => Rarity.High,
                1 => Rarity.Common,
                _ => Rarity.Common
            };

    }
}
