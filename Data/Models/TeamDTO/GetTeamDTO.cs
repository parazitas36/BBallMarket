using BBallMarket.Data.Models.PlayersDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Models.TeamDTO
{
    public class GetTeamDTO
    {
        public string Owner { get; set; }
        public string TeamName { get; set; }
        public IList<GetPlayerDTO> Players { get; set; }
    }
}
