using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject_Client.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PRN232_FinalProject_Client.DTO;
using System.Collections.Generic;
using System.Linq;

namespace PRN232_FinalProject_Client.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ArticleService _articleService;

        public ArticleController(ArticleService articleService)
        {
            _articleService = articleService;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetArticlesAsync();
            return View(articles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            // Get related articles by tag (excluding the current article)
            List<PRN232_FinalProject_Client.DTO.ArticleDto> relatedArticles = new();
            if (article.TagIds != null && article.TagIds.Any())
            {
                // For simplicity, get all articles and filter by tag overlap
                var allArticles = await _articleService.GetArticlesAsync();
                relatedArticles = allArticles
                    .Where(a => a.ArticleID != article.ArticleID && a.TagIds.Any(tagId => article.TagIds.Contains(tagId)))
                    .Take(5)
                    .ToList();
            }
            ViewBag.RelatedArticles = relatedArticles;

            // Get comments (replace with your actual comment service/repository)
            // Example: var comments = await _commentService.GetCommentsByArticleIdAsync(id);
            var comments = new List<string>(); // Placeholder, replace with real data
            ViewBag.Comments = comments;

            return View(article);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ArticleCreateDto dto)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");
            if (!ModelState.IsValid) return View(dto);
            var result = await _articleService.CreateArticleAsync(dto, token);
            if (result)
                return RedirectToAction("Index");
            ModelState.AddModelError("", "Tạo bài báo thất bại");
            return View(dto);
        }
    }
}
