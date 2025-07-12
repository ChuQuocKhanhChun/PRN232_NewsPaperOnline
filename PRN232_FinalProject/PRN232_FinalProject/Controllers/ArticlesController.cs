using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _service;

        public ArticlesController(IArticleService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var article = await _service.GetByIdAsync(id);
            return article == null ? NotFound() : Ok(article);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ArticleDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ArticleID }, created);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var articles = await _service.SearchAsync(keyword);
            return Ok(articles);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var articles = await _service.GetByCategoryAsync(categoryId);
            return Ok(articles);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
        {
            var articles = await _service.GetRecentAsync(count);
            return Ok(articles);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            // Kiểm tra giá trị status hợp lệ
            var validStatuses = new[] { "Draft", "Published", "Archived", "Pending" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest("Invalid status value. Allowed values are: " + string.Join(", ", validStatuses));
            }

            var updated = await _service.UpdateStatusAsync(id, status);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var count = await _service.GetCountAsync();
            return Ok(new { Count = count });
        }
    }

}
