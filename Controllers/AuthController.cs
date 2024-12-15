using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetEnv;
using System.Security.Cryptography;
using System.Linq;
using Models;
using Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var user = _context.Users.SingleOrDefault(u => u.email == model.Email);
        if (user != null && VerifyPassword(model.Password, user.password))
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecretKey = Env.GetString("JWT_SECRET_KEY");
            var key = Encoding.ASCII.GetBytes(jwtSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, model.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }

        return Unauthorized();
    }

    [HttpPost("signup")]
    public IActionResult Signup([FromBody] SignupModel model)
    {
        if (_context.Users.Any(u => u.email == model.Email))
        {
            return BadRequest("Email already in use.");
        }

        var user = new User
        {
            name = model.Username,
            email = model.Email,
            password = HashPassword(model.Password)
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("User registered successfully.");
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    private bool VerifyPassword(string enteredPassword, string storedHash)
    {
        var enteredHash = HashPassword(enteredPassword);
        return enteredHash == storedHash;
    }

    [HttpGet("signin-google")]
    public IActionResult SignInGoogle()
    {
        var redirectUrl = Url.Action("GoogleResponse", "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return BadRequest(); // Handle failure

        var claims = result.Principal.Identities
            .FirstOrDefault()?.Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

        var emailClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (emailClaim == null)
            return BadRequest("Email claim not found");

        var user = _context.Users.SingleOrDefault(u => u.email == emailClaim);
        if (user == null)
        {
            var nameClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "DefaultName";
            var randomPassword = HashPassword(Guid.NewGuid().ToString().Substring(0, 10));
            user = new User { email = emailClaim, name = nameClaim, password = randomPassword };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecretKey = Env.GetString("JWT_SECRET_KEY");
        var key = Encoding.ASCII.GetBytes(jwtSecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, emailClaim)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        var frontendUrl = "http://localhost:3000/callback?token=" + jwtToken;
        return Redirect(frontendUrl);
    }
}

public class LoginModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class SignupModel
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
