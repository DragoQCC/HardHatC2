using HardHatCore.HardHatC2Client.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;

namespace HardHatCore.HardHatC2Client.Services
{
    public class MyAuthenticationStateProviderService : AuthenticationStateProvider
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        //public string jwtToken { get; set;}
        private readonly ILocalStorageService _localStorageService;
        internal string LocalStorageTokenName = "bearerToken";
        
        public MyAuthenticationStateProviderService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                string jwtToken = null;
                try
                {
                   jwtToken  = await _localStorageService.GetItemAsync<string>(LocalStorageTokenName);
                }
                catch (Exception)
                {}
                
               
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
