using RedisExampleAPP.API.Models;
using RedisExampleAPP.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisExampleAPP.API.Repository;

public class ProductRepositoryWithCacheDecorator : IProductRepository
{
    private const string productKey = "productCaches";
    private readonly IProductRepository _productRepository;
    private readonly RedisService _redisService;
    private readonly IDatabase _cacheRepository;
    public ProductRepositoryWithCacheDecorator(IProductRepository productRepository, RedisService redisService)
    {
        _productRepository = productRepository;
        _redisService = redisService;
        _cacheRepository = _redisService.GetDb(2);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        var newProduct = await _productRepository.CreateAsync(product);
        if(await _cacheRepository.KeyExistsAsync(productKey))
        {
            await _cacheRepository.HashSetAsync(productKey, newProduct.Id, JsonSerializer.Serialize(newProduct));
        }
        return newProduct;

    }

    public async Task<List<Product>> GetAllAsync()
    {
        // data cache de yoksa
        if (!await _cacheRepository.KeyExistsAsync(productKey))
        {
            return await LoadToCacheFromDbAsync();
        }

        // data cache de var ise
        var products = new List<Product>();
        var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);
        foreach (var item in cacheProducts.ToList())
        {
            var product = JsonSerializer.Deserialize<Product>(item.Value);
            products.Add(product);
        }
        return products;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        // data cache de var ise
        if (_cacheRepository.KeyExists(productKey))
        {
            var product = await _cacheRepository.HashGetAsync(productKey, id);
            return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
        }

        // data cache de yoksa
        var products = await LoadToCacheFromDbAsync();
        return products.FirstOrDefault(p => p.Id == id);
    }

    // datayı db den alıp cache e kaydetme
    private async Task<List<Product>> LoadToCacheFromDbAsync()
    {
        var products = await _productRepository.GetAllAsync();
        products.ForEach(p =>
        {
            _cacheRepository.HashSetAsync(productKey, p.Id, JsonSerializer.Serialize(p));
        });
        return products;
    }
}
