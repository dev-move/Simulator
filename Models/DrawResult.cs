using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models
{
    public class DrawResult
    {
        public int Index { get; set; }

        public Prize? Prize { get; set; }

        public DateTime DrawTime { get; set; }

        public bool TiketExchanged { get; set; }
    }
}
