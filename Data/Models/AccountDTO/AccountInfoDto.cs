using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Models.AccountDTO
{
    public class AccountInfoDto
    {
        public string teamName { get; set; }
        public int? teamId { get; set; }
        public string famarket { get; set; }
        public int? faId { get; set; }
        public int? playerId { get; set; }
        public bool hasTeam { get; set; }
    }
}
