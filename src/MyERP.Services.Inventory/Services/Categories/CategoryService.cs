using MyERP.Services.Inventory.DTOs.Categories;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Categories;

namespace MyERP.Services.Inventory.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
        {
            // Check if code already exists
            var exists = await _categoryRepository.CodeExistsAsync(dto.CategoryCode);
            if (exists)
                throw new ConflictException($"Category with code '{dto.CategoryCode}' already exists");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                CategoryName = dto.CategoryName,
                CategoryCode = dto.CategoryCode.ToUpper(),
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);

            return MapToDto(category);
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            return categories.Select(MapToDto).ToList();
        }

        public async Task<CategoryResponseDto> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdWithRelationsAsync(id);
            if (category == null)
                throw new NotFoundException("Category", id);

            return MapToDto(category);
        }

        public async Task<CategoryResponseDto> UpdateAsync(Guid id, CreateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category", id);

            // Check if new code conflicts with existing
            var codeExists = await _categoryRepository.CodeExistsAsync(dto.CategoryCode, id);
            if (codeExists)
                throw new ConflictException($"Category with code '{dto.CategoryCode}' already exists");

            category.CategoryName = dto.CategoryName;
            category.CategoryCode = dto.CategoryCode.ToUpper();
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);

            return MapToDto(category);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdWithRelationsAsync(id);
            if (category == null)
                throw new NotFoundException("Category", id);

            // Check if category is in use
            if ((category.Products?.Any() == true) || (category.RawMaterials?.Any() == true))
                throw new ConflictException("Cannot delete category. It is being used by products or raw materials");

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(category);

            return true;
        }

        public async Task<bool> RestoreAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category", id);

            if (category.IsActive)
                throw new BadRequestException("Category is already active");

            category.IsActive = true;
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(category);

            return true;
        }

        private static CategoryResponseDto MapToDto(Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                Description = category.Description,
                IsActive = category.IsActive,
                ProductCount = category.Products?.Count ?? 0,
                RawMaterialCount = category.RawMaterials?.Count ?? 0,
                CreatedAt = category.CreatedAt
            };
        }
    }
}
