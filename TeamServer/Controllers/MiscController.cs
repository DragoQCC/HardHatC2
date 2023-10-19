using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Services;

namespace HardHatCore.TeamServer.Controllers
{
    [Authorize(Roles = "Operator,TeamLead")]
    [ApiController]
    [Route("[controller]")]
    public class MiscController : ControllerBase
    {
        //post to add a new credential
        [HttpPost("/addcred" ,Name = "AddCredential")]
        public async Task<IActionResult> AddCredential([FromBody] Cred credential)
        {
            try
            {
                await HardHatHub.AddCreds( new List<Cred>() { credential },true);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in add cred api: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }


    }
}
