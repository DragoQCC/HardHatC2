using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using TeamServer.Models.Database;
using TeamServer.Services.Extra;
using TeamServer.Services;
using System.Threading.Tasks;

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
            var user = new UserInfo { UserName = request.Username, PasswordHash = request.PasswordHash, NormalizedUserName = request.Username.Normalize().ToUpper() };
            Console.WriteLine($"{request.Username}'s logging in hashed password is {request.PasswordHash}");
            if (user != null)
            {
                string token = await Authentication.SignIn(user);
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
    }
}
