using System.Net;
using System.Net.Http.Json;
using Assessment.AP.Models; 
using Reqnroll;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Assessment.AP.dtos;

namespace Assessment.Tests.Steps;

[Binding]
public class CategorySteps
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private HttpResponseMessage _response = null!;
    private Category _categoryData = null!;
    private ProductCreateUpdateDto _productData = null!;
    private int _savedCategoryId;
    private int _savedProductId;
    private Category _categoryUpdateData = null!;
    private ProductCreateUpdateDto _productUpdateData = null!;

    public CategorySteps()
    {
        //  levanta el API en memoria funciona como dotnet run pero para pruebas
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Given(@"Tengo datos para crear una categoria")]
    public void GivenTengoDatosParaCrearUnaCategoria()
    {
        // Se usa un Guid para que el nombre sea único  y la prueba no falle si se corre más veces
        _categoryData = new Category 
        { 
            Name = "Cat_" + Guid.NewGuid().ToString().Substring(0, 5), 
            Description = "Categoria de prueba" 
        };
    }

    [When(@"hago un POST a (.*)")]
    public async Task WhenHagoUnPOSTA(string endpoint)
    {
        // petición real al controlador
        _response = await _client.PostAsJsonAsync(endpoint, _categoryData);
    }

    [Then(@"Deberia recibir status (.*)")]
    public void ThenDeberiaRecibirStatus(int expectedStatusCode)
    {
        // verifica  el código de respuesta (201, 422, etc) para ver que coincida
        var actualStatusCode = (int)_response.StatusCode;
        Assert.Equal(expectedStatusCode, actualStatusCode);
    }

    [Then(@"Deberia haber creado la categoria en la base de datos")]
    public async Task ThenDeberiaHaberCreadoLaCategoriaEnLaBaseDeDatos()
    {
        // Se lee la respuesta de la API, esto devuelve el objeto creado con su ID
        var createdCategory = await _response.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(createdCategory);
        Assert.True(createdCategory.Id > 0);
    }

    [Given(@"Tengo una categoria con nombre ""(.*)""")]
    public async Task GivenTengoUnaCategoriaConNombre(string categoryName)
    {
        // Pre-condición: Asegurar que "Refrescos" exista en la BD mandando un POST primero
        var preCategory = new Category { Name = categoryName, Description = "Pre-existente" };
        await _client.PostAsJsonAsync("/api/v1/categories", preCategory);
    }

    [Given(@"Tengo datos para crear una categoria con nombre ""(.*)""")]
    public void GivenTengoDatosParaCrearUnaCategoriaConNombre(string categoryName)
    {
        _categoryData = new Category { Name = categoryName, Description = "Intento duplicado" };
    }

    [Then(@"Deberia recibir un mensaje de error que el nombre ya esta en uso")]
    public async Task ThenDeberiaRecibirUnMensajeDeErrorQueElNombreYaEstaEnUso()
    {
        var responseString = await _response.Content.ReadAsStringAsync();
        Assert.Contains("El nombre ya esta en uso", responseString); // verifica que el mensaje de error esperado esté en la respuesta
    }


    // Aquí empiezan los steps para productos, 
    // se hace esto para cumplir con el requerimiento de que un producto debe tener al menos una categoría 
    // y para probar la relación N:N entre productos y categorías

    [Given(@"Existe una categoria en la base de datos para el producto")]
    public async Task GivenExisteUnaCategoriaEnLaBaseDeDatosParaElProducto()
    {
        var tempCategory = new Category { Name = "Cat_Prod_" + Guid.NewGuid().ToString().Substring(0, 5) };
        var res = await _client.PostAsJsonAsync("/api/v1/categories", tempCategory);
        var created = await res.Content.ReadFromJsonAsync<Category>();
        _savedCategoryId = created!.Id;
    }

    [Given(@"Tengo datos para crear un producto")]
    public void GivenTengoDatosParaCrearUnProducto()
    {
        _productData = new ProductCreateUpdateDto
        {
            Name = "Prod_" + Guid.NewGuid().ToString().Substring(0, 5),
            Description = "Producto de prueba",
            CategoryIds = new List<int> { _savedCategoryId } // Requerimiento: Al menos una categoría
        };
    }

    [When(@"hago un POST de producto a (.*)")]
    public async Task WhenHagoUnPOSTDeProductoA(string endpoint)
    {
        _response = await _client.PostAsJsonAsync(endpoint, _productData);
    }

    [Then(@"Deberia haber creado el producto en la base de datos")]
    public async Task ThenDeberiaHaberCreadoElProductoEnLaBaseDeDatos()
    {
        var createdProduct = await _response.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.NotNull(createdProduct);
        Assert.True(createdProduct.Id > 0);
        Assert.Contains(createdProduct.Categories, c => c.Id == _savedCategoryId);
    }

    [Given(@"Tengo un producto con nombre ""(.*)"" en esa categoria")]
    public async Task GivenTengoUnProductoConNombreEnEsaCategoria(string productName)
    {
        var preProduct = new ProductCreateUpdateDto
        {
            Name = productName,
            CategoryIds = new List<int> { _savedCategoryId }
        };
        await _client.PostAsJsonAsync("/api/v1/products", preProduct);
    }

    [Given(@"Tengo datos para intentar crear otro producto con nombre ""(.*)"" en la misma categoria")]
    public void GivenTengoDatosParaIntentarCrearOtroProductoConNombreEnLaMismaCategoria(string productName)
    {
        _productData = new ProductCreateUpdateDto
        {
            Name = productName,
            CategoryIds = new List<int> { _savedCategoryId }
        };
    }

    [Then(@"Deberia recibir un mensaje de error de producto duplicado")]
    public async Task ThenDeberiaRecibirUnMensajeDeErrorDeProductoDuplicado()
    {
        var responseString = await _response.Content.ReadAsStringAsync();
        Assert.Contains("El nombre ya esta en uso en una de las categorias seleccionadas", responseString);
    }

    [When(@"hago un GET a (/.+)")] // el regex se ajusta para aceptar endpoints como /api/v1/products o /api/v1/categories
    public async Task WhenHagoUnGETA(string endpoint)
    {
        _response = await _client.GetAsync(endpoint);
    }

    [Then(@"Debe de ver que categorias pertenece con su nombre")]
    public async Task ThenDebeDeVerQueCategoriasPerteneceConSuNombre()
    {
        var productsList = await _response.Content.ReadFromJsonAsync<List<ProductResponseDto>>();
        Assert.NotNull(productsList);
        Assert.NotEmpty(productsList);
        
        // verifica el requerimiento exacto: "no únicamente el ID a la categoría si no el nombre también"
        var productToVerify = productsList.First(p => p.Name == "Teclado");
        Assert.NotEmpty(productToVerify.Categories);
        Assert.False(string.IsNullOrEmpty(productToVerify.Categories.First().Name));
    }
 
    [Given(@"Existen categorias en la base de datos")]
    [Given(@"Existe una categoria en la base de datos")]
    public async Task GivenExisteUnaCategoriaEnLaBaseDeDatos()
    {
        var cat = new Category { Name = "CatTest_" + Guid.NewGuid().ToString().Substring(0, 5), Description = "Test" };
        var res = await _client.PostAsJsonAsync("/api/v1/categories", cat);
        _savedCategoryId = (await res.Content.ReadFromJsonAsync<Category>())!.Id;
    }

    [Given(@"Existen productos en la base de datos")]
    [Given(@"Existe un producto en la base de datos")]
    public async Task GivenExisteUnProductoEnLaBaseDeDatos()
    {
        // se crea la regla para crear una categoría porque el producto requiere al menos una categoría asignada
        await GivenExisteUnaCategoriaEnLaBaseDeDatos();
        
        var prod = new ProductCreateUpdateDto { Name = "ProdTest_" + Guid.NewGuid().ToString().Substring(0, 5), CategoryIds = new List<int> { _savedCategoryId } };
        var res = await _client.PostAsJsonAsync("/api/v1/products", prod);
        _savedProductId = (await res.Content.ReadFromJsonAsync<ProductResponseDto>())!.Id;
    }

    [When(@"hago un GET de lista a ""(.*)""")]
    public async Task WhenHagoUnGETDeListaA(string endpoint)
    {
        _response = await _client.GetAsync(endpoint);
    }

    [When(@"hago un GET a la categoria especifica")]
    public async Task WhenHagoUnGETALaCategoriaEspecifica()
    {
        _response = await _client.GetAsync($"/api/v1/categories/{_savedCategoryId}");
    }

    [When(@"hago un GET al producto especifico")]
    public async Task WhenHagoUnGETAlProductoEspecifico()
    {
        _response = await _client.GetAsync($"/api/v1/products/{_savedProductId}");
    }

    [Given(@"Modifico el nombre de la categoria a ""(.*)""")]
    public void GivenModificoElNombreDeLaCategoriaA(string nuevoNombre)
    {
        _categoryUpdateData = new Category { Id = _savedCategoryId, Name = nuevoNombre, Description = "Actualizada" };
    }

    [Given(@"Modifico el nombre de la categoria a vacio")]
    public void GivenModificoElNombreDeLaCategoriaAVacio()
    {
        _categoryUpdateData = new Category { Id = _savedCategoryId, Name = "", Description = "Vacia" };
    }

    [When(@"hago un PUT a la categoria especifica")]
    public async Task WhenHagoUnPUTALaCategoriaEspecifica()
    {
        _response = await _client.PutAsJsonAsync($"/api/v1/categories/{_savedCategoryId}", _categoryUpdateData);
    }

    [Given(@"Modifico el nombre del producto a ""(.*)""")]
    public void GivenModificoElNombreDelProductoA(string nuevoNombre)
    {
        _productUpdateData = new ProductCreateUpdateDto { Name = nuevoNombre, CategoryIds = new List<int> { _savedCategoryId } };
    }

    [Given(@"Modifico el nombre del producto a vacio")]
    public void GivenModificoElNombreDelProductoAVacio()
    {
        _productUpdateData = new ProductCreateUpdateDto { Name = "", CategoryIds = new List<int> { _savedCategoryId } };
    }

    [When(@"hago un PUT al producto especifico")]
    public async Task WhenHagoUnPUTAlProductoEspecifico()
    {
        _response = await _client.PutAsJsonAsync($"/api/v1/products/{_savedProductId}", _productUpdateData);
    }

    [When(@"hago un DELETE a la categoria especifica")]
    public async Task WhenHagoUnDELETEALaCategoriaEspecifica()
    {
        _response = await _client.DeleteAsync($"/api/v1/categories/{_savedCategoryId}");
    }

    [When(@"hago un DELETE al producto especifico")]
    public async Task WhenHagoUnDELETEAlProductoEspecifico()
    {
        _response = await _client.DeleteAsync($"/api/v1/products/{_savedProductId}");
    }

    [Given(@"Existe una categoria con productos asignados")]
    public async Task GivenExisteUnaCategoriaConProductosAsignados()
    {
        await GivenExisteUnProductoEnLaBaseDeDatos(); // Esto crea la categoría y un producto ligado a ella
    }

    [When(@"elimino el producto asignado")]
    public async Task WhenEliminoElProductoAsignado()
    {
        await _client.DeleteAsync($"/api/v1/products/{_savedProductId}");
    }
}