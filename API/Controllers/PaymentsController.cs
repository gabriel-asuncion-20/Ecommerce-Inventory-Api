using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceInventoryApi.Core.DTOs;
using EcommerceInventoryApi.Core.Interfaces;

namespace EcommerceInventoryApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentGatewayService _paymentService;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentsController(IPaymentGatewayService paymentService, IUnitOfWork unitOfWork)
        {
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Procesa el pago de una orden de compra activa a través de la pasarela de pagos.
        /// </summary>
        [HttpPost("process")]
        [ProducesResponseType(typeof(PaymentResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto paymentDto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(paymentDto.OrderId);
            if (order == null)
            {
                return NotFound(new { message = $"Orden con Id {paymentDto.OrderId} no fue encontrada." });
            }

            var result = await _paymentService.ProcessPaymentAsync(paymentDto, order.TotalAmount);
            return Ok(result);
        }
    }
}
