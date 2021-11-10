using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace BBallMarket.Data.Models.InviteDTO
{
    public class PostInviteDTO : Profile
    {
        [Required]
        public string teamName { get; set; }
    }
}
