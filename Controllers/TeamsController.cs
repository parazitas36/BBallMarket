using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.TeamDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BBallMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly ILogger<TeamsController> _logger;
        private readonly IMapper _imapper;

        IList<Player> players = new List<Player>();
        Player player = new Player(0, "Adas", "Bananas", "SG", 185.5f, 62.3f, 22, "Kaunas", "BC Nedametimas");
        Player newplayer = new Player(1, "Aldonius", "Onylas", "SF", 199.5f, 62.3f, 22, "Vilnius", "Trajakininkai");
        Player newplayer1 = new Player(2, "Edas", "Kiauras", "C", 210.5f, 100.3f, 25, "Vilnius", "Trajakininkai");

        Team team1;
        Team team2;
        IList<Team> teams;

        public TeamsController(ILogger<TeamsController> logger, IMapper imapper)
        {
            players.Add(player);
            players.Add(newplayer);
            players.Add(newplayer1);

            IList<Player> t1players = new List<Player>();
            t1players.Add(player);
            IList<Player> t2players = new List<Player>();
            t2players.Add(newplayer);
            t2players.Add(newplayer1);

            team1 = new Team("Savininkas1", t1players[0].team, t1players);
            team2 = new Team("Savininkas2", t2players[1].team, t2players);
            teams = new List<Team>();

            teams.Add(team1);
            teams.Add(team2);

            _logger = logger;
            _imapper = imapper;
        }

        // GET: api/<TeamController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(teams);
        }

        // GET api/<TeamController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if(id.ToCharArray().Where(x=> !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }
            if(Convert.ToInt32(id) > 1) { return NotFound(); }
            return Ok(teams[Convert.ToInt32(id)]);
        }

        // POST api/<TeamController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Team team)
        {
            return Created(nameof(team), team);
        }

        // PUT api/<TeamController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UpdateTeamDTO team)
        {
            if (id.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }
            if(Convert.ToInt32(id) > 2) { return NotFound(); }
            teams[Convert.ToInt32(id)].updateTeam(team.Owner, team.TeamName, _imapper.Map<Player>(team.player));
            return Ok(_imapper.Map<GetTeamDTO>(teams[Convert.ToInt32(id)]));
        }

        // DELETE api/<TeamController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if(id.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }
            if(Convert.ToInt32(id) > 1) { return NotFound(); }
            return NoContent();
        }
    }
}
