using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Crea una nueva orden de compra, descontando automáticamente las existencias del inventario.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (dto.Items == null || !dto.Items.Any())
            {
                return BadRequest(new { message = "La orden debe contener al menos un producto." });
            }

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemDto in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = $"El producto con Id {itemDto.ProductId} no fue encontrado." });
                }

                if (product.StockQuantity < itemDto.Quantity)
                {
                    return BadRequest(new { message = $"Stock insuficiente para '{product.Name}'. Stock disponible: {product.StockQuantity}, Solicitado: {itemDto.Quantity}." });
                }

                // Descontar inventario
                product.StockQuantity -= itemDto.Quantity;
                _unitOfWork.Products.Update(product);

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                };

                orderItems.Add(orderItem);
                totalAmount += (product.Price * itemDto.Quantity);
            }

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                ShippingAddress = dto.ShippingAddress.Trim(),
                OrderItems = orderItems
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, MapToDto(order));
        }

        /// <summary>
        /// Obtiene el historial de órdenes (el cliente ve las suyas, el Admin ve todas).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            int.TryParse(userIdClaim, out int userId);

            var query = _unitOfWork.Orders.Query()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.PaymentDetails)
                .AsQueryable();

            if (roleClaim != "Admin")
            {
                query = query.Where(o => o.UserId == userId);
            }

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            return Ok(orders.Select(MapToDto));
        }

        /// <summary>
        /// Obtiene los detalles de una orden específica por Id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            int.TryParse(userIdClaim, out int userId);

            var order = await _unitOfWork.Orders.Query()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.PaymentDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = $"No se encontró la orden con Id {id}." });
            }

            if (roleClaim != "Admin" && order.UserId != userId)
            {
                return Forbid();
            }

            return Ok(MapToDto(order));
        }

        /// <summary>
        /// Actualiza el estado de una orden (requiere rol Admin).
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus newStatus)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = $"Orden con Id {id} no encontrada." });
            }

            order.Status = newStatus;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Estado de orden actualizado.", orderId = id, status = newStatus.ToString() });
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserEmail = order.User != null ? order.User.Email : string.Empty,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product != null ? oi.Product.Name : string.Empty,
                    ProductSKU = oi.Product != null ? oi.Product.SKU : string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList(),
                PaymentDetails = order.PaymentDetails != null ? new PaymentResultDto
                {
                    Id = order.PaymentDetails.Id,
                    OrderId = order.PaymentDetails.OrderId,
                    Amount = order.PaymentDetails.Amount,
                    PaymentMethod = order.PaymentDetails.PaymentMethod,
                    Status = order.PaymentDetails.Status.ToString(),
                    TransactionId = order.PaymentDetails.TransactionId,
                    ProcessedAt = order.PaymentDetails.CreatedAt
                } : null
            };
        }
    }
}
