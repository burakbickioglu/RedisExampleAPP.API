using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisExampleAPP.API.Models;
using RedisExampleAPP.API.Repository;
using RedisExampleAPP.API.Services;
using RedisExampleAPP.Cache;
using StackExchange.Redis;

namespace RedisExampleAPP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    //private readonly IDatabase _database;
    //private readonly RedisService _redisService;
    //public ProductsController(IProductRepository productRepository)
    //{
    //    _productRepository = productRepository;
    //    //_database = database;
    //    //_database.StringSet("soyad", "yıldız");
    //    //_redisService = redisService;
    //}

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _productService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await _productService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        return Created(string.Empty, await _productService.CreateAsync(product));
    }
}
