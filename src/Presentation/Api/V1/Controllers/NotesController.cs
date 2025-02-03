using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Presentation.Api.V1.Dtos;


namespace MoodTracker_back.Presentation.Api.V1.Controllers
{
    [ApiController]
    [Route("api/notes")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly IQuickNotesService _notesService;
        private readonly ICurrentUserService _currentUserService;

        public NotesController(IQuickNotesService notesService, ICurrentUserService currentUserService)
        {
            _notesService = notesService;
            _currentUserService = currentUserService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuickNoteDto>> GetNote(int id)
        {
            try
            {
                var note = await _notesService.GetNoteByIdAsync(id, _currentUserService.UserId);
                return Ok(note);
            }
            catch (NotFoundException)
            {
                return NotFound("Note not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuickNoteDto>>> GetUserNotes()
        {
            try
            {
                var notes = await _notesService.GetUserNotesAsync(_currentUserService.UserId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<QuickNoteDto>> CreateNote([FromBody] CreateQuickNoteDto createQuickNoteDto)
        {
            try
            {
                var note = await _notesService.CreateNoteAsync(_currentUserService.UserId, createQuickNoteDto);
                return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<QuickNoteDto>> UpdateNote(int id, [FromBody] UpdateQuickNoteDto updateQuickNoteDto)
        {
            try
            {
                var note = await _notesService.UpdateNoteAsync(id, _currentUserService.UserId, updateQuickNoteDto);
                return Ok(note);
            }
            catch (NotFoundException)
            {
                return NotFound("Note not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNote(int id)
        {
            try
            {
                await _notesService.DeleteNoteAsync(id, _currentUserService.UserId);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound("Note not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
