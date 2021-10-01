﻿using AutoMapper;
using BBallMarket.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BBallMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FAmarketController : ControllerBase
    {
        IList<Player> players = new List<Player>();
        Player player = new Player(0, "Adas", "Bananas", "SG", 185.5f, 62.3f, 22, "Kaunas", "BC Nedametimas");
        Player newplayer = new Player(1, "Aldonius", "Onylas", "SF", 199.5f, 62.3f, 22, "Vilnius", null);
        Player newplayer1 = new Player(2, "Edas", "Kiauras", "C", 210.5f, 100.3f, 25, "Vilnius", "Trajakininkai");

        private readonly ILogger<FAmarketController> _logger;
        private readonly IMapper _imapper;

        public FAmarketController(ILogger<FAmarketController> logger, IMapper imapper)
        {
            players.Add(player);
            players.Add(newplayer);
            players.Add(newplayer1);
            _logger = logger;
            _imapper = imapper;
        }

        // GET All available cities: api/famarket
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<string> cities = new List<string>();
            cities.Add("Visi miestai");
            foreach(var p in players)
            {
                if (!cities.Contains(p.city) && p.city != null) { cities.Add(p.city); }
            }
            return Ok(cities);
        }

        /*
            PLAYERS
            GET
            GET(id)
         */

        // GET Players By City: api/famarket/city/players
        [HttpGet("{city}/players")]
        public async Task<IActionResult> GetCityPlayers(string city)
        {
            if (city.ToCharArray().Where(x => !Char.IsLetter(x)).Count() > 0){ return BadRequest(); }

            IList<Player> list;
            if(city == "all")
            {
                list = players;
            }
            else
            {
                list = players.Where<Player>(x => x.city.ToLower() == city.ToLower()).ToList();
            }

            if(list.Count == 0)
            {
                return NotFound();
            }

            return Ok(list);
        }

        // GET Player by ID: api/famarket/city/players/playerid
        [HttpGet("{city}/players/{playerid}")]
        public async Task<IActionResult> GetPlayerByID(string city, string playerid)
        {
            if (city.ToCharArray().Where(x => !Char.IsLetter(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }

            Player p = players.FirstOrDefault<Player>(x => x.id == Convert.ToInt32(playerid) && x.city.ToLower() == city.ToLower());
            if(p == null) { return NotFound(); }
            return Ok(p);
        }

        /*
            INVITES:
            Post
            Put
            Delete
        */

        // POST Send invite to the player: api/famarket/city/players/playerid/invites/inviteid
        [HttpPost("{city}/players/{playerid}/invites/{inviteid}")]
        public async Task<IActionResult> Post(string city, string playerid, string inviteid, [FromBody] string team)
        {
            if(city.ToCharArray().Where(x=> !Char.IsLetter(x)).Count() > 0 || 
                playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
                inviteid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }

            Player p = players.FirstOrDefault<Player>(x => x.id == Convert.ToInt32(playerid) && x.city.ToLower() == city.ToLower());
            if (p == null) {  return NotFound(); }

            Invite invitation = new Invite(Convert.ToInt32(inviteid), team, p, "Pending");
            return Ok(invitation);
        }

        // PUT Update invite status: api/famarket/city/players/playerid/invites/inviteid
        [HttpPut("{city}/players/{playerid}/invites/{inviteid}")]
        public async Task<IActionResult> Put(string city, string playerid, string inviteid, [FromBody] string inviteStatus)
        {
            if (city.ToCharArray().Where(x => !Char.IsLetter(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               inviteid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               inviteStatus.ToCharArray().Where(x => !Char.IsLetter(x)).Count() > 0)
            {
                return BadRequest();
            }

            Player p = players.FirstOrDefault<Player>(x => x.id == Convert.ToInt32(playerid) && x.city.ToLower() == city.ToLower());
            if (p == null) { return NotFound(); }
            
            Invite invitation = new Invite(Convert.ToInt32(inviteid), p.team, p, inviteStatus);
            return Ok(invitation);
        }

        // DELETE Invite api/famarket/city/players/playerid/invites/inviteid
        [HttpDelete("{city}/players/{playerid}/invites/{inviteid}")]
        public async Task<IActionResult> Delete(string city, string playerid, string inviteid, string inviteStatus)
        {
            if (city.ToCharArray().Where(x => !Char.IsLetter(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               inviteid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               inviteStatus.ToCharArray().Where(x => !Char.IsLetter(x)).Count() > 0)
            {
                return BadRequest();
            }
            if(Convert.ToInt32(inviteid) < 5)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}