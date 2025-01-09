using BookStore.Helper;
using Infrastructure.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Models.Commands;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BookStore.Services
{
    public interface IOrdersService
    {
        public Task<Guid> CreateOrder(int deliveryTypeNr);
        public Task<List<Order>> GetAllOrders();
        public Task<Guid> AddOrderItem(AddOrderItemCommand command);
    }
    public class OrdersService : IOrdersService
    {
        private readonly IIdentityService _identityService;
        private readonly JwtInfo _jwtInfo;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IBookSearchService _bookSearchService;
        public OrdersService(IIdentityService identityService, JwtInfo jwtInfo, IOrdersRepository ordersRepository, IBookSearchService bookSearchService)
        {
            _identityService = identityService;
            _jwtInfo = jwtInfo;
            _ordersRepository = ordersRepository;
            _bookSearchService = bookSearchService;
        }

        public async Task<List<Order>> GetAllOrders()
        {
            return await _ordersRepository.GetAllOrders();
        }

        public async Task<Guid> CreateOrder(int deliveryTypeNr)
        {
            var login = _jwtInfo.GetName();
            var user = await _identityService.GetUserByLogin(login);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User with given login doesn't exist.");
            }
            var order = Order.Create(deliveryTypeNr, Guid.Parse(user.Id));
            await _ordersRepository.CreateOrder(order);
            return order.Id;
        }
        public async Task<Guid> AddOrderItem(AddOrderItemCommand command)
        {

            var login = _jwtInfo.GetName();
            IdentityUser user;
            Order order;
            bool IsManager = false;
            var userTask = _identityService.GetUserByLogin(login);
            var orderTask = _ordersRepository.GetOrderById(command.OrderId);
            await Task.WhenAll(userTask, orderTask);
            user = userTask.Result;
            order = orderTask.Result;

            if (!IsManager && Guid.Parse(user.Id) != order.UserId)
            {
                throw new AccessViolationException("You aren't allowed to modify this order.");
            }

            if (order.IsSent)
            {
                throw new OrderInProgressException("Order is already in progress.");
            }

            var bookInfo = await _bookSearchService.GetFullInfoByBookId(command.BookId);
            var price = bookInfo.Price;

            var item = OrderedItem.Create(bookInfo, order);

            await _ordersRepository.AddItem(item);
            return item.Id;
        }
    }
}
