using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Entities
{
    public class FAmarket
    {
        public string city { get; set; } // used as FA market id
        public FAmarket(string _city)
        {
            city = _city;
        }
        public FAmarket() { }
    }
}
