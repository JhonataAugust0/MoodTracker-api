using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodTracker_back.Application.Services
{
    public class TagAppService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly ILoggingService _logger;

        public TagAppService(ITagRepository tagRepository, ILoggingService loggingService)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _logger = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }
        
        public async Task<TagDto> GetTagByIdAsync(int id, int userId)
        {
            try
            {
                var tag = await _tagRepository.GetByIdAsync(id);
                if (tag == null || tag.UserId != userId)
                {
                    throw new NotFoundException("Tag not found");
                }
                return MapToDto(tag);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar tag com ID {TagId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao buscar a tag. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<IEnumerable<Tag>> GetUserTagsAsync(int userId)
        {
            try
            {
                return await _tagRepository.GetUserTagsAsync(userId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar tags para o usuário {UserId}", userId);
                throw new ApplicationException("Erro ao buscar as tags. Tente novamente mais tarde.", ex);
            }
        }
        
        public async Task<TagDto> CreateTagAsync(int userId, CreateTagDto createTagDto)
        {
            if (createTagDto == null)
                throw new ArgumentNullException(nameof(createTagDto));
            if (string.IsNullOrWhiteSpace(createTagDto.Name))
                throw new ArgumentException("O nome da tag é obrigatório.", nameof(createTagDto.Name));
            
            try
            {
                var tag = new Tag()
                {
                    UserId = userId,
                    Name = createTagDto.Name,
                    Color = createTagDto.Color,
                    CreatedAt = createTagDto.Timestamp ?? DateTimeOffset.UtcNow
                };

                await _tagRepository.CreateAsync(tag);
                return MapToDto(tag);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar a tag para o usuário {UserId}", userId);
                throw new ApplicationException("Erro ao criar a tag. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<TagDto> UpdateTagAsync(int id, int userId, UpdateTagDto updateTagDto)
        {
            if (updateTagDto == null)
                throw new ArgumentNullException(nameof(updateTagDto));

            try
            {
                var tag = await _tagRepository.GetByIdAsync(id);
                if (tag == null || tag.UserId != userId)
                {
                    throw new NotFoundException("Tag not found");
                }

                if (!string.IsNullOrWhiteSpace(updateTagDto.Name))
                {
                    tag.Name = updateTagDto.Name;
                }

                if (!string.IsNullOrWhiteSpace(updateTagDto.Color))
                {
                    tag.Color = updateTagDto.Color;
                }

                await _tagRepository.UpdateAsync(tag);
                return MapToDto(tag);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar a tag com ID {TagId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao atualizar a tag. Tente novamente mais tarde.", ex);
            }
        }

        public async Task DeleteTagAsync(int id, int userId)
        {
            try
            {
                var tag = await _tagRepository.GetByIdAsync(id);
                if (tag == null || tag.UserId != userId)
                {   
                    throw new NotFoundException("Tag not found");
                }

                await _tagRepository.DeleteAsync(id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar a tag com ID {TagId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao deletar a tag. Tente novamente mais tarde.", ex);
            }
        }
        
        private static TagDto MapToDto(Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                UserId = tag.UserId,
                Name = tag.Name,
                Color = tag.Color,
                CreatedAt = tag.CreatedAt,
            };
        }
    }
}
