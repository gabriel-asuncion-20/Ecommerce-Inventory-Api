using System.Collections.Generic;

namespace EcommerceInventoryApi.Core.Entities
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string ShippingAddress { get; set; } = string.Empty;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public PaymentDetails? PaymentDetails { get; set; }
    }

    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PaymentDetails : BaseEntity
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Stripe"; // Stripe, PayPal, MockCard
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string TransactionId { get; set; } = string.Empty;
        public string RawGatewayResponse { get; set; } = string.Empty;
    }
}
