using BlogAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Controllers
{
    [Route("api/Blog/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("id").Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email
            });
        }
    }
}
