using AutoMapper;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.PlayersDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayersController : ControllerBase
    {
        Player[] players = new Player[2];
        Player player = new Player(1, "Adas", "Bananas", "SG", 185.5f, 62.3f, 22, "Kaunas", "BC Nedametimas");
        Player newplayer = new Player(2, "Aldonius", "Onylas", "SF", 199.5f, 62.3f, 22, "Vilnius", null);

        private readonly ILogger<PlayersController> _logger;
        private readonly IMapper _imapper;

        public PlayersController(ILogger<PlayersController> logger, IMapper imapper)
        {
            players[0] = player;
            players[1] = newplayer;
            _logger = logger;
            _imapper = imapper;
        }

        // Get Players List
        [HttpGet]
        public async Task<IActionResult> GetPlayerList()
        {
            return Ok(players.Select(p => _imapper.Map<GetPlayerDTO>(p)));
        }

        // Get Player by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayerById(string id)
        {
            try
            {
                if(id.ToCharArray().Where(x=> !Char.IsDigit(x)).Count() > 0)
                {
                    return BadRequest("Player ID must be a number.");
                }
                if (Math.Abs((Convert.ToInt32(id))) <= 1)
                {
                    Player p = players[Math.Abs((Convert.ToInt32(id)))];
                    return Ok(_imapper.Map<GetPlayerDTO>(p));
                }
                else
                {
                    return NotFound($"There is no player with an ID: {id}.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // Create Player
        [HttpPost]
        public async Task<IActionResult> CreatePlayer(PostPlayerDTO player)
        { 
            return Ok(player);
        }

        // Update Player Data
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlayerById(string id, [FromBody] UpdatePlayerDTO player)
        {
            try
            {
                if (id.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
                {
                    return BadRequest("Player ID must be a number.");
                }
                if (Math.Abs((Convert.ToInt32(id))) <= 1)
                {
                    Player p = players[Math.Abs((Convert.ToInt32(id)))];
                    Console.WriteLine("Player data before update: " + p.ToString());
                    p.UpdateData(_imapper.Map<Player>(player));
                    Console.WriteLine("Player data after update: " + p.ToString());
                    return Ok(_imapper.Map<GetPlayerDTO>(p));
                }
                else
                {
                    return NotFound($"There is no player with an ID: {id}.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // Delete Player by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayerByID(string id)
        {
            try
            {
                if(id.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
                {
                    return BadRequest("Player ID must be a number.");
                }
                if (Math.Abs((Convert.ToInt32(id))) <= 1)
                {
                    Player p = players[Math.Abs((Convert.ToInt32(id)))];
                    players[Convert.ToInt32(id)] = null;
                    Console.WriteLine("Deleted: " + p.ToString());
                    return NoContent();
                }
                else
                {
                    return NotFound($"There is no player with an ID: {id}.");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
