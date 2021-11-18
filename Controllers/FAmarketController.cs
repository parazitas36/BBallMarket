using AutoMapper;
using BBallMarket.Authentication;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.FADTO;
using BBallMarket.Data.Models.InviteDTO;
using BBallMarket.Data.Models.PlayersDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;


namespace BBallMarket.Controllers
{
    [Authorize(Roles =UserRoles.User + "," + UserRoles.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class FAmarketController : ControllerBase
    {
        private readonly ILogger<FAmarketController> _logger;
        private readonly IMapper _imapper;
        private readonly IConfiguration _config;

        public FAmarketController(IConfiguration config, ILogger<FAmarketController> logger, IMapper imapper)
        {
            _logger = logger;
            _imapper = imapper;
            _config = config;
        }

        private SqlConnection ConnectToDB()
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("Default"));
            conn.Open();
            return conn;
        }

        // GET All available cities: api/famarket
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            SqlConnection conn = ConnectToDB();
            string query = "SELECT * FROM FAmarket";
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            IList<GetFADTO> dtos = new List<GetFADTO>();
            while (reader.Read())
            {
                IDataRecord dataRecord = (IDataRecord)reader;
                GetFADTO dto = new GetFADTO();
                dto.city = (string)dataRecord[1];
                dtos.Add(dto);
            }
            conn.Close();
            return Ok(dtos);
        }

