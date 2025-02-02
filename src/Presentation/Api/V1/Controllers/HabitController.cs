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
                return NotFound("Habit not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabitDto>>> GetUserHabits()
        {
            try
            {
                var habits = await _habitService.GetUserHabitsAsync(_currentUserService.UserId);
                return Ok(habits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<HabitDto>>> GetUserHistoryHabitCompletion(
            [FromQuery] int habitId, [FromQuery] DateTimeOffset? startDate, 
            [FromQuery] DateTimeOffset? endDate
        )
        {
            try
            {
                var habits = await _habitService.GetUserHistoryHabitCompletionAsync(habitId, startDate, endDate);
                return Ok(habits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto)
        {
            try
            {
                var habit = await _habitService.CreateHabitAsync(_currentUserService.UserId, createHabitDto);
                return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, habit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
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
                return NotFound("Habit not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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
                return NotFound("Habit not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}