using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtGalleryStore.ViewModels;
using ArtGalleryStore.Services;
using ArtGalleryStore.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ArtGalleryStore.Tests.ViewModels
{
    [TestClass]
    public class ProductsViewModelTests
    {
        private Mock<ProductService> _mockProductService;
        private Mock<AuthService> _mockAuthService;
        private Mock<CartService> _mockCartService;
        private ProductsViewModel _viewModel;
        private List<Product> _testProducts;

        [TestInitialize]
        public void Setup()
        {
            // Создаем тестовые продукты
            _testProducts = new List<Product>
            {
                new Product 
                { 
                    Id = 1, 
                    Title = "Горный пейзаж", 
                    Artist = "Иван Иванов", 
                    Price = 15000, 
                    Medium = "Масло", 
                    InStock = true 
                },
                new Product 
                { 
                    Id = 2, 
                    Title = "Морской закат", 
                    Artist = "Петр Петров", 
                    Price = 25000, 
                    Medium = "Акварель", 
                    InStock = true 
                },
                new Product 
                { 
                    Id = 3, 
                    Title = "Абстракция", 
                    Artist = "Анна Смирнова", 
                    Price = 18000, 
                    Medium = "Акрил", 
                    InStock = false 
                }
            };

            // Настраиваем мок ProductService
            _mockProductService = new Mock<ProductService>();
            _mockProductService.Setup(s => s.GetAllProducts()).Returns(_testProducts);
            _mockProductService.Setup(s => s.SearchProducts(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), 
                It.IsAny<decimal?>(), It.IsAny<bool?>())).Returns(
                    (string query, string medium, decimal? minPrice, decimal? maxPrice, bool? inStock) => 
                    {
                        var filteredProducts = _testProducts;
                        
                        if (!string.IsNullOrEmpty(query))
                        {
                            filteredProducts = filteredProducts.Where(p => 
                                p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                                p.Artist.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
                        }
                        
                        if (!string.IsNullOrEmpty(medium))
                        {
                            filteredProducts = filteredProducts.Where(p => 
                                p.Medium == medium).ToList();
                        }
                        
                        if (minPrice.HasValue)
                        {
                            filteredProducts = filteredProducts.Where(p => 
                                p.Price >= minPrice.Value).ToList();
                        }
                        
                        if (maxPrice.HasValue)
                        {
                            filteredProducts = filteredProducts.Where(p => 
                                p.Price <= maxPrice.Value).ToList();
                        }
                        
                        if (inStock.HasValue)
                        {
                            filteredProducts = filteredProducts.Where(p => 
                                p.InStock == inStock.Value).ToList();
                        }
                        
                        return filteredProducts;
                    });

            // Настраиваем мок AuthService
            _mockAuthService = new Mock<AuthService>();
            var testUser = new User { Id = 1, Username = "testuser", Name = "Test User", IsAdmin = false };
            _mockAuthService.Setup(a => a.IsLoggedIn).Returns(true);
            _mockAuthService.Setup(a => a.CurrentUser).Returns(testUser);

            // Настраиваем мок CartService
            _mockCartService = new Mock<CartService>();
            _mockCartService.Setup(c => c.AddToCart(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            // Создаем ViewModel
            _viewModel = new ProductsViewModel(_mockAuthService.Object, _mockProductService.Object, _mockCartService.Object);
        }

        [TestMethod]
        public void LoadProducts_ShouldPopulateProductsList()
        {
            // Arrange
            _viewModel.Products.Clear();

            // Act
            _viewModel.LoadProducts();

            // Assert
            Assert.AreEqual(_testProducts.Count, _viewModel.Products.Count);
        }

        [TestMethod]
        public void SearchProducts_WithSearchQuery_ShouldFilterProducts()
        {
            // Arrange
            string searchQuery = "пейзаж";

            // Act
            _viewModel.SearchQuery = searchQuery;

            // Assert
            Assert.AreEqual(1, _viewModel.Products.Count);
            Assert.IsTrue(_viewModel.Products.All(p => 
                p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || 
                p.Artist.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void SearchProducts_WithMedium_ShouldFilterProducts()
        {
            // Arrange
            string medium = "Акварель";

            // Act
            _viewModel.SelectedMedium = medium;

            // Assert
            Assert.AreEqual(1, _viewModel.Products.Count);
            Assert.IsTrue(_viewModel.Products.All(p => p.Medium == medium));
        }

        [TestMethod]
        public void SearchProducts_WithPriceRange_ShouldFilterProducts()
        {
            // Arrange
            decimal minPrice = 20000;
            decimal maxPrice = 30000;

            // Act
            _viewModel.MinPrice = minPrice;
            _viewModel.MaxPrice = maxPrice;

            // Assert
            Assert.AreEqual(1, _viewModel.Products.Count);
            Assert.IsTrue(_viewModel.Products.All(p => p.Price >= minPrice && p.Price <= maxPrice));
        }

        [TestMethod]
        public void SearchProducts_WithInStockOnly_ShouldFilterProducts()
        {
            // Arrange
            bool inStockOnly = true;

            // Act
            _viewModel.InStockOnly = inStockOnly;

            // Assert
            Assert.IsTrue(_viewModel.Products.Count > 0);
            Assert.IsTrue(_viewModel.Products.All(p => p.InStock == true));
        }

        [TestMethod]
        public void ResetFilters_ShouldClearAllFilters()
        {
            // Arrange
            _viewModel.SearchQuery = "пейзаж";
            _viewModel.SelectedMedium = "Масло";
            _viewModel.MinPrice = 10000;
            _viewModel.MaxPrice = 20000;
            _viewModel.InStockOnly = true;

            // Act
            _viewModel.ResetFiltersCommand.Execute(null);

            // Assert
            Assert.IsNull(_viewModel.SearchQuery);
            Assert.IsNull(_viewModel.SelectedMedium);
            Assert.IsNull(_viewModel.MinPrice);
            Assert.IsNull(_viewModel.MaxPrice);
            Assert.IsNull(_viewModel.InStockOnly);
            Assert.AreEqual(_testProducts.Count, _viewModel.Products.Count);
        }

        [TestMethod]
        public void AddToCart_LoggedInUser_ShouldAddProductToCart()
        {
            // Arrange
            var product = _testProducts.First(p => p.InStock);

            // Act
            _viewModel.AddToCartCommand.Execute(product);

            // Assert
            _mockCartService.Verify(c => 
                c.AddToCart(_mockAuthService.Object.CurrentUser.Id, product.Id, 1), Times.Once);
        }

        [TestMethod]
        public void AddToCart_NotLoggedIn_ShouldShowError()
        {
            // Arrange
            var product = _testProducts.First(p => p.InStock);
            _mockAuthService.Setup(a => a.IsLoggedIn).Returns(false);
            _mockAuthService.Setup(a => a.CurrentUser).Returns((User)null);

            // Act
            _viewModel.AddToCartCommand.Execute(product);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(_viewModel.ErrorMessage));
            _mockCartService.Verify(c => 
                c.AddToCart(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void ViewProductDetails_ShouldSelectProduct()
        {
            // Arrange
            var product = _testProducts.First();

            // Act
            _viewModel.ViewProductDetailsCommand.Execute(product);

            // Assert
            Assert.IsNotNull(_viewModel.SelectedProduct);
            Assert.AreEqual(product.Id, _viewModel.SelectedProduct.Id);
        }

        [TestMethod]
        public void CloseProductDetails_ShouldClearSelectedProduct()
        {
            // Arrange
            _viewModel.SelectedProduct = _testProducts.First();

            // Act
            _viewModel.CloseProductDetailsCommand.Execute(null);

            // Assert
            Assert.IsNull(_viewModel.SelectedProduct);
        }
    }
}