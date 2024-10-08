﻿using BlogAPI.Data;
using BlogAPI.Models;
using BlogAPI.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlogAPI.Controllers
{
    [Route("api/Blog/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto registerDto)
        {
            // Hash the password before storing it in the database
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = hashedPassword
            };

            // Save user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }


        //public async Task<IActionResult> Register(UserDto userDto)
        //{
        //    //throw new NotImplementedException();
        //    var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
        //    if (existingUser != null) return BadRequest("User already exists");

        //    var user = new User
        //    {
        //        Username = userDto.Username,
        //        Email = userDto.Email,
        //        PasswordHash = HashPassword(userDto.Password)
        //    };

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();
        //    return Ok("User registered successfully");
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            // Find the user by username or email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Verify the provided password against the stored hashed password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate JWT token (or handle session as per your logic)
            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }


        //public async Task<IActionResult> Login(LoginDto loginDto)
        //{
        //    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
        //    if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash)) return Unauthorized("Invalid credentials");

        //    var token = GenerateJwtToken(user);
        //    return Ok(new { Token = token });
        //}

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim("id", user.Id.ToString())
        };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            using var hmac = new HMACSHA512();
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return storedHash == Convert.ToBase64String(computedHash);
        }

    }
}
