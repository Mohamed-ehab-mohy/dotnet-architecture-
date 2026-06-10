using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Set<Product>()
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string searchTerm)
    {
        return await _context.Set<Product>()
            .Where(p => p.Name.Contains(searchTerm))
            .ToListAsync();
    }
}
