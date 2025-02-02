using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;


namespace MoodTracker_back.Application.Services
{
    public class MoodAppService : IMoodService
    {
        private readonly IMoodRepository _moodRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserService _userService;
        private readonly ILoggingService _logger;

        public MoodAppService(
            IMoodRepository moodRepository, 
            ITagRepository tagRepository, 
            IUserService userService,
            ILoggingService logger)
        {
            _moodRepository = moodRepository ?? throw new ArgumentNullException(nameof(moodRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<MoodDto> GetByIdAsync(int id, int userId)
        {
            try
            {
                var mood = await _moodRepository.GetByIdAsync(id);
                if (mood == null || mood.UserId != userId)
                    throw new NotFoundException("Mood not found");

                return MapToDto(mood);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar o mood com ID {MoodId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao buscar o mood. Tente novamente mais tarde.", ex);
            }
        }
        
        public async Task<IEnumerable<MoodDto>> GetUserMoodsAsync(int userId)
        {
            try
            {
                var moods = await _moodRepository.GetUserMoodsAsync(userId);
                return moods.Select(MapToDto);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar os humores para o usuário {UserId}", userId);
                throw new ApplicationException("Erro ao buscar os humores. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<IEnumerable<MoodDto>> GetUserMoodHistoryAsync(int moodId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new ArgumentException("A data inicial não pode ser posterior à data final.");

            try
            {
                var moods = await _moodRepository.GetUserHistoryMoodAsync(moodId, startDate, endDate);
                return moods.Select(MapToDto);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar o histórico do mood com ID {MoodId}", moodId);
                throw new ApplicationException("Erro ao buscar o histórico do mood. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<MoodDto> CreateMoodAsync(int userId, CreateMoodDto createMoodDto)
        {
            if (createMoodDto == null)
                throw new ArgumentNullException(nameof(createMoodDto));

            try
            {
                var mood = new Mood()
                {
                    UserId = userId,
                    MoodType = createMoodDto.MoodType,
                    Intensity = createMoodDto.Intensity,
                    Notes = createMoodDto.Notes,
                    Timestamp = createMoodDto.Timestamp ?? DateTimeOffset.UtcNow,
                };

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException("User not found");
                }
                user.LastMoodEntry = createMoodDto.Timestamp ?? DateTimeOffset.UtcNow;
                user.LastLogin = createMoodDto.Timestamp ?? DateTimeOffset.UtcNow;
                user.LastNotified = createMoodDto.Timestamp ?? DateTimeOffset.UtcNow;
                await _userService.UpdateUserAsync(user);

                if (createMoodDto.TagIds != null && createMoodDto.TagIds.Any())
                {
                    var tags = await _tagRepository.GetByIdsAsync(createMoodDto.TagIds);
                    foreach (var tag in tags.Where(t => t.UserId == userId))
                    {
                        mood.Tags.Add(tag);
                    }
                }

                await _moodRepository.CreateAsync(mood);
                return MapToDto(mood);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar o mood para o usuário {UserId}", userId);
                throw new ApplicationException("Erro ao criar o mood. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<MoodDto> UpdateMoodAsync(int id, int userId, UpdateMoodDto updateMoodDto)
        {
            if (updateMoodDto == null)
                throw new ArgumentNullException(nameof(updateMoodDto));

            try
            {
                var mood = await _moodRepository.GetByIdAsync(id);
                if (mood == null || mood.UserId != userId)
                    throw new NotFoundException("Mood not found");

                if (!string.IsNullOrWhiteSpace(updateMoodDto.MoodType))
                    mood.MoodType = updateMoodDto.MoodType;
                
                if (updateMoodDto.Intensity.HasValue)
                    mood.Intensity = updateMoodDto.Intensity.Value;
                
                if (updateMoodDto.Notes != null)
                    mood.Notes = updateMoodDto.Notes;

                if (updateMoodDto.TagIds != null)
                {
                    mood.Tags.Clear();
                    var tags = await _tagRepository.GetByIdsAsync(updateMoodDto.TagIds);
                    foreach (var tag in tags.Where(t => t.UserId == userId))
                    {
                        mood.Tags.Add(tag);
                    }
                }

                mood.UpdatedAt = DateTimeOffset.UtcNow;
                await _moodRepository.UpdateAsync(mood);
                return MapToDto(mood);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar o mood com ID {MoodId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao atualizar o mood. Tente novamente mais tarde.", ex);
            }
        }

        public async Task DeleteMoodAsync(int id, int userId)
        {
            try
            {
                var mood = await _moodRepository.GetByIdAsync(id);
                if (mood == null || mood.UserId != userId)
                    throw new NotFoundException("Mood not found");

                await _moodRepository.DeleteAsync(id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar o mood com ID {MoodId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao deletar o mood. Tente novamente mais tarde.", ex);
            }
        }

        private static MoodDto MapToDto(Mood moodBase)
        {
            return new MoodDto
            {
                Id = moodBase.Id,
                MoodType = moodBase.MoodType,
                Intensity = moodBase.Intensity,
                Notes = moodBase.Notes,
                Timestamp = moodBase.Timestamp,
                TagIds = moodBase.Tags.Select(t => t.Id).ToList()
            };
        }
    }
}
