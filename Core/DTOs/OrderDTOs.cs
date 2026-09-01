using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EcommerceInventoryApi.Core.Entities;

namespace EcommerceInventoryApi.Core.DTOs
{
    public class CreateOrderItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }
    }

    public class CreateOrderDto
    {
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public PaymentResultDto? PaymentDetails { get; set; }
    }

    public class ProcessPaymentDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "StripeMock"; // StripeMock, PayPalMock

        [Required]
        public string CardNumber { get; set; } = "4242424242424242"; // Mock card format

        [Required]
        public string ExpiryMonthYear { get; set; } = "12/28";

        [Required]
        public string Cvc { get; set; } = "123";
    }

    public class PaymentResultDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}
