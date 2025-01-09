
using BookStore.Helper;
using Infrastructure;
using Infrastructure.Entities.Identity;
using Infrastructure.Enums;
using Infrastructure.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace BookStore.Services
{
    public interface IIdentityService
    {
        public Task<string> Login(LoginUser login);
        public Task<IdentityResult> Register(RegisterUser login);
        public Task<IdentityUser> GetUserByLogin(string login);
    }
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtConfiguration _config;
        public IdentityService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<JwtConfiguration> config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config.Value;
        }
        public async Task<string> Login(LoginUser login)
        {
            var user = await _userManager.FindByNameAsync(login.Login);
            if (user != null)
            {
                if(await _userManager.CheckPasswordAsync(user, login.Password))
                {
                    return await GenerateTokenString(user, _config);
                }
            }
            throw new AccessViolationException("Account with given login doesn't exist.");
        }
        public async Task<IdentityResult> Register(RegisterUser registration)
        {
            var user = new IdentityUser
            {
                UserName = registration.Login,
                Email = registration.Email
            };

            var result = await _userManager.CreateAsync(user, registration.Password);

            if (result.Succeeded)
            {
                try
                {
                    await _userManager.AddToRoleAsync(user, Roles.User.ToString());
                }
                catch
                {
                    await SeedRoles.seed(_roleManager);
                    await _userManager.AddToRoleAsync(user, Roles.User.ToString());
                }
            }

            return result;
        }
        public async Task<IdentityUser> GetUserByLogin(string login)
        {
            var user = await _userManager.FindByNameAsync(login);
            if (user != null)
            {
                return user;
            }
            throw new NotFoundException("Account with given login doesn't exist.");
        }
        private async Task<List<Claim>> GetClaims(IdentityUser user)
        {

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
            };

            claims.AddRange(GetClaimsSeperated(await _userManager.GetClaimsAsync(user)));
            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                var identityRole = await _roleManager.FindByNameAsync(role);
                claims.AddRange(GetClaimsSeperated(await _roleManager.GetClaimsAsync(identityRole)));
            }

            return claims;
        }
        private List<Claim> GetClaimsSeperated(IList<Claim> claims)
        {
            var result = new List<Claim>();
            foreach (var claim in claims)
            {
                result.AddRange(claim.DeserializePermissions().Select(t => new Claim(claim.Type, t.ToString())));
            }
            return result;
        }

        private async Task<string> GenerateTokenString(IdentityUser user, JwtConfiguration jwtConfig)
        {
            var claims = await GetClaims(user);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));

            var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                issuer: jwtConfig.Issuer,
                audience: jwtConfig.Audience,
                signingCredentials: signingCred);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
            return tokenString;
        }
    }
}
