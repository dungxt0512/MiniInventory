using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MiniInventory.Data;
using MiniInventory.Helpers;
using MiniInventory.Model.DTOs.Auth;
using MiniInventory.Model.Entities;

namespace MiniInventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthControllers : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly JwtHelpers _jwtHelpers;
        private readonly PasswordHelpers _passwordHelpers;
        public AuthControllers(AppDbContext db, IConfiguration config, JwtHelpers jwtHelpers, PasswordHelpers passwordHelpers)
        {
            _db = db;
            _config = config;
            _jwtHelpers = jwtHelpers;
            _passwordHelpers = passwordHelpers;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Username va password khong duoc de trong" });
            }
            var user = _db.users.FirstOrDefault(u => u.UserName == request.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Sai username or password" });
            }
            var token = _jwtHelpers.generateToken(user.Id, _config["Jwt:Key"]!, _config["Jwt:Issuer"]!);
            return Ok(new UserLoginResponse
            {
                Token = token,
                UserName = user.UserName,
                Role = user.Role
            });
        }
    }
}
