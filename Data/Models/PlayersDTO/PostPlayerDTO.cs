using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Models.PlayersDTO
{
    public class PostPlayerDTO
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string surname { get; set; }
        [Required]
        public string position { get; set; }
        [Required]
        public float height { get; set; }
        [Required]
        public float weight { get; set; }
        [Required]
        public int age { get; set; }
        [Required]
        public string city { get; set; }
        public string? team { get; set; }
        public string? famarket { get; set; }
    }
}
