﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Services.Interfaces;
using System.Security.Claims;

namespace PRN232_FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class MyArticlesController : ControllerBase
    {
        private readonly IArticleService _service;

        public MyArticlesController(IArticleService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyArticles()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            // Fix: Explicitly call the correct method and handle the return type  
            var articles = await _service.GetByAuthorEmailAsync(email) as IEnumerable<ArticleDto>;
            if (articles == null) return NotFound();

            return Ok(articles);
        }
    }

}
