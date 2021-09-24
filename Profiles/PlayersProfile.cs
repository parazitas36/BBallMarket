using AutoMapper;
using BBallMarket.Data.Model.PlayersDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Profiles
{
    public class PlayersProfile : Profile
    {
        public PlayersProfile()
        {
            CreateMap<Player, GetPlayerDTO>();
        }
    }
}
