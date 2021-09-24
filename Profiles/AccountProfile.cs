using AutoMapper;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Model.AccountDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Profiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, CreateAccountDTO>();
        }
    }
}
