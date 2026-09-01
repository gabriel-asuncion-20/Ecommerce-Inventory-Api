using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceInventoryApi.Core.DTOs;
using EcommerceInventoryApi.Core.Entities;
using EcommerceInventoryApi.Core.Interfaces;

namespace EcommerceInventoryApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene la lista completa de categorías.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _unitOfWork.Categories.Query()
                .Include(c => c.Products)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Obtiene una categoría por su Id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _unitOfWork.Categories.Query()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = $"No se encontró la categoría con Id {id}." });
            }

            return Ok(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = category.Products.Count
            });
        }

        /// <summary>
        /// Crea una nueva categoría (requiere rol de Admin).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim()
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = 0
            });
        }

        /// <summary>
        /// Actualiza una categoría existente (requiere rol de Admin).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"No se encontró la categoría con Id {id}." });
            }

            category.Name = dto.Name.Trim();
            category.Description = dto.Description.Trim();

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        /// <summary>
        /// Elimina una categoría por Id (requiere rol de Admin).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"No se encontró la categoría con Id {id}." });
            }

            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}
