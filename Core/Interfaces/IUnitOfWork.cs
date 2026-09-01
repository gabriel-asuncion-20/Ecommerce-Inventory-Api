using System;
using System.Threading.Tasks;
using EcommerceInventoryApi.Core.Entities;

namespace EcommerceInventoryApi.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Category> Categories { get; }
        IRepository<Product> Products { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }
        IRepository<PaymentDetails> PaymentDetails { get; }

        Task<int> CompleteAsync();
    }
}
