using Assessment.AP.data;
using Assessment.AP.dtos;
using Assessment.AP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assessment.AP.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/v1/products
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        // muestra a qué categorías pertenece con su nombre
        var products = await _context.Products
            .Include(p => p.Categories)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Categories = p.Categories.Select(c => new CategoryInfoDto { Id = c.Id, Name = c.Name }).ToList()
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: api/v1/products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Categories)
            .Where(p => p.Id == id)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Categories = p.Categories.Select(c => new CategoryInfoDto { Id = c.Id, Name = c.Name }).ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null) return NotFound();
        return Ok(product);
    }

    // POST: api/v1/products
    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductCreateUpdateDto request)
    {
        var validationError = await ValidateProductRulesAsync(request.Name, request.CategoryIds);
        if (validationError != null)
            return UnprocessableEntity(validationError); // Hace Status 422 para los tests

        var categoriesToAttach = await _context.Categories
            .Where(c => request.CategoryIds.Contains(c.Id))
            .ToListAsync();

        var newProduct = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Categories = categoriesToAttach
        };

        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();

        //respuesta con el nuevo producto creado, incluyendo las categorías a las que pertenece, 
        // para que el cliente tenga toda la información necesaria sin hacer otra consulta
        var responseDto = new ProductResponseDto
        {
            Id = newProduct.Id,
            Name = newProduct.Name,
            Description = newProduct.Description,
            Categories = categoriesToAttach.Select(c => new CategoryInfoDto { Id = c.Id, Name = c.Name }).ToList()
        };

        return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, responseDto);
    }

    // PUT: api/v1/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductCreateUpdateDto request)
    {
        var validationError = await ValidateProductRulesAsync(request.Name, request.CategoryIds, id);
        if (validationError != null)
            return UnprocessableEntity(validationError);

        var product = await _context.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        // Actualizar datos
        product.Name = request.Name;
        product.Description = request.Description;

        // actualiza relaciones N:N para borrar las anteriores y poner las nuevas
        product.Categories.Clear();
        var newCategories = await _context.Categories
            .Where(c => request.CategoryIds.Contains(c.Id))
            .ToListAsync();
        
        product.Categories.AddRange(newCategories);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/v1/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        // aquí entra EF Core para borrar los registros de la tabla intermedia automáticamente y no crear un error de llave foránea
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    //  métodos para evitar duplicación de código (principio DRY de SOLID) pensado en una arquitectura limpia
    private async Task<string?> ValidateProductRulesAsync(string name, List<int> categoryIds, int? productIdToIgnore = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return "El nombre es requerido.";
        
        // Requerimiento: Al menos una categoría
        if (categoryIds == null || !categoryIds.Any()) 
            return "El producto debe tener al menos una categoría asignada.";

        var existingCategoriesCount = await _context.Categories.CountAsync(c => categoryIds.Contains(c.Id));
        if (existingCategoriesCount != categoryIds.Count) 
            return "Una o más categorías enviadas no existen en la base de datos.";

        // Requerimiento: Nombre único por categoría
        var nameLower = name.ToLower();
        var duplicateExists = await _context.Products
            .Include(p => p.Categories)
            .Where(p => productIdToIgnore == null || p.Id != productIdToIgnore) // ignora el producto actual si es un Update
            .AnyAsync(p => p.Name.ToLower() == nameLower && p.Categories.Any(c => categoryIds.Contains(c.Id)));

        if (duplicateExists) 
            return "El nombre ya esta en uso en una de las categorias seleccionadas.";

        return null; //  todo está perfecto no hay repetidos ni errores de validación
    }
}