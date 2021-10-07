using BBallMarket.Data.Models.PlayersDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Models.TeamDTO
{
    public class UpdateTeamDTO
    {
        public string Owner { get; set; }
        public string TeamName { get; set; }
        public GetPlayerDTO player { get; set; }

    }
}
