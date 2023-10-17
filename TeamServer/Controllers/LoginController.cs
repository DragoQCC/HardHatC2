using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using TeamServer.Models.Database;
using TeamServer.Services.Extra;
using TeamServer.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ApiModels.Shared;

namespace TeamServer.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[Controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            //create an instance of the UserStore and check if the user exists
            UserStore userStore = new UserStore();
            var user = await userStore.FindByNameAsync(request.Username, new CancellationToken());
            if (user != null)
            {
                string PasswordHash = "";
                if (request.PasswordHash is null)
                {
                    byte[] salt = await UserStore.GetUserPasswordSalt(request.Username);
                    PasswordHash = Utilities.Hash.HashPassword(request.Password, salt);
                }
                else
                {
                    PasswordHash = request.PasswordHash;
                }
                string token = await Authentication.SignIn(user, PasswordHash);
                //if the user exists check if the password hash matches the one in the store
                if (!string.IsNullOrEmpty(token))
                {
                    //sign was successful 
                    return Ok(token);
                }
                else
                {
                    //sign in failed
                    return Unauthorized("Login failed");
                }
            }
            else
            {
                //if the user does not exist return false
                return Unauthorized("User not found");
            }
        }

        //allow someone with Administrator permission to register a new user
        [HttpPost("Register")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegRequest request)
        {
            bool createdUser = await HardHatHub.CreateUserTS(request.Username, request.Password, request.Role);
            if (createdUser)
            {
                return Ok($"User {request.Username} created"); // or any other appropriate response
            }
            else
            {
                return BadRequest("User creation failed");
            }
        }

        //allow someone with Administrator permission to register a new user
        [HttpGet("{username}",Name="Checkreg")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CheckIfUserExists()
        {
            //check if the user exists
            UserStore userStore = new UserStore();
            var user = await userStore.FindByNameAsync("username", new CancellationToken());
            if(user != null)
            {
                return Ok("User exists");
            }
            else
            {
                return NotFound("User does not exist");
            }
        }

    }
}
