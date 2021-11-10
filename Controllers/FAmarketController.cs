using AutoMapper;
using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.FADTO;
using BBallMarket.Data.Models.InviteDTO;
using BBallMarket.Data.Models.PlayersDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace BBallMarket.Controllers
{
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
        [HttpPut("{city}")]
        public async Task<IActionResult> PutCity(string city, [FromBody] GetFADTO fa)
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

        [HttpDelete("{city}")]
        public async Task<IActionResult> DeleteCity(string city)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0) { return BadRequest(); }
            // Patikrinti ar redaguojamas miestas egzistuoja
            SqlConnection conn = ConnectToDB();
            string query = "SELECT * FROM FAmarket WHERE id=@city";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (!reader.HasRows) { conn.Close(); return NotFound(); }
            reader.Close();

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
            //if (Convert.ToInt32(city) == 1)
            //{
            //    string query = "SELECT DISTINCT Players.id, Players.name, Players.surname, Players.height, Players.weight, Players.age, Players.position, Players.city, FAmarket.city as FAmarket , CASE WHEN Players.fk_team IS NULL THEN NULL ELSE Team.teamName END AS Team " +
            //                    " FROM Players, FAmarket, Team " +
            //                    " WHERE (FAmarket.id = Players.fk_famarket) AND (Team.id = Players.fk_team OR Players.fk_team IS NULL) ";
            //    SqlCommand cmd = new SqlCommand(query, conn);
            //    SqlDataReader reader = cmd.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        IDataRecord dataRecord = (IDataRecord)reader;
            //        int id = (int)dataRecord[0];
            //        string name = (string)dataRecord[1];
            //        string surname = (string)dataRecord[2];
            //        int height = (int)dataRecord[3];
            //        int weight = (int)dataRecord[4];
            //        int age = (int)dataRecord[5];
            //        string pos = (string)dataRecord[6];
            //        string player_city = (string)dataRecord[7];
            //        string famarket = dataRecord[8].ToString() == "" ? null : (string)dataRecord[8];
            //        string team = dataRecord[9].ToString() == "" ? null : (string)dataRecord[9]; ;
            //        Player player = new Player(id, name, surname, pos, height, weight, age, player_city, team, famarket);
            //        list.Add(player);
            //    }
            //}
            //else
            //{
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
            //}
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
        [HttpPost("{city}/players")]
        public async Task<IActionResult> PostPlayer(string city, [FromBody] PostPlayerDTO player)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }
            Player p = _imapper.Map<Player>(player);
            // Iterpti zaideja i db
            string query = @"
            INSERT INTO Players (name, surname, height, weight, age, position, city, fk_famarket) 
            VALUES(@name, @surname, @height, @weight, @age, @position, @city, @famarket)
            ";
            SqlConnection conn = ConnectToDB();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", p.name);
            cmd.Parameters.AddWithValue("@surname", p.surname);
            cmd.Parameters.AddWithValue("@height", p.height);
            cmd.Parameters.AddWithValue("@weight", p.weight);
            cmd.Parameters.AddWithValue("@age", p.age);
            cmd.Parameters.AddWithValue("@position", p.position);
            cmd.Parameters.AddWithValue("@city", p.city);
            cmd.Parameters.AddWithValue("@famarket", Convert.ToInt32(city));
            cmd.ExecuteNonQuery();
            conn.Close();

            GetPlayerDTO dto = _imapper.Map<GetPlayerDTO>(p);
            return Created(nameof(dto), dto);
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
        public async Task<IActionResult> Post(string city, string playerid, [FromBody] PostInviteDTO postInvite)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
                playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            { return BadRequest(); }

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

            // Irasyti pakvietima i DB
            string teamName = postInvite.teamName;
            string query = @"
            INSERT INTO Invite (fk_team, fk_player)
            VALUES ((SELECT id FROM Team WHERE teamName = @team), @playerid)
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@team", teamName);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.ExecuteNonQuery();
            GetInviteDTO getDTO = new GetInviteDTO();
            getDTO.inviteStatusID = 1;
            getDTO.playerID = Convert.ToInt32(playerid);
            getDTO.team = teamName;
            return Ok(getDTO);
        }

        // PUT Update invite status: api/famarket/city/players/playerid/invites/inviteid
        [HttpPut("{city}/players/{playerid}/invites/{inviteid}/")]
        public async Task<IActionResult> Put(string city, string playerid, string inviteid, [FromBody] UpdateInviteDTO inviteStatus)
        {
            if (city.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               playerid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0 ||
               inviteid.ToCharArray().Where(x => !Char.IsDigit(x)).Count() > 0)
            {
                return BadRequest();
            }

            SqlConnection conn = ConnectToDB();
            // Patikrinti ar toks egzsituoja
            string existQuery = @"
            SELECT p.id, i.id, fa.id
            FROM Players p, Invite i, FAmarket fa
            WHERE p.id = @playerid AND i.id = @inviteid AND fa.id = @cityid
            ";
            SqlCommand cmd = new SqlCommand(existQuery, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@inviteid", Convert.ToInt32(inviteid));
            cmd.Parameters.AddWithValue("@cityid", Convert.ToInt32(city));
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) { return NotFound(); }
            reader.Close();

            // Atnaujinti pakvietimo busena
            string query = @"
            UPDATE Invite SET fk_inviteStatus=@invStatus
            WHERE id = @inviteid
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@inviteid", Convert.ToInt32(inviteid));
            cmd.Parameters.AddWithValue("@invStatus", Convert.ToInt32(inviteStatus.inviteStatus));
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
            query = @"
            DELETE FROM Invite WHERE Invite.id=@inviteID
            ";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerid", Convert.ToInt32(playerid));
            cmd.Parameters.AddWithValue("@city", Convert.ToInt32(city));
            cmd.Parameters.AddWithValue("@inviteID", Convert.ToInt32(inviteid));
            cmd.ExecuteNonQuery();
            return NoContent();
        }
    }
}
