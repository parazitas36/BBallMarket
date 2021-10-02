using AutoMapper;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.InviteDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Profiles
{
    public class InviteProfile : Profile
    {
        public InviteProfile()
        {
            CreateMap<Invite, GetInviteDTO>();
            CreateMap<GetInviteDTO, Invite>();
        }
    }
}