        // GET City: api/famarket/city
        [HttpGet("{city}")]
        public async Task<IActionResult> GetCity(string city)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }
            SqlConnection conn = ConnectToDB();
            string query = "SELECT * FROM FAmarket WHERE id=@city";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            IDataRecord dataRecord = (IDataRecord)reader;
            GetFADTO dto = new GetFADTO();
            if (reader.HasRows) { dto.city = (string)dataRecord[1]; }
            else { conn.Close(); return NotFound(); }
            conn.Close();
            return Ok(dto);
        }

        [Authorize(Roles=UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> PostCity([FromBody] PostFADTO fa)
        {
            // Patikrinti ar toks neegzistuoja
            SqlConnection conn = ConnectToDB();
            string query = "SELECT * FROM FAmarket WHERE city=@city";
            SqlCommand cmd = new SqlCommand(query, conn);
            FAmarket fm = _imapper.Map<FAmarket>(fa);
            cmd.Parameters.AddWithValue("@city", fm.city);
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (reader.HasRows) { conn.Close(); return Conflict(); }
            reader.Close();

            // Pridet i DB
            query = "INSERT INTO [dbo].[FAmarket](city) VALUES(@city)";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", fm.city);
            cmd.ExecuteNonQuery();
            conn.Close();

            GetFADTO dto = _imapper.Map<GetFADTO>(fm);
            return Created(nameof(dto), dto);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPatch("{city}")]
        public async Task<IActionResult> PatchCity(string city, [FromBody] GetFADTO fa)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }
            // Patikrinti ar redaguojamas miestas egzistuoja
            FAmarket fm = _imapper.Map<FAmarket>(fa);
            SqlConnection conn = ConnectToDB();
            string query = "SELECT * FROM FAmarket WHERE id=@city";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (!reader.HasRows) { conn.Close(); return NotFound(); }
            reader.Close();

            // Patikrinti ar nauja reiksme jau egzsituoja
            query = "SELECT * FROM FAmarket WHERE city=@city";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", fm.city);
            reader = cmd.ExecuteReader();
            reader.Read();
            if (reader.HasRows) { conn.Close(); return Conflict(); }
            else
            {
                reader.Close();
                // Atnaujinti irasa
                query = "UPDATE FAmarket SET city=@city WHERE id=@oldcity";
                cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@city", fm.city);
                cmd.Parameters.AddWithValue("@oldcity", Convert.ToInt32(city));
                cmd.ExecuteNonQuery();
                conn.Close();
                return Ok();
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{city}")]
        public async Task<IActionResult> DeleteCity(string city)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }

            // Patikrinti ar redaguojamas miestas egzistuoja
            SqlConnection conn = ConnectToDB();
            string query = "SELECT id FROM FAmarket WHERE id=@city";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (!reader.HasRows) { conn.Close(); return NotFound(); }
            reader.Read();
            int cityid = (int)reader[0];
            reader.Close();

            // Pakeisti zaideju duomenis, kurie registruoti sitam mieste
            query = "UPDATE Players SET fk_famarket=NULL WHERE fk_famarket = @cityid";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@cityid", cityid);
            cmd.ExecuteNonQuery();

            // Istrinti miesta
            query = "DELETE FROM FAmarket WHERE id=@city";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.ExecuteNonQuery();
            conn.Close();
            return NoContent();
        }

        /*
            PLAYERS:
            GET
            GET(id)
            POST
            DELETE
         */

        // GET Players By City: api/famarket/city/players
        [HttpGet("{city}/players")]
        public async Task<IActionResult> GetCityPlayers(string city)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }

            IList<Player> list = new List<Player>();
            SqlConnection conn = ConnectToDB();
            string query = "SELECT DISTINCT Players.id, Players.name, Players.surname, Players.height, Players.weight, Players.age, Players.position, Players.city, FAmarket.city as FAmarket , CASE WHEN Players.fk_team IS NULL THEN NULL ELSE Team.teamName END AS Team " +
                            " FROM Players, FAmarket, Team " +
                            " WHERE FAmarket.id = Players.fk_famarket AND (Team.id = Players.fk_team OR Players.fk_team IS NULL) AND FAmarket.id=@city ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                IDataRecord dataRecord = (IDataRecord)reader;
                int id = (int)dataRecord[0];
                string name = (string)dataRecord[1];
                string surname = (string)dataRecord[2];
                int height = (int)dataRecord[3];
                int weight = (int)dataRecord[4];
                int age = (int)dataRecord[5];
                string pos = (string)dataRecord[6];
                string player_city = (string)dataRecord[7];
                string famarket = dataRecord[8].ToString() == "" ? null : (string)dataRecord[8];
                string team = dataRecord[9].ToString() == "" ? null : (string)dataRecord[9]; ;
                Player player = new Player(id, name, surname, pos, height, weight, age, player_city, team, famarket);
                list.Add(player);
            }
            if (list.Count == 0) { return NotFound(); }
            IList<GetPlayerDTO> dtoList = list.Select(x => _imapper.Map<GetPlayerDTO>(x)).ToList();
            return Ok(dtoList);
        }

        // GET Player by ID: api/famarket/city/players/playerid
        [HttpGet("{city}/players/{playerid}")]
        public async Task<IActionResult> GetPlayerByID(string city, string playerid)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }

            int id = Convert.ToInt32(playerid);
            // Patikrinti ar toks yra
            string query = "SELECT Players.id, Players.name, Players.surname, Players.height, Players.weight, Players.age, Players.position, Players.city, FAmarket.city as FAmarket , CASE WHEN Players.fk_team IS NULL THEN NULL ELSE Team.teamName END AS Team " +
                                " FROM Players, FAmarket, Team " +
                                " WHERE FAmarket.id = Players.fk_famarket AND (Team.id = Players.fk_team OR Players.fk_team IS NULL) AND FAmarket.id=@city AND Players.id=@id";
            SqlConnection conn = ConnectToDB();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.Parameters.AddWithValue("@id", id);
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) { return NotFound();}
            reader.Read();
            // Grazinti
            IDataRecord dataRecord = (IDataRecord)reader;
            id = (int)dataRecord[0];
            string name = (string)dataRecord[1];
            string surname = (string)dataRecord[2];
            int height = (int)dataRecord[3];
            int weight = (int)dataRecord[4];
            int age = (int)dataRecord[5];
            string pos = (string)dataRecord[6];
            string player_city = (string)dataRecord[7];
            string famarket = dataRecord[8].ToString() == "" ? null : (string)dataRecord[8];
            string team = dataRecord[9].ToString() == "" ? null : (string)dataRecord[9]; ;
            Player player = new Player(id, name, surname, pos, height, weight, age, player_city, team, famarket);
            conn.Close();

            return Ok(player);
        }

        // POST Add player to the market api/famarket/city/players/
        [Authorize(Roles =UserRoles.User)]
        [HttpPost("{city}/players")]
        public async Task<IActionResult> PostPlayer(string city, [FromBody] PostPlayerDTO player)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }

            // Paima user id is tokeno
            string? uid = null;
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                uid = (string)identity.FindFirst("uid").Value;
                Console.WriteLine(uid);
            }

            SqlConnection conn = ConnectToDB();
            // Patikrinti ar nera zaidejo registruoto su sia paskyra
            string checkQuery = "SELECT * FROM Players WHERE fk_account = @accid";
            SqlCommand checkCMD = new SqlCommand(checkQuery, conn);
            checkCMD.Parameters.AddWithValue("@accid", uid);
            SqlDataReader reader = checkCMD.ExecuteReader();
            if (reader.HasRows) 
            { 
                return Conflict(
                    new Response() { Status = "409", Message = "There is a player already associated with this account!"}
                    );
            }
            reader.Close();

            Player p = _imapper.Map<Player>(player);
            // Iterpti zaideja i db
            string query = @"
            INSERT INTO Players (name, surname, height, weight, age, position, city, fk_famarket, fk_account) 
            VALUES(@name, @surname, @height, @weight, @age, @position, @city, @famarket, @accid)
            ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", p.name);
            cmd.Parameters.AddWithValue("@surname", p.surname);
            cmd.Parameters.AddWithValue("@height", p.height);
            cmd.Parameters.AddWithValue("@weight", p.weight);
            cmd.Parameters.AddWithValue("@age", p.age);
            cmd.Parameters.AddWithValue("@position", p.position);
            cmd.Parameters.AddWithValue("@city", p.city);
            cmd.Parameters.AddWithValue("@famarket", Convert.ToInt32(city));
            cmd.Parameters.AddWithValue("@accid", uid);
            cmd.ExecuteNonQuery();
            conn.Close();

            GetPlayerDTO dto = _imapper.Map<GetPlayerDTO>(p);
            return Created(nameof(dto), dto);
        }

        // Update Player by ID: api/famarket/city/players/playerid
        [HttpPatch("{city}/players/{playerid}")]
        public async Task<IActionResult> UpdatePlayerByID(string city, string playerid, [FromBody] UpdatePlayerDTO player)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }

            string? uid = null;
            // Paima user id is tokeno
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                uid = (string)identity.FindFirst("uid").Value;
                Console.WriteLine(uid);
            }

            // Patikrinti ar egzistuoja
            string query = @"
            SELECT Players.id, Players.fk_famarket, FAmarket.id, FAmarket.city 
            FROM Players, FAmarket 
            WHERE Players.id = @pid AND FAmarket.id = Players.fk_famarket AND FAmarket.id = @city AND fk_account = @accid
            ";
            SqlConnection conn = ConnectToDB();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", playerid);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.Parameters.AddWithValue("@accid", uid);
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) { return NotFound(); }
            reader.Close();

            // Atnaujinti
            query = @"
            UPDATE Players
            SET
            fk_team = @team,
            fk_famarket = NULL
            WHERE id = @playerid AND 
            fk_famarket = @city AND
            fk_account = @accid
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@team", Convert.ToInt32(player.team));
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.Parameters.AddWithValue("@accid", uid);
            cmd.ExecuteNonQuery();
            conn.Close();
            return NoContent();
        }


        // Delete Player by ID: api/famarket/city/players/playerid
        [HttpDelete("{city}/players/{playerid}")]
        public async Task<IActionResult> DeletePlayerByID(string city, string playerid)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }

            string? role = null;
            string? uid = null;
            // Paima user id ir role is tokeno
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                uid = (string)identity.FindFirst("uid").Value;
                role = (string)identity.FindFirst(ClaimTypes.Role).Value;
                Console.WriteLine(uid);
                Console.WriteLine(role);
            }

            // Patikrinti ar egzistuoja
            string query = @"
            SELECT Players.id, Players.fk_famarket, FAmarket.id, FAmarket.city 
            FROM Players, FAmarket 
            WHERE Players.id = @pid AND FAmarket.id = Players.fk_famarket AND FAmarket.id = @city
            ";
            SqlConnection conn = ConnectToDB();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", playerid);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) { return NotFound(); }
            reader.Close();
            reader = null;
            
            // Ar sis vartotojas sukure si zaideja
            if(role != UserRoles.Admin)
            {
                string q = @"
                SELECT * FROM Players WHERE fk_account = @accid AND id = @playerid AND fk_famarket = @cityid
                ";
                SqlCommand checkCmd = new SqlCommand(q, conn);
                checkCmd.Parameters.AddWithValue("@accid", uid);
                checkCmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
                checkCmd.Parameters.AddWithValue("@cityid", Convert.ToInt32(city));
                SqlDataReader read = cmd.ExecuteReader();
                if(!read.HasRows) { return Forbid(); }
                read.Close();
            }

            // Panaikinti
            query = @"
            DELETE FROM Players
            WHERE Players.id = @playerid AND 
            (SELECT FAmarket.city 
            FROM FAmarket WHERE FAmarket.id = Players.fk_famarket 
            AND FAmarket.id = @city) IS NOT NULL
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.ExecuteNonQuery();  
            conn.Close();
            return NoContent();
        }

        /*
            INVITES:
            GET
            GET(id)
            Post
            Put
            Delete
        */

        // GET Invite by ID api/famarket/city/players/playerid/invites/inviteid
        [HttpGet("{city}/players/{playerid}/invites/{inviteid}")]
        public async Task<IActionResult> Get(string city, string playerid, string inviteid)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 || 
                playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
                inviteid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) 
            { 
                return BadRequest(); 
            }

            SqlConnection conn = ConnectToDB();
            string query = @"
            SELECT Invite.id as inviteID,  Team.teamName, p.id, p.name, p.surname, p.height,
            p.weight, p.age, p.position, p.city, FAmarket.city, inviteStatus.status
            FROM Players p
	            JOIN Invite ON Invite.fk_player = p.id
	            JOIN FAmarket ON p.fk_famarket = FAmarket.id
	            JOIN Team ON Team.id = Invite.fk_team
	            JOIN inviteStatus ON Invite.fk_inviteStatus = inviteStatus.id
            WHERE FAmarket.id = @city AND p.id = @playerid AND Invite.id=@inviteID
            ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.Parameters.AddWithValue("@inviteID", Convert.ToInt32(inviteid));
            SqlDataReader reader = cmd.ExecuteReader();
            if(!reader.HasRows) { return NotFound(); }
            reader.Read();
            IDataRecord records = (IDataRecord)reader;
            int inviteID = (int)records[0];
            string teamName = (string)records[1];
            int playerID = (int)records[2];
            string name = (string)records[3];
            string surname = (string)records[4];
            int height = (int)records[5];
            int weight = (int)records[6];
            int age = (int)records[7];
            string position = (string)records[8];
            string playerCity = (string)records[9];
            string? famarket = (string?)records[10];
            Player p = new Player(playerID, name, surname, position, height, weight, age,
                playerCity, famarket, teamName);
            string inviteStatus = (string)records[11];
            Invite inv = new Invite(inviteID, teamName, p, inviteStatus);
            GetInviteDTO dto = _imapper.Map<GetInviteDTO>(inv);
            return Ok(dto);   
        }

        // GET All invites api/famarket/city/players/playerid/invites
        [HttpGet("{city}/players/{playerid}/invites")]
        public async Task<IActionResult> GetInvite(string city, string playerid)
        {
            if (playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
                city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            { return BadRequest(); }

            SqlConnection conn = ConnectToDB();

            // Patikrint ar egzsituoja
            string existQuery = @"
            SELECT Players.id, FAmarket.id
            FROM Players, FAmarket
            WHERE Players.id = @playerid AND FAmarket.id = @cityid
            ";
            SqlCommand cmd = new SqlCommand(existQuery, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@cityid", city);
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) { return NotFound(); }
            reader.Close();

            // Gauti pakvietimus
            string query = @"
            SELECT Invite.id as inviteID,  Team.teamName, p.id, p.name, p.surname, p.height,
            p.weight, p.age, p.position, p.city, FAmarket.city, inviteStatus.status
            FROM Players p
	            JOIN Invite ON Invite.fk_player = p.id
	            JOIN FAmarket ON p.fk_famarket = FAmarket.id
	            JOIN Team ON Team.id = Invite.fk_team
	            JOIN inviteStatus ON Invite.fk_inviteStatus = inviteStatus.id
            WHERE FAmarket.id = @city AND p.id = @playerid
            ";
            IList<GetInviteDTO> invitesDTO = new List<GetInviteDTO>();
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                IDataRecord records = (IDataRecord)reader;
                int inviteID = (int)records[0];
                string teamName = (string)records[1];
                int playerID = (int)records[2];
                string name = (string)records[3];
                string surname = (string)records[4];
                int height = (int)records[5];
                int weight = (int)records[6];
                int age = (int)records[7];
                string position = (string)records[8];
                string playerCity = (string)records[9];
                string? famarket = (string?)records[10];
                Player p = new Player(playerID, name, surname, position, height, weight, age,
                    playerCity, famarket, teamName);
                string inviteStatus = (string)records[11];
                Invite inv = new Invite(inviteID, teamName, p, inviteStatus);
                invitesDTO.Add(_imapper.Map<GetInviteDTO>(inv));
            }
            return Ok(invitesDTO);
        }

        // POST Send invite to the player: api/famarket/city/players/playerid/invites/inviteid
        [HttpPost("{city}/players/{playerid}/invites")]
        public async Task<IActionResult> Post(string city, string playerid)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
                playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            { return BadRequest(); }

            string? uid = null;
            // Paima user id is tokeno
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                uid = (string)identity.FindFirst("uid").Value;
                Console.WriteLine(uid);
            }

            SqlConnection conn = ConnectToDB();

            // Patikrinti ar egzsituoja
            string existQuery = @"
            SELECT Players.id, FAmarket.id
            FROM Players, FAmarket
            WHERE Players.id = @playerid AND FAmarket.id = @city
            ";
            SqlCommand cmd = new SqlCommand(existQuery, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            if(!reader.HasRows) { return NotFound(); }
            reader.Close();

            // Patikrinti ar prisijunges vartotojas turi komanda (gali pakviesti zaideja)
            string checkQuery = @"
            SELECT t.id
            FROM Players p, AspNetUsers acc, Team t
            WHERE acc.Id = p.fk_account AND p.id = t.fk_owner AND acc.Id = @accid
            ";
            cmd = new SqlCommand(checkQuery, conn);
            cmd.Parameters.AddWithValue("@accid", uid);
            reader = cmd.ExecuteReader();
            if(!reader.HasRows) { return NotFound(new Response() { Status = "404", Message = "You cannot invite other players until you own a team!" }); }
            reader.Read();
            IDataRecord dataRecords = (IDataRecord)reader;
            int teamID = (int)dataRecords[0];
            reader.Close();
            reader = null;

            // Patikrinti ar prisijunges vartotojas jau kviete si zaideja
            checkQuery = @"
            SELECT *
            FROM Invite
            WHERE Invite.fk_player = @playerid AND Invite.fk_team = @team
            ";
            cmd = new SqlCommand(checkQuery, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@team", teamID);
            reader = cmd.ExecuteReader();
            if(reader.HasRows) { return Conflict(new Response() { Status = "409", Message = "This player is already invited to your team!"}); }
            reader.Close();
            reader = null;

            // Irasyti pakvietima i DB
            string query = @"
            INSERT INTO Invite (fk_team, fk_player)
            VALUES (@team, @playerid)
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@team", teamID);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.ExecuteNonQuery();

            // Paimti sukurta objekta
            query = @"
            SELECT p.id, t.teamName, inv.id
            FROM Players p, Team t, Invite inv
            WHERE p.id = @playerid AND t.id = @team
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@team", teamID);
            reader = cmd.ExecuteReader();
            reader.Read();
            dataRecords = null;
            dataRecords = (IDataRecord)reader;

            PostInviteDTO invite = new PostInviteDTO()
            {
                inviteID = (int)dataRecords[2],
                playerID = (int)dataRecords[0],
                teamName = (string)dataRecords[1]
            };

            return Created(nameof(PostInviteDTO), invite);
        }

        // PATCH Update invite status: api/famarket/city/players/playerid/invites/inviteid
        [HttpPatch("{city}/players/{playerid}/invites/{inviteid}/")]
        public async Task<IActionResult> Patch(string city, string playerid, string inviteid, [FromBody] UpdateInviteDTO inviteStatus)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               inviteid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }

            string? uid = null;
            // Paima user id is tokeno
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                uid = (string)identity.FindFirst("uid").Value;
                Console.WriteLine(uid);
            }

            SqlConnection conn = ConnectToDB();
            // Patikrinti ar toks egzsituoja
            string existQuery = @"
            SELECT p.id, i.id, fa.id
            FROM Players p, Invite i, FAmarket fa
            WHERE p.id = @playerid AND i.id = @inviteid AND fa.id = @cityid AND i.fk_player = p.id
            ";
            SqlCommand cmd = new SqlCommand(existQuery, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@inviteid", Convert.ToInt32(inviteid));
            cmd.Parameters.AddWithValue("@cityid", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) { return NotFound(); }
            reader.Close();

            // Patikrinti ar tai prisijungusio vartotojo kvietimas
            string checkQuery = @"
            SELECT p.fk_account, acc.Id, inv.id
            FROM Players p, AspNetUsers acc, Invite inv
            WHERE p.id = @playerid AND p.fk_account = acc.Id AND acc.Id = @accid AND p.fk_famarket = @city AND inv.fk_player = p.id AND inv.id = @inviteid
            ";
            SqlCommand command = new SqlCommand(checkQuery, conn);
            command.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            command.Parameters.AddWithValue("@accid", uid);
            command.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            command.Parameters.AddWithValue("@inviteid", Convert.ToInt32(inviteid));
            SqlDataReader read = command.ExecuteReader();
            if(!read.HasRows) { return Forbid(); }
            read.Close();

            // Atnaujinti pakvietimo busena
            string query = @"
            UPDATE Invite SET fk_inviteStatus=(SELECT id FROM inviteStatus WHERE status = @invStatus)
            WHERE id = @inviteid
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@inviteid", Convert.ToInt32(inviteid));
            cmd.Parameters.AddWithValue("@invStatus", inviteStatus.inviteStatus);
            cmd.ExecuteNonQuery();
            return Ok();
        }

        // DELETE Invite api/famarket/city/players/playerid/invites/inviteid
        [HttpDelete("{city}/players/{playerid}/invites/{inviteid}")]
        public async Task<IActionResult> Delete(string city, string playerid, string inviteid)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               inviteid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            { return BadRequest(); }

            string? uid = null;
            // Paima user id is tokeno
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                uid = (string)identity.FindFirst("uid").Value;
                Console.WriteLine(uid);
            }

            // Patikrinti ar yra toks pakvietimas
            SqlConnection conn = ConnectToDB();
            string query = @"
            SELECT Invite.id as inviteID,  Team.teamName, p.id, p.name, p.surname, p.height,
            p.weight, p.age, p.position, p.city, FAmarket.city, inviteStatus.status
            FROM Players p
	            JOIN Invite ON Invite.fk_player = p.id
	            JOIN FAmarket ON p.fk_famarket = FAmarket.id
	            JOIN Team ON Team.id = Invite.fk_team
	            JOIN inviteStatus ON Invite.fk_inviteStatus = inviteStatus.id
            WHERE FAmarket.id = @city AND p.id = @playerid AND Invite.id=@inviteID
            ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.Parameters.AddWithValue("@inviteID", Convert.ToInt32(inviteid));
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) { return NotFound(); }
            reader.Close();

            // Istrinti
            query = @"
            DELETE FROM Invite WHERE Invite.id=@inviteID AND (SELECT Players.fk_account FROM Players WHERE Players.id = @playerid) = @accid 
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.Parameters.AddWithValue("@inviteID", Convert.ToInt32(inviteid));
            cmd.Parameters.AddWithValue("@accid", uid);
            if(cmd.ExecuteNonQuery() == 0) { return Forbid(); }
            return NoContent();
        }
    }
}
