using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace BBallMarket.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BMarketController : ControllerBase
    {
        Player[] players = new Player[2];
        Player player = new Player("Adas", "Bananas", "SG", 185.5f, 62.3f, 22, "Kaunas", "BC Nedametimas");
        Player newplayer = new Player("Aldonius", "Onylas", "SF", 199.5f, 62.3f, 22, "Vilnius", null);

        private readonly ILogger<BMarketController> _logger;

        public BMarketController(ILogger<BMarketController> logger)
        {
            players[0] = player;
            players[1] = newplayer;
            _logger = logger;
        }

        // Landing Page
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("index.html");
        }

        // Get Players List
        [HttpGet("players")]
        public IActionResult GetPlayerList()
        {
            return Ok(players);
        }

        // Get Player by ID
        [HttpGet("player/{id}")]
        public IActionResult GetPlayerById(string id)
        {
            try
            {
                if(id.ToCharArray().Where(x=> !Char.IsDigit(x)).Count() > 0)
                {
                    return StatusCode(400, new ResponseMessage("Bad Request", "400", "Player ID must be a number."));
                }
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

        // Update Player Data
        [HttpPut("player/{id}")]
        public IActionResult UpdatePlayerById(string id, [FromBody] Player newPlayer)
        {
            try
            {
                if (id.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
                {
                    return StatusCode(400, new ResponseMessage("Bad Request", "400", "Player ID must be a number."));
                }
                if (Math.Abs((Convert.ToInt32(id))) <= 1)
                {
                    Player p = players[Math.Abs((Convert.ToInt32(id)))];
                    Console.WriteLine("Player data before update: " + p.ToString());
                    p.UpdateData(newPlayer);
                    Console.WriteLine("Player data after update: " + p.ToString());
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

        // Login
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

        // Registration
        [HttpPost("registration")]
        public IActionResult Registration([FromBody] Registration account)
        {
            return Ok(account);
        }
    }
}
