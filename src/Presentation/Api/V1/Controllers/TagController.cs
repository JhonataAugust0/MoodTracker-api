using MoodTracker_back.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Presentation.Api.V1.Dtos;


namespace MoodTracker_back.Presentation.Api.V1.Controllers
{
    [ApiController]
    [Route("api/tags")]
    [Authorize]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ICurrentUserService _currentUserService;

        public TagController(ITagService tagService, ICurrentUserService currentUserService)
        {
            _tagService = tagService;
            _currentUserService = currentUserService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id, _currentUserService.UserId);
                return Ok(tag);
            }
            catch (NotFoundException)
            {
                return NotFound(new { error = "Tag not found" });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetUserTags()
        {
            try
            {
                var tags = await _tagService.GetUserTagsAsync(_currentUserService.UserId);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching tags.", details = ex.Message });
            }
        }

        [HttpPost,HttpOptions]
        public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto)
        {
            try
            {
                var tag = await _tagService.CreateTagAsync(_currentUserService.UserId, createTagDto);
                return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TagDto>> UpdateTag(int id, UpdateTagDto updateTagDto)
        {
            try
            {
                var tag = await _tagService.UpdateTagAsync(id, _currentUserService.UserId, updateTagDto);
                return Ok(tag);
            }
            catch (NotFoundException)
            {
                return NotFound(new { error = "Tag not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTag(int id)
        {
            try
            {
                await _tagService.DeleteTagAsync(id, _currentUserService.UserId);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound(new { error = "Tag not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the tag.", details = ex.Message });
            }
        }
    }
}