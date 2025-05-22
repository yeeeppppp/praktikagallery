using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtGalleryStore.Services;
using ArtGalleryStore.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ArtGalleryStore.Tests.Services
{
    [TestClass]
    public class ProductServiceTests
    {
        private ProductService _productService;

        [TestInitialize]
        public void Setup()
        {
            // Инициализируем сервис перед каждым тестом
            _productService = new ProductService();
        }

        [TestMethod]
        public void GetAllProducts_ShouldReturnProducts()
        {
            // Arrange
            // По умолчанию сервис уже должен содержать демонстрационные продукты

            // Act
            var products = _productService.GetAllProducts();

            // Assert
            Assert.IsNotNull(products);
            Assert.IsTrue(products.Any(), "Продукты должны быть в списке");
        }

        [TestMethod]
        public void GetProductById_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var allProducts = _productService.GetAllProducts();
            var testProduct = allProducts.First();
            int testProductId = testProduct.Id;

            // Act
            var product = _productService.GetProductById(testProductId);

            // Assert
            Assert.IsNotNull(product);
            Assert.AreEqual(testProductId, product.Id);
            Assert.AreEqual(testProduct.Title, product.Title);
        }

        [TestMethod]
        public void GetProductById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            int invalidId = -1;

            // Act
            var product = _productService.GetProductById(invalidId);

            // Assert
            Assert.IsNull(product);
        }

        [TestMethod]
        public void AddProduct_ShouldCreateNewProduct()
        {
            // Arrange
            var newProduct = new Product
            {
                Title = "Тестовая картина",
                Artist = "Тестовый художник",
                Description = "Описание тестовой картины",
                Price = 15000,
                Medium = "Масло",
                Size = "50x70 см",
                InStock = true,
                ImageUrl = "/images/test.jpg"
            };

            // Act
            var result = _productService.AddProduct(newProduct);
            var retrievedProduct = _productService.GetProductById(result.Id);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Product);
            Assert.IsNotNull(retrievedProduct);
            Assert.AreEqual(newProduct.Title, retrievedProduct.Title);
            Assert.AreEqual(newProduct.Artist, retrievedProduct.Artist);
            Assert.AreEqual(newProduct.Price, retrievedProduct.Price);
        }

        [TestMethod]
        public void UpdateProduct_WithValidId_ShouldUpdateProduct()
        {
            // Arrange
            var allProducts = _productService.GetAllProducts();
            var productToUpdate = allProducts.First();
            int productId = productToUpdate.Id;

            var updatedProduct = new Product
            {
                Id = productId,
                Title = "Обновленная картина",
                Artist = productToUpdate.Artist,
                Description = "Обновленное описание",
                Price = productToUpdate.Price + 5000,
                Medium = productToUpdate.Medium,
                Size = productToUpdate.Size,
                InStock = productToUpdate.InStock,
                ImageUrl = productToUpdate.ImageUrl
            };

            // Act
            var result = _productService.UpdateProduct(updatedProduct);
            var retrievedProduct = _productService.GetProductById(productId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(retrievedProduct);
            Assert.AreEqual(updatedProduct.Title, retrievedProduct.Title);
            Assert.AreEqual(updatedProduct.Description, retrievedProduct.Description);
            Assert.AreEqual(updatedProduct.Price, retrievedProduct.Price);
        }

        [TestMethod]
        public void UpdateProduct_WithInvalidId_ShouldFail()
        {
            // Arrange
            int invalidId = -1;
            var product = new Product
            {
                Id = invalidId,
                Title = "Недействительный продукт",
                Artist = "Неизвестный художник",
                Price = 1000,
                InStock = true
            };

            // Act
            var result = _productService.UpdateProduct(product);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public void DeleteProduct_WithValidId_ShouldRemoveProduct()
        {
            // Arrange
            // Создаем продукт для последующего удаления
            var newProduct = new Product
            {
                Title = "Удаляемая картина",
                Artist = "Тестовый художник",
                Price = 10000,
                InStock = true
            };
            var addResult = _productService.AddProduct(newProduct);
            Assert.IsTrue(addResult.Success);
            int productId = addResult.Product.Id;

            // Act
            var deleteResult = _productService.DeleteProduct(productId);
            var retrievedProduct = _productService.GetProductById(productId);

            // Assert
            Assert.IsTrue(deleteResult.Success);
            Assert.IsNull(retrievedProduct);
        }

        [TestMethod]
        public void DeleteProduct_WithInvalidId_ShouldFail()
        {
            // Arrange
            int invalidId = -1;

            // Act
            var result = _productService.DeleteProduct(invalidId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public void SearchProducts_ByTitle_ShouldReturnMatchingProducts()
        {
            // Arrange
            string searchTerm = "пейзаж";
            
            // Добавим продукт с заданным термином в названии
            var product1 = new Product
            {
                Title = "Горный пейзаж",
                Artist = "Тестовый художник",
                Price = 12000,
                InStock = true
            };
            
            var product2 = new Product
            {
                Title = "Портрет девушки",
                Artist = "Другой художник",
                Price = 15000,
                InStock = true
            };
            
            _productService.AddProduct(product1);
            _productService.AddProduct(product2);

            // Act
            var results = _productService.SearchProducts(searchTerm, null, null, null, null);

            // Assert
            Assert.IsTrue(results.Any(p => p.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            Assert.IsFalse(results.Any(p => p.Title.Equals(product2.Title)));
        }

        [TestMethod]
        public void SearchProducts_ByPriceRange_ShouldReturnMatchingProducts()
        {
            // Arrange
            decimal minPrice = 10000;
            decimal maxPrice = 20000;
            
            // Добавим продукты в разных ценовых диапазонах
            var cheapProduct = new Product
            {
                Title = "Дешевая картина",
                Artist = "Художник",
                Price = 5000,
                InStock = true
            };
            
            var mediumProduct = new Product
            {
                Title = "Средняя картина",
                Artist = "Художник",
                Price = 15000,
                InStock = true
            };
            
            var expensiveProduct = new Product
            {
                Title = "Дорогая картина",
                Artist = "Художник",
                Price = 25000,
                InStock = true
            };
            
            _productService.AddProduct(cheapProduct);
            _productService.AddProduct(mediumProduct);
            _productService.AddProduct(expensiveProduct);

            // Act
            var results = _productService.SearchProducts(null, null, minPrice, maxPrice, null);

            // Assert
            Assert.IsTrue(results.All(p => p.Price >= minPrice && p.Price <= maxPrice));
            Assert.IsTrue(results.Any(p => p.Title.Equals(mediumProduct.Title)));
            Assert.IsFalse(results.Any(p => p.Title.Equals(cheapProduct.Title)));
            Assert.IsFalse(results.Any(p => p.Title.Equals(expensiveProduct.Title)));
        }

        [TestMethod]
        public void SearchProducts_ByInStock_ShouldReturnMatchingProducts()
        {
            // Arrange
            bool inStockOnly = true;
            
            // Добавим продукты с разными статусами наличия
            var inStockProduct = new Product
            {
                Title = "В наличии",
                Artist = "Художник",
                Price = 10000,
                InStock = true
            };
            
            var outOfStockProduct = new Product
            {
                Title = "Нет в наличии",
                Artist = "Художник",
                Price = 10000,
                InStock = false
            };
            
            _productService.AddProduct(inStockProduct);
            _productService.AddProduct(outOfStockProduct);

            // Act
            var results = _productService.SearchProducts(null, null, null, null, inStockOnly);

            // Assert
            Assert.IsTrue(results.All(p => p.InStock));
            Assert.IsTrue(results.Any(p => p.Title.Equals(inStockProduct.Title)));
            Assert.IsFalse(results.Any(p => p.Title.Equals(outOfStockProduct.Title)));
        }
    }
}