using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;


namespace MoodTracker_back.Application.Services
{
    public class HabitAppService : IHabitService
    {
        private readonly IHabitRepository _habitRepository;
        private readonly IHabitCompletionRepository _habitCompletionRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ICryptographService _cryptographService;
        private readonly ILoggingService _logger;

        public HabitAppService(
            IHabitRepository habitRepository,
            ITagRepository tagRepository,
            IHabitCompletionRepository habitCompletionRepository,
            ICryptographService cryptographService,
            ILoggingService logger)
        {
            _habitRepository = habitRepository ?? throw new ArgumentNullException(nameof(habitRepository));
            _habitCompletionRepository = habitCompletionRepository ??
                                         throw new ArgumentNullException(nameof(habitCompletionRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _cryptographService = cryptographService ?? throw new ArgumentNullException(nameof(cryptographService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HabitDto> GetByIdAsync(int id, int userId)
        {
            try
            {
                var habit = await _habitRepository.GetByIdAsync(id);
                if (habit == null || habit.UserId != userId)
                    throw new NotFoundException("Hábito não encontrado");

                return MapToDto(habit);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao recuperar o hábito com ID {HabitId} para o usuário {UserId}",
                    id, userId);
                throw new ApplicationException("Erro ao recuperar o hábito. Por favor, tente novamente mais tarde.",
                    ex);
            }
        }

        public async Task<IEnumerable<HabitDto>> GetUserHabitsAsync(int userId)
        {
            try
            {
                var habits = await _habitRepository.GetUserHabitsAsync(userId);
                return habits.Select(MapToDto);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao recuperar hábitos do usuário de ID {UserId}", userId);
                throw new ApplicationException("Erro ao recuperar hábitos. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<IEnumerable<HabitCompletion>> GetUserHistoryHabitCompletionAsync(
            int habitId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new ArgumentException("A data de início não pode ser posterior à data de término.");

            try
            {
                var history =
                    await _habitCompletionRepository.GetUserHistoryHabitCompletionAsync(habitId, startDate, endDate);
                return history;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao recuperar o histórico de conclusão do hábito de ID {HabitId}",
                    habitId);
                throw new ApplicationException(
                    "Erro ao recuperar o histórico de conclusão do hábito. Por favor, tente novamente mais tarde.", ex);
            }
        }

        public async Task<HabitDto> CreateHabitAsync(int userId, CreateHabitDto createHabitDto)
        {
            if (createHabitDto == null)
                throw new ArgumentNullException(nameof(createHabitDto));
            if (string.IsNullOrWhiteSpace(createHabitDto.Name))
                throw new ArgumentException("Nome do hábito é obrigatório", nameof(createHabitDto.Name));

            try
            {
                var habit = new Habit
                {
                    UserId = userId,
                    Name = _cryptographService.Encrypt(createHabitDto.Name),
                    Description = _cryptographService.Encrypt(createHabitDto.Description ?? ""),
                    CreatedAt = createHabitDto.CreatedAt ?? DateTimeOffset.UtcNow,
                    UpdatedAt = createHabitDto.CreatedAt ?? DateTimeOffset.UtcNow,
                    IsActive = createHabitDto.IsActive ?? true,
                    FrequencyType = createHabitDto.FrequencyType,
                    FrequencyTarget = createHabitDto.FrequencyTarget,
                    Color = createHabitDto.Color
                };

                if (createHabitDto.TagIds != null && createHabitDto.TagIds.Any())
                {
                    var tags = await _tagRepository.GetByIdsAsync(createHabitDto.TagIds);
                    foreach (var tag in tags.Where(t => t.UserId == userId))
                    {
                        habit.Tags.Add(tag);
                    }
                }

                await _habitRepository.CreateAsync(habit);
                return MapToDto(habit);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar hábito para o usuário {UserId}", userId);
                throw new ApplicationException("Erro ao criar hábito. Por favor, tente novamente mais tarde.", ex);
            }
        }

        public async Task<HabitCompletionDto> LogHabitAsync(int userId, LogHabitCompletionDto habitCompletionDto)
        {
            if (habitCompletionDto == null)
                throw new ArgumentNullException(nameof(habitCompletionDto));

            try
            {
                var habit = await _habitRepository.GetByIdAsync(habitCompletionDto.HabitId);
                if (habit == null || habit.UserId != userId)
                    throw new NotFoundException("Hábito não encontrado ou usuário não autorizado.");

                var habitCompletion = new HabitCompletion
                {
                    HabitId = habit.Id,
                    CompletedAt = habitCompletionDto.CompletedAt,
                    Notes = _cryptographService.Encrypt(habitCompletionDto.Notes),
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _habitCompletionRepository.CreateAsync(habitCompletion);

                return new HabitCompletionDto
                {
                    Id = habitCompletion.Id,
                    HabitId = habitCompletion.HabitId,
                    CompletedAt = habitCompletion.CompletedAt,
                    Notes = _cryptographService.Decrypt(habitCompletion.Notes),
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex,
                    "Erro ao registrar a conclusão do hábito de ID {HabitId} do usuário de ID {UserId}",
                    habitCompletionDto.HabitId, userId);
                throw new ApplicationException(
                    "Erro ao registrar a conclusão do hábito. Por favor, tente novamente mais tarde.", ex);
            }
        }

        public async Task<HabitDto> UpdateHabitAsync(int id, int userId, UpdateHabitDto updateHabitDto)
        {
            if (updateHabitDto == null)
                throw new ArgumentNullException(nameof(updateHabitDto));

            try
            {
                var habit = await _habitRepository.GetByIdAsync(id);
                if (habit == null || habit.UserId != userId)
                    throw new NotFoundException("Habit not found");

                if (!string.IsNullOrWhiteSpace(updateHabitDto.Name))
                    habit.Name = _cryptographService.Encrypt(updateHabitDto.Name);

                if (!string.IsNullOrWhiteSpace(updateHabitDto.Description))
                    habit.Description = _cryptographService.Encrypt(updateHabitDto.Description);

                if (updateHabitDto.IsActive.HasValue)
                    habit.IsActive = updateHabitDto.IsActive.Value;

                if (updateHabitDto.TagIds != null)
                {
                    habit.Tags.Clear();
                    var tags = await _tagRepository.GetByIdsAsync(updateHabitDto.TagIds);
                    foreach (var tag in tags.Where(t => t.UserId == userId))
                    {
                        habit.Tags.Add(tag);
                    }
                }

                habit.UpdatedAt = DateTimeOffset.UtcNow;
                await _habitRepository.UpdateAsync(habit);
                return MapToDto(habit);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar hábito de ID {HabitId} do usuário de ID {UserId}",
                    id, userId);
                throw new ApplicationException("Erro ao atualizar hábito. Tente novamente mais tarde.", ex);
            }
        }

        public async Task DeleteHabitAsync(int id, int userId)
        {
            try
            {
                var habit = await _habitRepository.GetByIdAsync(id);
                if (habit == null || habit.UserId != userId)
                    throw new NotFoundException("Hábito não encontrado");

                await _habitRepository.DeleteAsync(id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar hábito de ID {HabitId} do usuário de ID {UserId}", id,
                    userId);
                throw new ApplicationException("Erro ao deletar hábito. Tente novamente mais tarde.", ex);
            }
        }

        private HabitDto MapToDto(Habit habit)
        {
            return new HabitDto
            {
                Id = habit.Id,
                UserId = habit.UserId,
                Name = _cryptographService.Decrypt(habit.Name),
                Description = _cryptographService.Decrypt(habit.Description ?? ""),
                CreatedAt = habit.CreatedAt,
                UpdatedAt = habit.UpdatedAt,
                IsActive = habit.IsActive,
                FrequencyType = habit.FrequencyType,
                FrequencyTarget = habit.FrequencyTarget,
                Color = habit.Color,
                Tags = habit.Tags.Select(tag => new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name
                }).ToList()
            };
        }
    }
}
