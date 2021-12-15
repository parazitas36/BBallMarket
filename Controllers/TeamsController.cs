using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BBallMarket.Authentication;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.TeamDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BBallMarket.Controllers
{
    [Authorize(Roles =UserRoles.Admin+","+UserRoles.User)]
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly ILogger<TeamsController> _logger;
        private readonly IMapper _imapper;
        private readonly IConfiguration _config;

        public TeamsController(ILogger<TeamsController> logger, IMapper imapper, IConfiguration config)
        {
            _logger = logger;
            _imapper = imapper;
            _config = config;
        }

        private SqlConnection ConnectToDB()
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("Local"));
            conn.Open();
            return conn;
        }

        // GET: api/<TeamController> all teams
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            SqlConnection conn = ConnectToDB();

            // Atrinkti unikalias komandas
            string query = @"
            SELECT DISTINCT t.id, t.teamName, p.id
            From Team t, Players p
            WHERE p.id = t.fk_owner
            ";
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            IList<Team> teams = new List<Team>();
            while (reader.Read())
            {
                var dataRecords = reader;
                Team team = new Team()
                {
                    TeamID = (int)dataRecords[0],
                    teamName = (string)dataRecords[1],
                    OwnerID = (int)dataRecords[2],
                    Players = new List<Player>()
                };
                teams.Add(team);
            }
            reader.Close(); reader = null;

            // Uzpildyti komandas zaidejais
            foreach(var team in teams)
            {
                string q = @"
                SELECT p.*
                FROM Players p, Team t
                WHERE p.fk_team = t.id AND t.id = @team
                ";
                cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@team", team.TeamID);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var playerRecords = reader;
                    Player p = new Player()
                    {
                        id = (int)playerRecords[0],
                        name = (string)playerRecords[1],
                        surname = (string)playerRecords[2],
                        height = (int)playerRecords[3],
                        weight = (int)playerRecords[4],
                        age = (int)playerRecords[5],
                        position = (string)playerRecords[6],
                        city = (string)playerRecords[7]
                    };
                    team.Players.Add(p);
                }
                reader.Close();
            }
            IList<GetTeamDTO> dto = new List<GetTeamDTO>();
            foreach(var team in teams)
            {
                dto.Add(_imapper.Map<GetTeamDTO>(team));
            }
            return Ok(dto);
        }

        // GET api/<TeamController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if(id.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }
            SqlConnection conn = ConnectToDB();

            // Patikrinti ar egzistuoja
            string query = @"
            SELECT t.id, t.teamName, fk_owner
            FROM Team t
            WHERE t.id = @team
            ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@team", Convert.ToInt32(id));
            SqlDataReader reader = cmd.ExecuteReader();
            Team team = null;
            if (!reader.HasRows) { return NotFound(); }
            while(reader.Read())
            {
                var dataRecords = reader;
                team = new Team()
                {
                    TeamID = (int)dataRecords[0],
                    teamName = (string)dataRecords[1],
                    OwnerID = (int)dataRecords[2],
                    Players = new List<Player>()
                };
            }
            reader.Close(); reader = null;

            // Uzpildyti komandas zaidejais
            string q = @"
            SELECT p.*
            FROM Players p, Team t
            WHERE p.fk_team = t.id AND t.id = @team
            ";
            cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@team", team.TeamID);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var playerRecords = reader;
                Player p = new Player()
                {
                    id = (int)playerRecords[0],
                    name = (string)playerRecords[1],
                    surname = (string)playerRecords[2],
                    height = (int)playerRecords[3],
                    weight = (int)playerRecords[4],
                    age = (int)playerRecords[5],
                    position = (string)playerRecords[6],
                    city = (string)playerRecords[7]
                };
                team.Players.Add(p);
            }
            reader.Close();
            return Ok(_imapper.Map<GetTeamDTO>(team));
        }

        // POST api/<TeamController>
        [Authorize(Roles =UserRoles.User)]
        [HttpPost("{pid}")]
        public async Task<IActionResult> Post(int pid, [FromBody] PostTeamDTO team)
        {
            Console.WriteLine(team.teamName);
            // Patikrinti ar naudotojas jau neturi komandos
            SqlConnection conn = ConnectToDB();

            // Sukurti komanda
            string query = @"
            INSERT INTO Team (fk_owner, teamName) 
            VALUES(@pid, @tName)
            ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", pid);
            cmd.Parameters.AddWithValue("@tName", team.teamName);
            cmd.ExecuteNonQuery();
            query = @"UPDATE Players SET fk_team = (SELECT t.id FROM Team t WHERE t.teamName=@teamName), fk_famarket = NULL WHERE id = @pid";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", pid);
            cmd.Parameters.AddWithValue("@teamName", team.teamName);
            cmd.ExecuteNonQuery();
            return Created(nameof(PostTeamDTO), team);
        }

        // DELETE api/<TeamController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if(id.ToCharArray().Where(x=> !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }

            // Patikrinti ar tokia komanda egzistuoja
            string query = @"
            SELECT *
            FROM Team
            WHERE id = @id
            ";
            SqlConnection conn = ConnectToDB();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", Convert.ToInt32(id));
            SqlDataReader reader = cmd.ExecuteReader();
            if(!reader.HasRows) { return NotFound(); }
            reader.Close();
            reader = null;
            
            string? uid = null;
            string? role = null;
            // Paima user id is tokeno
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                uid = (string)identity.FindFirst("uid").Value;
                role = (string)identity.FindFirst(ClaimTypes.Role).Value;
                Console.WriteLine(uid);
            }
            
            // Jei admin gali istrynt betkuria, user tik savo
            if(role == UserRoles.Admin)
            {
                // "Ismesti" zaidejus is komandos
                query = @"
                UPDATE Players
                SET fk_team = NULL
                WHERE fk_team = @teamid
                ";
                cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@teamid", Convert.ToInt32(id));
                cmd.ExecuteNonQuery();
            }
            else
            {
                // Patikrinti ar user sukure sia komanda
                query = @"
                SELECT t.id
                FROM Players p, Team t
                WHERE p.fk_account = @accid AND t.fk_owner = p.id AND t.id = @teamid
                ";
                cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@accid", uid);
                cmd.Parameters.AddWithValue("@teamid", Convert.ToInt32(id));
                reader = cmd.ExecuteReader();
                if(!reader.HasRows) { return Forbid(); }
                reader.Close();
                reader = null;

                // "Ismesti" zaidejus is komandos
                query = @"
                UPDATE Players
                SET fk_team = NULL
                WHERE fk_team = @teamid
                ";
                cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@teamid", Convert.ToInt32(id));
                cmd.ExecuteNonQuery();
            }

            // Panaikinti komandos pakvietimus
            query = @"
            DELETE FROM Invite
            WHERE fk_team = @teamid
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@teamid", Convert.ToInt32(id));
            cmd.ExecuteNonQuery();

            // Panaikinti komanda
            query = @"
            DELETE FROM Team
            WHERE id = @teamid
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@teamid", Convert.ToInt32(id));
            cmd.ExecuteNonQuery();
            return NoContent();
        }
    }
}
