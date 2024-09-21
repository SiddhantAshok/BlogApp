using BlogAPI.Data;
using BlogAPI.Models;
using BlogAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers
{
    [Route("api/Blog/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArticleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/Blog/articles
        [HttpGet]
        public async Task<IActionResult> GetArticles()
        {
            var articles = await _context.Articles.Include(a => a.User).ToListAsync();
            return Ok(articles);
        }

        // POST: /api/Blog/articles
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateArticle(ArticleDto articleDto)
        {
            var userId = int.Parse(User.FindFirst("id").Value);
            var article = new Article
            {
                Title = articleDto.Title,
                Content = articleDto.Content,
                UserId = userId
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return Ok("Article created");
        }

        // PUT: /api/Blog/articles/{id} 
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, ArticleDto articleDto)
        {
            var userId = int.Parse(User.FindFirst("id").Value);
            var article = await _context.Articles.FindAsync(id);
            if (article == null || article.UserId != userId) return Unauthorized();

            article.Title = articleDto.Title;
            article.Content = articleDto.Content;

            _context.Articles.Update(article);
            await _context.SaveChangesAsync();

            return Ok("Article updated");
        }

        // DELETE: /api/Blog/articles/{id} 
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var userId = int.Parse(User.FindFirst("id").Value);
            var article = await _context.Articles.FindAsync(id);
            if (article == null || article.UserId != userId) return Unauthorized();

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return Ok("Article deleted");
        }
    }
}
