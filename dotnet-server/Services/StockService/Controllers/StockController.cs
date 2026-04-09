using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockService.Persistence;
using StockService.Requests;

namespace StockService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        private readonly StockServiceContext _context;

        public StockController(StockServiceContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> AddStockItem([FromBody] AddStockItemRequest dto)
        {
            var stockItem = await _context.StockItems.FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);
            if(stockItem == null)
            {
                var newStockItem = new Models.StockItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = dto.ProductId,
                    QuantityAvailable = dto.Quantity
                };
                await _context.StockItems.AddAsync(newStockItem);
            }
            else
            {
                stockItem.QuantityAvailable += dto.Quantity;
            }
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
