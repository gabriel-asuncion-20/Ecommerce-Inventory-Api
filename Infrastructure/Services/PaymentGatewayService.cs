using System;
using System.Threading.Tasks;
using EcommerceInventoryApi.Core.DTOs;
using EcommerceInventoryApi.Core.Entities;
using EcommerceInventoryApi.Core.Interfaces;

namespace EcommerceInventoryApi.Infrastructure.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentGatewayService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentDto paymentDto, decimal amount)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(paymentDto.OrderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"No se encontró la orden con Id {paymentDto.OrderId}.");
            }

            if (order.Status == OrderStatus.Paid)
            {
                throw new InvalidOperationException($"La orden #{order.Id} ya ha sido pagada previamente.");
            }

            // Simple validation simulation (e.g. card ending in 0000 fails)
            bool isSuccessful = !paymentDto.CardNumber.EndsWith("0000");

            var transactionId = $"TXN_{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
            var status = isSuccessful ? PaymentStatus.Completed : PaymentStatus.Failed;

            var paymentDetails = new PaymentDetails
            {
                OrderId = order.Id,
                Amount = amount,
                PaymentMethod = paymentDto.PaymentMethod,
                Status = status,
                TransactionId = transactionId,
                RawGatewayResponse = isSuccessful
                    ? $"{{\"status\": \"succeeded\", \"gateway\": \"{paymentDto.PaymentMethod}\", \"txn\": \"{transactionId}\"}}"
                    : $"{{\"status\": \"failed\", \"error\": \"Declined card\", \"gateway\": \"{paymentDto.PaymentMethod}\"}}"
            };

            await _unitOfWork.PaymentDetails.AddAsync(paymentDetails);

            if (isSuccessful)
            {
                order.Status = OrderStatus.Paid;
                _unitOfWork.Orders.Update(order);
            }

            await _unitOfWork.CompleteAsync();

            return new PaymentResultDto
            {
                Id = paymentDetails.Id,
                OrderId = order.Id,
                Amount = paymentDetails.Amount,
                PaymentMethod = paymentDetails.PaymentMethod,
                Status = paymentDetails.Status.ToString(),
                TransactionId = paymentDetails.TransactionId,
                ProcessedAt = paymentDetails.CreatedAt
            };
        }
    }
}
