using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.FADTO;

namespace BBallMarket.Profiles
{
    public class FAProfile : Profile
    {
        public FAProfile()
        {
            CreateMap<FAmarket, GetFADTO>();
            CreateMap<FAmarket, PostFADTO>();
            CreateMap<GetFADTO, FAmarket>();
            CreateMap<PostFADTO, FAmarket>();
        }
    }
}
