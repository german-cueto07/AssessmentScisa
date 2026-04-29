using Assessment.AP.data;
using Assessment.AP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assessment.AP.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/v1/categories
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(categories);
    }

    // GET: api/v1/categories/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        
        return Ok(category);
    }

    // POST: api/v1/categories
    [HttpPost]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return UnprocessableEntity("El nombre es requerido."); // Reqnroll pide status 422 

        
        var nameExists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
        if (nameExists)
            return UnprocessableEntity("El nombre ya esta en uso");

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category); // Status 201 o sea Created, con la ubicación del nuevo recurso y el recurso mismo en el body.
    }

    // PUT: api/v1/categories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, Category category)
    {
        if (id != category.Id) return BadRequest();

        if (string.IsNullOrWhiteSpace(category.Name))
            return UnprocessableEntity("El nombre es requerido.");

        _context.Entry(category).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Categories.Any(e => e.Id == id)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    // DELETE: api/v1/categories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        // revisa la relación N:N antes de borrar
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        // esto no elimina si tiene productos
        if (category.Products.Any())
            return BadRequest("No se puede eliminar una categoría que tenga productos asignados.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}