namespace EcommerceInventoryApi.Core.Entities
{
    public enum UserRole
    {
        Customer = 0,
        Admin = 1
    }

    public enum OrderStatus
    {
        Pending = 0,
        Paid = 1,
        Shipped = 2,
        Cancelled = 3
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Refunded = 3
    }
}
