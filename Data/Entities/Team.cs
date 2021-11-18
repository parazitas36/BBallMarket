using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Entities
{
    public class Team
    {
        public int TeamID { get; set; }
        public int OwnerID { get; set; }
        public string TeamName { get; set; }
        public IList<Player>? Players { get; set; }
        public Team() { }
        public Team(int id, int owner, string teamName, IList<Player>? players)
        {
            TeamID = id;
            OwnerID = owner;
            TeamName = teamName;
            Players = players;
        }
    }
}
