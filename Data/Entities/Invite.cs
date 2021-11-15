using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Entities
{
    public class Invite
    {
        public int inviteID { get; set; }
        public string team { get; set; }
        public Player player { get; set; }
        public string inviteStatus { get; set; } // Pending, Accepted, Declined
        public Invite(int id, string _team, Player _receiver, string status)
        {
            inviteID = id;
            team = _team;
            player = _receiver;
            inviteStatus = status;
        }
        public Invite() { }
    }
}
