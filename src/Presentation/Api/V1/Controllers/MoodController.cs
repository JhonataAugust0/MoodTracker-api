using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Infrastructure.Exceptions;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MoodController : ControllerBase
    {
        private readonly IMoodService _moodService;
        private readonly ICurrentUserService _currentUserService;

        public MoodController(IMoodService moodService, ICurrentUserService currentUserService)
        {
            _moodService = moodService;
            _currentUserService = currentUserService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MoodDto>> GetMood(int id)
        {
            try
            {
                var mood = await _moodService.GetByIdAsync(id, _currentUserService.UserId);
                return Ok(mood);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MoodDto>>> GetMoodHistory(
            [FromQuery] DateTimeOffset? startDate,
            [FromQuery] DateTimeOffset? endDate)
        {
            var moods = await _moodService.GetUserMoodHistoryAsync(
                _currentUserService.UserId,
                startDate,
                endDate);
            return Ok(moods);
        }

        [HttpPost]
        public async Task<ActionResult<MoodDto>> CreateMood(CreateMoodDto createMoodDto)
        {
            var mood = await _moodService.CreateMoodAsync(_currentUserService.UserId, createMoodDto);
            return CreatedAtAction(nameof(GetMood), new { id = mood.Id }, mood);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MoodDto>> UpdateMood(int id, UpdateMoodDto updateMoodDto)
        {
            try
            {
                var mood = await _moodService.UpdateMoodAsync(id, _currentUserService.UserId, updateMoodDto);
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
                await _moodService.DeleteMoodAsync(id, _currentUserService.UserId);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}