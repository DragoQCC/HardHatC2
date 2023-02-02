using HardHatC2Client.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HardHatC2Client.Services
{
    public class MyAuthenticationStateProviderService : AuthenticationStateProvider
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        public string jwtToken { get; set;}

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                jwtToken = Login.jwtToken;
                if (jwtToken != null)
                {
                    JwtSecurityToken token = _tokenHandler.ReadJwtToken(jwtToken);

                    //check that the token isnt expired
                    if (token.ValidTo < DateTime.UtcNow)
                    {
                        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    }
                    
                    var claims = GetClaimsFromToken(token);
                    var identity = new ClaimsIdentity(claims, "jwt");
                    var user = new ClaimsPrincipal(identity);
                    Task<AuthenticationState> authstate = Task.FromResult(new AuthenticationState(user));
                    NotifyAuthenticationStateChanged(authstate);
                    return authstate.Result;
                }
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception ex)
            {
                //returns a blank state if there is an error or invalid login
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        private IList<Claim> GetClaimsFromToken(JwtSecurityToken token)
        {
            return token.Claims.ToList();
        }

    }
}
