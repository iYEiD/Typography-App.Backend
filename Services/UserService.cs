using Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Data;

namespace Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserFromTokenAsync(string token)
        {
           var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
           var jsonToken = handler.ReadToken(token) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
            var email = jsonToken?.Claims.First(claim => claim.Type == "unique_name").Value;

            // Use EF Core to get user information
            var user = await GetUserByEmailAsync(email);

            return user;
        }

        private async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.email == email)
                .Select(u => new User
                {
                    id = u.id,
                    name = u.name,
                    email = u.email,
                    password = u.password
                })
                .FirstOrDefaultAsync();
        }
    }

}