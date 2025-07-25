using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Services.Implement;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu xác thực JWT cho tất cả các action
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _service;

        public ArticlesController(IArticleService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        [AllowAnonymous] // Cho phép truy cập không cần token (ví dụ để xem bài viết công khai)
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var article = await _service.GetByIdAsync(id);

            return article == null ? NotFound() : Ok(article);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin hoặc Staff mới được tạo bài viết
        public async Task<IActionResult> Create([FromBody] ArticleDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ArticleID }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var articles = await _service.SearchAsync(keyword);
            return Ok(articles);
        }

        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var articles = await _service.GetByCategoryAsync(categoryId);
            return Ok(articles);
        }

        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
        {
            var articles = await _service.GetRecentAsync(count);
            return Ok(articles);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var validStatuses = new[] { "Draft", "Published", "Archived", "Pending" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest("Invalid status value. Allowed values are: " + string.Join(", ", validStatuses));
            }

            var updated = await _service.UpdateStatusAsync(id, status);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCount()
        {
            var count = await _service.GetCountAsync();
            return Ok(new { Count = count });
        }
        [HttpGet("{id}/like-count")]
        public async Task<IActionResult> GetLikeCount(int id)
        {
            var count = await _service.GetLikeCountAsync(id);
            return Ok(count);
        }

        [HttpGet("{id}/is-liked/{userId}")]
        public async Task<IActionResult> IsLiked(int id, string userId)
        {
            var liked = await _service.IsLikedAsync(id, userId);
            return Ok(liked);
        }

        [HttpPost("{id}/like/{userId}")]
        public async Task<IActionResult> Like(int id, string userId)
        {
            await _service.LikeAsync(id, userId);
            return NoContent();
        }

        [HttpPost("{id}/unlike/{userId}")]
        public async Task<IActionResult> Unlike(int id, string userId)
        {
            await _service.UnlikeAsync(id, userId);
            return NoContent();
        }

    }
}
