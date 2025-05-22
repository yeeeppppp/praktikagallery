using System;
using System.Collections.Generic;
using System.Linq;
using ArtGalleryStore.Models;

namespace ArtGalleryStore.Services
{
    public class ProductService
    {
        private List<Product> _products;
        
        public IReadOnlyList<Product> Products => _products.AsReadOnly();
        
        // События для оповещения об изменениях в коллекции продуктов
        public event EventHandler ProductsChanged;
        
        public ProductService()
        {
            InitializeProducts();
        }
        
        private void InitializeProducts()
        {
            // В реальном приложении это должно быть загружено из базы данных
            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Title = "Закат над озером",
                    Description = "Живописная картина с изображением заката над горным озером.",
                    Price = 15000,
                    ImageUrl = "https://images.unsplash.com/photo-1518998053901-5348d3961a04",
                    Artist = "Анна Иванова",
                    Medium = "масло",
                    Size = "60x80 см",
                    Year = 2023,
                    InStock = true,
                    CreatedAt = DateTime.Now.AddDays(-20)
                },
                new Product
                {
                    Id = 2,
                    Title = "Абстрактная композиция",
                    Description = "Современная абстрактная живопись с яркими цветами.",
                    Price = 12000,
                    ImageUrl = "https://images.unsplash.com/photo-1536924940846-227afb31e2a5",
                    Artist = "Михаил Чен",
                    Medium = "акрил",
                    Size = "70x100 см",
                    Year = 2022,
                    InStock = true,
                    CreatedAt = DateTime.Now.AddDays(-45)
                },
                new Product
                {
                    Id = 3,
                    Title = "Морской пейзаж",
                    Description = "Спокойный вид на океан с далеким горизонтом.",
                    Price = 8000,
                    ImageUrl = "https://images.unsplash.com/photo-1580137189272-c9379f8864fd",
                    Artist = "Сара Миллер",
                    Medium = "акварель",
                    Size = "50x70 см",
                    Year = 2023,
                    InStock = true,
                    CreatedAt = DateTime.Now.AddDays(-15)
                },
                new Product
                {
                    Id = 4,
                    Title = "Городской пейзаж",
                    Description = "Современный городской пейзаж с высотными зданиями.",
                    Price = 20000,
                    ImageUrl = "https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9",
                    Artist = "Алексей Петров",
                    Medium = "масло",
                    Size = "90x120 см",
                    Year = 2021,
                    InStock = true,
                    CreatedAt = DateTime.Now.AddDays(-60)
                },
                new Product
                {
                    Id = 5,
                    Title = "Цветочная композиция",
                    Description = "Яркая композиция с полевыми цветами.",
                    Price = 6500,
                    ImageUrl = "https://images.unsplash.com/photo-1445110236002-8fd381e285e9",
                    Artist = "Елена Смирнова",
                    Medium = "акрил",
                    Size = "40x50 см",
                    Year = 2023,
                    InStock = true,
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new Product
                {
                    Id = 6,
                    Title = "Зимний лес",
                    Description = "Атмосферный зимний пейзаж с заснеженными деревьями.",
                    Price = 18000,
                    ImageUrl = "https://images.unsplash.com/photo-1418985991508-e47386d96a71",
                    Artist = "Иван Кузнецов",
                    Medium = "масло",
                    Size = "65x85 см",
                    Year = 2022,
                    InStock = false,
                    CreatedAt = DateTime.Now.AddDays(-90)
                }
            };
        }
        
        public Product GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }
        
        public List<Product> GetProductsByArtist(string artist)
        {
            return _products.Where(p => p.Artist.Contains(artist, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        public List<Product> GetProductsByMedium(string medium)
        {
            return _products.Where(p => p.Medium.Equals(medium, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return _products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
        }
        
        public List<Product> SearchProducts(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _products.ToList();
                
            query = query.ToLower();
            
            return _products.Where(p => 
                p.Title.ToLower().Contains(query) || 
                p.Description.ToLower().Contains(query) || 
                p.Artist.ToLower().Contains(query) ||
                p.Medium.ToLower().Contains(query)
            ).ToList();
        }
        
        public List<Product> GetAllProducts()
        {
            return _products.ToList();
        }
        
        public List<Product> FilterProducts(string searchQuery = null, string medium = null, 
            decimal? minPrice = null, decimal? maxPrice = null, bool? inStock = null)
        {
            var query = _products.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(searchQuery) || 
                    p.Description.ToLower().Contains(searchQuery) || 
                    p.Artist.ToLower().Contains(searchQuery)
                );
            }
            
            if (!string.IsNullOrWhiteSpace(medium))
            {
                query = query.Where(p => p.Medium.Equals(medium, StringComparison.OrdinalIgnoreCase));
            }
            
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }
            
            if (inStock.HasValue)
            {
                query = query.Where(p => p.InStock == inStock.Value);
            }
            
            return query.ToList();
        }
        
        public bool AddProduct(Product product)
        {
            if (product == null)
                return false;
                
            // Установка ID для нового продукта
            product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
            product.CreatedAt = DateTime.Now;
            
            _products.Add(product);
            ProductsChanged?.Invoke(this, EventArgs.Empty);
            
            return true;
        }
        
        public bool UpdateProduct(Product product)
        {
            if (product == null)
                return false;
                
            var index = _products.FindIndex(p => p.Id == product.Id);
            if (index == -1)
                return false;
            
            _products[index] = product;
            ProductsChanged?.Invoke(this, EventArgs.Empty);
            
            return true;
        }
        
        public bool DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return false;
                
            _products.Remove(product);
            ProductsChanged?.Invoke(this, EventArgs.Empty);
            
            return true;
        }
    }
}