using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Presentation.Api.V1.Controllers
{
    [ApiController]
    [Route("api/habits")]
    [Authorize]
    public class HabitController : ControllerBase
    {
        private readonly IHabitService _habitService;
        private readonly ICurrentUserService _currentUserService;

        public HabitController(IHabitService habitService, ICurrentUserService currentUserService)
        {
            _habitService = habitService;
            _currentUserService = currentUserService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HabitDto>> GetHabit(int id)
        {
            try
            {
                var habit = await _habitService.GetByIdAsync(id, _currentUserService.UserId);
                return Ok(habit);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabitDto>>> GetUserHabits()
        {
            var habits = await _habitService.GetUserHabitsAsync(_currentUserService.UserId);
            return Ok(habits);
        }
        
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<HabitDto>>> GetUserHistoryHabitCompletion([FromQuery] int habitId,
            [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
        {
            var habits = await _habitService.GetUserHistoryHabitCompletionAsync(habitId, startDate, endDate);
            return Ok(habits);
        }

        [HttpPost("create")]
        public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto)
        {
            var habit = await _habitService.CreateHabitAsync(_currentUserService.UserId, createHabitDto);
            return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, habit);
        }

        [HttpPost("log")]
        public async Task<ActionResult<LogHabitCompletionDto>> Log(LogHabitCompletionDto logHabitDto)
        {
            var habit = await _habitService.LogHabitAsync(_currentUserService.UserId, logHabitDto);
            return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, habit);
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<HabitDto>> UpdateHabit(int id, UpdateHabitDto updateHabitDto)
        {
            try
            {
                var habit = await _habitService.UpdateHabitAsync(id, _currentUserService.UserId, updateHabitDto);
                return Ok(habit);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteHabit(int id)
        {
            try
            {
                await _habitService.DeleteHabitAsync(id, _currentUserService.UserId);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}