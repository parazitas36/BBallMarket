using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Models.PlayersDTO
{
    public class UpdatePlayerDTO
    {
        [Required]
        public int? team { get; set; }
    }
}
