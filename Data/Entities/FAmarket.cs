using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Entities
{
    public class FAmarket
    {
        public string? city { get; set; } // used as FA market id
        public IList<Player> freeAgents { get; set; }
        public FAmarket(string _city, IList<Player> _freeAgents)
        {
            city = _city;
            freeAgents = _freeAgents;
        }
        public FAmarket(IList<Player> _freeAgents)
        {
            freeAgents = _freeAgents;
        }
        public FAmarket() { }
    }
}
