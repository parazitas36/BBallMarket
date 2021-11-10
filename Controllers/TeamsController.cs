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


        public TeamsController(ILogger<TeamsController> logger, IMapper imapper)
        {
            _logger = logger;
            _imapper = imapper;
        }

        // GET: api/<TeamController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }

        // GET api/<TeamController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            return Ok();
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
            return Ok();
        }

        // DELETE api/<TeamController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return NoContent();
        }
    }
}
