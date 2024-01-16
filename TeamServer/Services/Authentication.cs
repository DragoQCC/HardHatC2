using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HardHatCore.TeamServer.Services
{
    public class Authentication
    {
        public static SignInManager<UserInfo> SignInManager { get; set; } 
        public static UserManager<UserInfo> UserManager { get; set; } 
        public static IConfiguration Configuration { get; set; }


        public Authentication(SignInManager<UserInfo> signInManager, UserManager<UserInfo> userManager, IConfiguration configuration)
        {
            SignInManager = signInManager;
            UserManager = userManager;
            Configuration = configuration;
        }

        public static async Task<string> SignIn(UserInfo user, string ProvidedPasswordHaash)
        {
            try
            {
                SignInResult signInResult = await SignInManager.PasswordSignInAsync(user.UserName, ProvidedPasswordHaash, false, false);
                if (signInResult.Succeeded)
                {

                    UserInfo SignedInuser = await UserManager.FindByNameAsync(user.NormalizedUserName);
                    string JSONWebTokenString = await GenerateJSONWebToken(SignedInuser);
                    Console.WriteLine($"{SignedInuser.UserName} logged in successfully");
                    return JSONWebTokenString;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return  null;
            }
            
        }

        private static async Task<string> GenerateJSONWebToken(UserInfo user)
        {
            SymmetricSecurityKey SymmetricSecurityKey = new(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            SigningCredentials signingCreds = new(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new()
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            IList<string> roleNames = await UserManager.GetRolesAsync(user);
            foreach (string roleName in roleNames)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, roleName));
            }
            
            JwtSecurityToken jwtsecToken = new(Configuration["Jwt:Issuer"],Configuration["Jwt:Issuer"], claims,null,DateTime.Now.AddDays(28),signingCreds);
            return new JwtSecurityTokenHandler().WriteToken(jwtsecToken);

        }

    }
}
