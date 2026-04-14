using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Models;
using ProductService.Persistence;
using ProductService.Requests;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductServiceContext _context;

        public ProductController(ProductServiceContext context)
        {
            _context = context;
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> List()
        {
            var products = await _context.Products.ToListAsync();

            return Ok(products);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Name == dto.Name);
            if (product != null)
            {
                return BadRequest($"Product with name {product.Name} is exist");
            }

            var newProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
            };
            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            return Ok(newProduct.Id);
        }
    }
}
