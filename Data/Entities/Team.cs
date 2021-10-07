using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Entities
{
    public class Team
    {
        public int TeamID { get; set; }
        public string Owner { get; set; }
        public string TeamName { get; set; }
        public IList<Player> Players { get; set; }
        public Team() { }
        public Team(string owner, string teamName, IList<Player> players)
        {
            TeamID = 1;
            Owner = owner;
            TeamName = teamName;
            Players = players;
        }

        public void updateTeam(string? owner, string? teamName, Player player)
        {
            Owner = owner == null ? Owner : owner;
            TeamName = teamName == null ? TeamName : teamName;
            if(player != null) { Players.Add(player); }
        }
    }
}
