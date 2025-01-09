using Infrastructure.Entities;
using Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public interface IOrdersRepository
    {
        public Task CreateOrder(Order order);
        public Task ChangeCost(Order order, double amountToAdd);
        public Task<List<Order>> GetAllOrders();
        public Task AddItem(OrderedItem item);
        public Task<Order> GetOrderById(Guid Id);
    }
    public class OrdersRepository : IOrdersRepository
    {
        public ApplicationDbContext Context { get; set; }
        public OrdersRepository(ApplicationDbContext context) { Context = context; }

        public async Task CreateOrder(Order order)
        {
            Context.Orders
                .Add(order);
            await Context.SaveChangesAsync();
        }
        public async Task ChangeCost(Order order, double amountToAdd)
        {
            order.ChangeCost(amountToAdd);
            await Context.SaveChangesAsync();
        }
        public async Task<List<Order>> GetAllOrders()
        {
            return await Context.Orders.ToListAsync();
        }
        public async Task<Order> GetOrderById(Guid Id)
        {
            return await Context.Orders.Where
                (x => x.Id == Id)
                .FirstAsync();
        }

        public async Task AddItem(OrderedItem item)
        {
            await Context.OrderedItems.AddAsync(item);
            await Context.SaveChangesAsync();
        }
    }
}
