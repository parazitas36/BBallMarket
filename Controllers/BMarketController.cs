using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BMarketController : ControllerBase
    {

        Player[] players = new Player[2];
        Player player = new Player("Vardas", "Pavarde", "Atakuojantis gynejas", 165.5f, 62.3f, 22, "Kaunas", "Komandos pavadinimas");
 
        Player newplayer = new Player("Vardas2", "Pavarde2", "Atakuojantis gynejas2", 165.5f, 62.3f, 22, "Kaunas", "Komandos pavadinimas");
        private readonly ILogger<BMarketController> _logger;

        public BMarketController(ILogger<BMarketController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Index.html");
        }
        [HttpGet("players")]
        public IActionResult GetList()
        {
            players[0] = player;
            players[1] = newplayer;
            return Ok(players);
        }
        [HttpGet("player/{id:int}")]
        public IActionResult GetById(int id)
        {
            try
            {
                players[0] = player;
                players[1] = newplayer;
                if (Math.Abs(id) <= 1)
                {
                    Player p = players[Math.Abs(id)];
                    return Ok(p);
                }
                else
                {
                    return StatusCode(404, new ResponseMessage("Not found", "404", "There is no player with an id you have provided."));
                }
            }
            catch (Exception)
            {
                return StatusCode(404, new ResponseMessage("Not found", "404", "There is no player with an id you have provided."));
            }
        }
    }
}
