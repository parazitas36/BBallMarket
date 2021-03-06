using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Models.PlayersDTO
{
    public class GetPlayerDTO 
    {
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string position { get; set; }
        public float height { get; set; }
        public float weight { get; set; }
        public int age { get; set; }
        public string city { get; set; }
        public string? team { get; set; }
        public string? famarket { get; set; }
    }
}
