using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtGalleryStore.Services;
using ArtGalleryStore.Models;
using System;
using System.Linq;

namespace ArtGalleryStore.Tests.Services
{
    [TestClass]
    public class CartServiceTests
    {
        private CartService _cartService;
        private ProductService _productService;
        private int _testUserId = 1;
        private Product _testProduct;

        [TestInitialize]
        public void Setup()
        {
            _productService = new ProductService();
            
            // Создаем тестовый продукт
            _testProduct = new Product
            {
                Title = "Тестовая картина для корзины",
                Artist = "Тестовый художник",
                Description = "Описание тестовой картины",
                Price = 10000,
                Medium = "Масло",
                Size = "40x60 см",
                InStock = true,
                ImageUrl = "/images/test.jpg"
            };
            
            var result = _productService.AddProduct(_testProduct);
            _testProduct = result.Product; // Получаем созданный продукт с присвоенным ID
            
            // Инициализируем CartService
            _cartService = new CartService();
        }

        [TestMethod]
        public void GetCart_ShouldReturnEmptyCartForNewUser()
        {
            // Arrange
            int newUserId = 999;

            // Act
            var cart = _cartService.GetCart(newUserId);

            // Assert
            Assert.IsNotNull(cart);
            Assert.AreEqual(newUserId, cart.UserId);
            Assert.IsFalse(cart.Items.Any());
            Assert.AreEqual(0, cart.TotalItems);
            Assert.AreEqual(0, cart.Total);
        }

        [TestMethod]
        public void AddToCart_NewItem_ShouldAddItemToCart()
        {
            // Arrange
            int productId = _testProduct.Id;
            int quantity = 1;

            // Act
            bool result = _cartService.AddToCart(_testUserId, productId, quantity);
            var cart = _cartService.GetCart(_testUserId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(cart.Items.Any());
            Assert.AreEqual(1, cart.Items.Count);
            Assert.AreEqual(productId, cart.Items[0].ProductId);
            Assert.AreEqual(quantity, cart.Items[0].Quantity);
            Assert.AreEqual(_testProduct.Price * quantity, cart.Total);
        }

        [TestMethod]
        public void AddToCart_ExistingItem_ShouldIncreaseQuantity()
        {
            // Arrange
            int productId = _testProduct.Id;
            int initialQuantity = 1;
            int additionalQuantity = 2;

            // Добавляем товар в корзину первый раз
            _cartService.AddToCart(_testUserId, productId, initialQuantity);
            
            // Act - добавляем тот же товар еще раз
            bool result = _cartService.AddToCart(_testUserId, productId, additionalQuantity);
            var cart = _cartService.GetCart(_testUserId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(cart.Items.Any());
            Assert.AreEqual(1, cart.Items.Count);
            Assert.AreEqual(productId, cart.Items[0].ProductId);
            Assert.AreEqual(initialQuantity + additionalQuantity, cart.Items[0].Quantity);
            Assert.AreEqual(_testProduct.Price * (initialQuantity + additionalQuantity), cart.Total);
        }

        [TestMethod]
        public void AddToCart_NonExistentProduct_ShouldReturnFalse()
        {
            // Arrange
            int nonExistentProductId = -1;

            // Act
            bool result = _cartService.AddToCart(_testUserId, nonExistentProductId);

            // Assert
            Assert.IsFalse(result);
            var cart = _cartService.GetCart(_testUserId);
            Assert.IsFalse(cart.Items.Any());
        }

        [TestMethod]
        public void UpdateCartItemQuantity_ShouldUpdateQuantity()
        {
            // Arrange
            int productId = _testProduct.Id;
            int initialQuantity = 1;
            int newQuantity = 3;

            // Добавляем товар в корзину
            _cartService.AddToCart(_testUserId, productId, initialQuantity);
            var cart = _cartService.GetCart(_testUserId);
            int itemId = cart.Items[0].Id;

            // Act
            bool result = _cartService.UpdateCartItemQuantity(_testUserId, itemId, newQuantity);
            cart = _cartService.GetCart(_testUserId);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, cart.Items.Count);
            Assert.AreEqual(newQuantity, cart.Items[0].Quantity);
            Assert.AreEqual(_testProduct.Price * newQuantity, cart.Total);
        }

        [TestMethod]
        public void UpdateCartItemQuantity_ZeroOrLess_ShouldRemoveItem()
        {
            // Arrange
            int productId = _testProduct.Id;
            int initialQuantity = 2;

            // Добавляем товар в корзину
            _cartService.AddToCart(_testUserId, productId, initialQuantity);
            var cart = _cartService.GetCart(_testUserId);
            int itemId = cart.Items[0].Id;

            // Act
            bool result = _cartService.UpdateCartItemQuantity(_testUserId, itemId, 0);
            cart = _cartService.GetCart(_testUserId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(cart.Items.Any());
            Assert.AreEqual(0, cart.Total);
        }

        [TestMethod]
        public void RemoveFromCart_ShouldRemoveItem()
        {
            // Arrange
            int productId = _testProduct.Id;
            
            // Добавляем товар в корзину
            _cartService.AddToCart(_testUserId, productId, 1);
            var cart = _cartService.GetCart(_testUserId);
            int itemId = cart.Items[0].Id;

            // Act
            bool result = _cartService.RemoveFromCart(_testUserId, itemId);
            cart = _cartService.GetCart(_testUserId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(cart.Items.Any());
            Assert.AreEqual(0, cart.Total);
        }

        [TestMethod]
        public void RemoveFromCart_NonExistentItem_ShouldReturnFalse()
        {
            // Arrange
            int nonExistentItemId = -1;

            // Act
            bool result = _cartService.RemoveFromCart(_testUserId, nonExistentItemId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ClearCart_ShouldRemoveAllItems()
        {
            // Arrange - добавляем несколько товаров в корзину
            _cartService.AddToCart(_testUserId, _testProduct.Id, 1);
            
            // Добавим еще один продукт
            var secondProduct = new Product
            {
                Title = "Второй тестовый продукт",
                Price = 5000,
                InStock = true
            };
            var result = _productService.AddProduct(secondProduct);
            _cartService.AddToCart(_testUserId, result.Product.Id, 2);
            
            // Проверяем, что корзина не пуста
            var cartBefore = _cartService.GetCart(_testUserId);
            Assert.IsTrue(cartBefore.Items.Any());
            Assert.AreEqual(2, cartBefore.Items.Count);

            // Act
            bool clearResult = _cartService.ClearCart(_testUserId);
            var cartAfter = _cartService.GetCart(_testUserId);

            // Assert
            Assert.IsTrue(clearResult);
            Assert.IsFalse(cartAfter.Items.Any());
            Assert.AreEqual(0, cartAfter.Total);
        }

        [TestMethod]
        public void CartEvent_ShouldFireWhenCartChanged()
        {
            // Arrange
            bool eventFired = false;
            _cartService.CartChanged += (sender, e) => { eventFired = true; };

            // Act
            _cartService.AddToCart(_testUserId, _testProduct.Id, 1);

            // Assert
            Assert.IsTrue(eventFired);
        }
    }
}