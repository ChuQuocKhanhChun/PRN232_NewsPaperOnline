using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject_Client.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PRN232_FinalProject_Client.DTO;

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
