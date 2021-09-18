using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BBallMarket.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BMarketController : ControllerBase
    {

        Player[] players = new Player[2];
        Player player = new Player("Vardas", "Pavarde", "Atakuojantis gynejas", 165.5f, 62.3f, 22, "Kaunas", "Komandos pavadinimas");
        Player newplayer = new Player("Vardas2", "Pavarde2", "Atakuojantis gynejas2", 165.5f, 62.3f, 22, "Kaunas", null);

        private readonly ILogger<BMarketController> _logger;

        public BMarketController(ILogger<BMarketController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("index.html");
        }
        [HttpGet("players")]
        public IActionResult GetPlayerList()
        {
            players[0] = player;
            players[1] = newplayer;
            return Ok(players);
        }
        [HttpGet("player/{id}")]
        public IActionResult GetPlayerById(string id)
        {
            try
            {
                if(id.ToCharArray().Where(x=> !Char.IsDigit(x)).Count() > 0)
                {
                    return StatusCode(400, new ResponseMessage("Bad Request", "400", "Player ID must be a number."));
                }
                players[0] = player;
                players[1] = newplayer;
                if (Math.Abs((Convert.ToInt32(id))) <= 1)
                {
                    Player p = players[Math.Abs((Convert.ToInt32(id)))];
                    return Ok(p);
                }
                else
                {
                    return StatusCode(404, new ResponseMessage("Not found", "404", "There is no player with an id you have provided."));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [HttpGet("login")]
        public IActionResult Login([FromBody] Login account)
        {
            if (account.username == "lukas123")
            {
                return Ok("Logged in as: " + account.username);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost("registration")]
        public IActionResult Registration([FromBody] Registration account)
        {
            return Ok(account);
        }
    }
}
