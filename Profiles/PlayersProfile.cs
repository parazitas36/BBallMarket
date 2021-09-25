using AutoMapper;
using BBallMarket.Data.Models.PlayersDTO;
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
            CreateMap<Player, PostPlayerDTO>();
            CreateMap<Player, UpdatePlayerDTO>();

            CreateMap<GetPlayerDTO, Player>();
            CreateMap<PostPlayerDTO, Player>();
            CreateMap<UpdatePlayerDTO, Player>();
        }
    }
}
