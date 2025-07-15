using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject_Client.Models;
using PRN232_FinalProject_Client.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PRN232_FinalProject_Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ArticleService _articleService;
        public HomeController(ArticleService articleService)
        {
            _articleService = articleService;
        }

        public async Task<IActionResult> Index(string search, int category, int tag, DateTime? publishDate)
        {
            var articles = await _articleService.GetFilteredArticlesAsync(search, category, tag, publishDate);

            var tags = await _articleService.GetAllTagsAsync();
            var categories = await _articleService.GetAllCategoriesAsync();

            ViewBag.Tags = tags;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentTag = tag;
            ViewBag.CurrentPublishDate = publishDate?.ToString("yyyy-MM-dd");
            ViewBag.Categories = categories;

            return View(articles);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}