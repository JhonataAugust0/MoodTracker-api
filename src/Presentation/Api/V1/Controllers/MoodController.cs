using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Api.V1.Controllers
{
    [ApiController]
    [Route("api/moods")]
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
                return NotFound("Mood not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MoodDto>>> GetUserMoods()
        {
            try
            {
                var moods = await _moodService.GetUserMoodsAsync(_currentUserService.UserId);
                return Ok(moods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<HabitDto>>> GetUserMoodHistory(
            [FromQuery] int moodId,
            [FromQuery] DateTimeOffset? startDate,
            [FromQuery] DateTimeOffset? endDate)
        {
            try
            {
                var habits = await _moodService.GetUserMoodHistoryAsync(moodId, startDate, endDate);
                return Ok(habits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost, HttpOptions]
        public async Task<ActionResult<MoodDto>> CreateMood(CreateMoodDto createMoodDto)
        {
            try
            {
                var mood = await _moodService.CreateMoodAsync(_currentUserService.UserId, createMoodDto);
                return CreatedAtAction(nameof(GetMood), new { id = mood.Id }, mood);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
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
                return NotFound("Mood not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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
                return NotFound("Mood not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}