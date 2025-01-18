using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Infrastructure.Exceptions;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Controllers
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
        public async Task<ActionResult<MoodDto>> GetMood(int id)
        {
            try
            {
                var mood = await _tagService.GetTagByIdAsync(id, _currentUserService.UserId);
                return Ok(mood);
            }
            catch (NotFoundException)
            {
                return NotFound("Tag not found");
            }
        }

        [HttpPost]
        public async Task<ActionResult<MoodDto>> CreateTag(CreateTagDto createTagDto)
        {
            var tag = await _tagService.CreateTagAsync(_currentUserService.UserId, createTagDto);
            return CreatedAtAction(nameof(GetMood), new { id = tag.Id }, tag);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MoodDto>> UpdateMood(int id, UpdateTagDto updateTagDto)
        {
            try
            {
                var mood = await _tagService.UpdateTagAsync(id, _currentUserService.UserId, updateTagDto);
                return Ok(mood);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMood(int id)
        {
            try
            {
                await _tagService.DeleteTagAsync(id, _currentUserService.UserId);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}