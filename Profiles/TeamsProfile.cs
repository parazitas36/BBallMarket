using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.TeamDTO;

namespace BBallMarket.Profiles
{
    public class TeamsProfile : Profile
    {
        public TeamsProfile()
        {
            CreateMap<Team, GetTeamDTO>();
            CreateMap<GetTeamDTO, Team>();
            CreateMap<Team, PostTeamDTO>();
            CreateMap<PostTeamDTO, Team>();
            CreateMap<Team, UpdateTeamDTO>();
            CreateMap<UpdateTeamDTO, Team>();
        }
    }
}
