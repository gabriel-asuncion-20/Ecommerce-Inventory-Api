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
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene productos filtrados, ordenados y paginados para el catálogo o inventario.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductQueryFilter filter)
        {
            var query = _unitOfWork.Products.Query().Include(p => p.Category).AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                         p.SKU.ToLower().Contains(term) ||
                                         p.Description.ToLower().Contains(term));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            // Sorting
            query = (filter.SortBy?.ToLower()) switch
            {
                "price" => filter.IsDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "stock" => filter.IsDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
                _ => filter.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                })
                .ToListAsync();

            return Ok(new PagedResult<ProductDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            });
        }

        /// <summary>
        /// Obtiene los detalles de un producto por su Id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _unitOfWork.Products.Query()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Producto con Id {id} no fue encontrado." });
            }

            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category != null ? product.Category.Name : string.Empty
            });
        }

        /// <summary>
        /// Registra un nuevo producto en el inventario (requiere rol Admin).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
        {
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = $"La categoría especificada (Id: {dto.CategoryId}) no existe." });
            }

            var skuExists = await _unitOfWork.Products.FindAsync(p => p.SKU.ToLower() == dto.SKU.ToLower().Trim());
            if (skuExists.Any())
            {
                return BadRequest(new { message = $"El código SKU '{dto.SKU}' ya se encuentra registrado." });
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                SKU = dto.SKU.Trim().ToUpper(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = ""
            });
        }

        /// <summary>
        /// Actualiza un producto existente en el inventario (requiere rol Admin).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Producto con Id {id} no encontrado." });
            }

            product.Name = dto.Name.Trim();
            product.SKU = dto.SKU.Trim().ToUpper();
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.ImageUrl = dto.ImageUrl;
            product.CategoryId = dto.CategoryId;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        /// <summary>
        /// Actualiza rápidamente la cantidad de stock de un producto (requiere rol Admin).
        /// </summary>
        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int newStock)
        {
            if (newStock < 0)
            {
                return BadRequest(new { message = "El stock no puede ser negativo." });
            }

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Producto con Id {id} no encontrado." });
            }

            product.StockQuantity = newStock;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Stock actualizado correctamente.", productId = id, newStock = newStock });
        }

        /// <summary>
        /// Elimina un producto del inventario (requiere rol Admin).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Producto con Id {id} no encontrado." });
            }

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}
