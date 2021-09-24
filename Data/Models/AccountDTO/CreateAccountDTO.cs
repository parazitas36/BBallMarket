using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Model.AccountDTO
{
    public class CreateAccountDTO
    {
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        public string email { get; set; }
    }
}
