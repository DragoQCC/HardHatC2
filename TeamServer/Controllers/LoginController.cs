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
                byte[] salt = await UserStore.GetUserPasswordSalt(request.Username);
                string PasswordHash = Utilities.Hash.HashPassword(request.Password, salt);
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
        public async Task<IActionResult> RegisterAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Now, parse the body to extract the values you need
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            var username = json["Username"];
            var password = json["Password"];
            var role = json["Role"];

            bool createdUser =  await HardHatHub.CreateUserTS(username, password, role);
            if (createdUser)
            {
                return Ok($"User {username} created"); // or any other appropriate response
            }
            else
            {
                return BadRequest("User creation failed");
            }
        }

    }
}
