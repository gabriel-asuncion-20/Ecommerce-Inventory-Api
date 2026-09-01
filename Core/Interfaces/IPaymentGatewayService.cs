using System.Threading.Tasks;
using EcommerceInventoryApi.Core.DTOs;

namespace EcommerceInventoryApi.Core.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentDto paymentDto, decimal amount);
    }
}
