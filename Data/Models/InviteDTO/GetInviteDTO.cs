using BBallMarket.Data.Models.PlayersDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Models.InviteDTO
{
    public class GetInviteDTO
    {
        public int inviteID { get; set; }
        public string team { get; set; }
        public GetPlayerDTO receiver { get; set; }
    }
}
