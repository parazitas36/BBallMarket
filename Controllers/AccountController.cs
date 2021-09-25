using BBallMarket.Data.Entities;
using BBallMarket.Data.Models.AccountDTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BBallMarket.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // Login
        [HttpGet]
        public async Task<IActionResult> Login([FromBody] Account account)
        {
            account.id = 1;
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
        [HttpPost]
        public async Task<IActionResult> Registration([FromBody] PostAccountDTO account)
        {
            return Ok(account);
        }
    }
}
