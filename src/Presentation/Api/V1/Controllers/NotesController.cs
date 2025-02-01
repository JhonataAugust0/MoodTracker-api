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
                var tag = await _notesService.GetNoteByIdAsync(id, _currentUserService.UserId);
                return Ok(tag);
            }
            catch (NotFoundException)
            {
                return NotFound("Note not found");
            }
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuickNoteDto>>> GetUserNotes()
        {
            var tags = await _notesService.GetUserNotesAsync(_currentUserService.UserId);
            return Ok(tags);
        }

        [HttpPost]
        public async Task<ActionResult<QuickNoteDto>> CreateNote(CreateQuickNoteDto createQuickNoteDto)
        {
            var tag = await _notesService.CreateNoteAsync(_currentUserService.UserId, createQuickNoteDto);
            return CreatedAtAction(nameof(GetNote), new { id = tag.Id }, tag);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<QuickNoteDto>> UpdateNote(int id, UpdateQuickNoteDto updateQuickNoteDto)
        {
            try
            {
                var tag = await _notesService.UpdateNoteAsync(id, _currentUserService.UserId, updateQuickNoteDto);
                return Ok(tag);
            }
            catch (NotFoundException)
            {
                return NotFound();
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
                return NotFound();
            }
        }
    }
}