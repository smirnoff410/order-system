using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using OrderService.Persistence;
using OrderService.Requests;
using OrderService.Services;
using SharedKafkaEvents.Events;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderServiceContext _dbContext;
        private readonly KafkaProducerService _kafkaProducer;
        public OrdersController(OrderServiceContext dbContext, KafkaProducerService kafkaProducer)
        {
            _dbContext = dbContext;
            _kafkaProducer = kafkaProducer;
        }
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            // Начинаем транзакцию
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Создаем заказ в БД
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    CustomerName = request.CustomerName,
                    TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    Items = request.Items.Select(i => new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                await _dbContext.Orders.AddAsync(order);
                await _dbContext.SaveChangesAsync();

                // 2. Публикуем событие
                var orderEvent = new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    Items = order.Items.Select(i => new OrderItemEvent
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList(),
                    TotalAmount = order.TotalAmount,
                    CreatedAt = order.CreatedAt
                };

                await _kafkaProducer.PublishOrderCreatedAsync(orderEvent);

                // 3. Коммитим транзакцию
                await transaction.CommitAsync();

                return Ok(new { OrderId = order.Id, Status = "Pending" });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Status(Guid orderId)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);

            if(order == null)
            {
                return BadRequest($"Order with id {orderId} not found");
            }

            return Ok(new { OrderId = order.Id, Status = order.Status });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> List()
        {
            var orders = await _dbContext.Orders.OrderByDescending(x => x.CreatedAt).ToListAsync();

            return Ok(orders.Select(x => new { OrderId = x.Id, x.Status, x.CreatedAt, x.CustomerId, x.CustomerName, x.TotalAmount }));
        }
    }
}
