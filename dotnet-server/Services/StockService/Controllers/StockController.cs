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
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> List()
        {
            var stockItems = await _context.StockItems.ToListAsync();

            return Ok(stockItems.Select(x => new { x.ProductId, x.ProductName, x.QuantityAvailable }));
        }
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddItem([FromBody] AddStockItemRequest dto)
        {
            var stockItem = await _context.StockItems.FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);
            if(stockItem == null)
            {
                var newStockItem = new Models.StockItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = dto.ProductId,
                    ProductName = dto.ProductName,
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
